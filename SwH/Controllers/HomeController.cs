using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwH.Models;
using SwH.Models.EF;

//Testing
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace SwH.Controllers
{
    public class HomeController : Controller
    {
        private static SwHDbContext _db = new SwHDbContext(); // tässä luotiin ef olio

        [Authorize]
        public IActionResult Index()
        {
            var userName = HttpContext.User.Identity.Name;
            var adminRole = HttpContext.User.IsInRole("Admin");
            var headRole = HttpContext.User.IsInRole("Head");
            var managerRole = HttpContext.User.IsInRole("Manager");
            
            var user = _db.User.Where(x => x.Username == userName)
                        .Select(u => new { u.FirstName, u.LastName }).FirstOrDefault();
            ViewData["User"] = user.FirstName + " " + user.LastName;

            if (!adminRole) {
                if (!headRole && !managerRole)
                {
                    //Task preview
                    var myTasks = _db.Tasks
                        .Join(_db.User.Where(u => u.Username == userName),
                            t1 => t1.UserId,
                            u => u.Id,
                            (t1, u) => new { Task = t1, User = u })
                        .Join(_db.Project, 
                            t2 => t2.Task.ProjectId,
                            p => p.Id,
                            (t2, p) => new { t2.Task, Project = p.Name })
                        .Select(t => new {
                            t.Task.Id,
                            t.Task.Name,
                            t.Project,
                            t.Task.Descr,
                            t.Task.Status,
                            t.Task.Expires
                        })
                        .Where(t => t.Status != "Done")
                        .OrderBy(x => x.Expires)
                        .Take(10)
                        .ToList();
                    if (myTasks != null) {
                        ViewData["MyTasks"] = myTasks;
                        if (myTasks.Count() <= 0) {
                            ViewData["Message"] = "No tasks found";
                            ViewData["MsgType"] = "Info";
                        }
                    }
                }
                //Project preview
                if (headRole)
                {
                    var allProjects = _db.Project
                        .Select(t => new {
                            t.Name,
                            t.Manager,
                            t.Expires
                        })
                        .OrderBy(x => x.Name)
                        .Take(5)
                        .ToList();
                    if (allProjects != null) {
                        ViewData["Projects"] = allProjects;
                        if (allProjects.Count() <= 0) {
                            ViewData["Message"] = "No projects found";
                            ViewData["MsgType"] = "Info";
                        }
                    }
                } else if (managerRole) {
                    var myProjects = _db.Project
                        .Join(_db.User.Where(u => u.Username == userName),
                            p => p.Manager,
                            u => u.Id,
                            (p, u) => new { Project = p, User = u, Manager = (u.FirstName + " " + u.LastName) })
                        .Select(t => new {
                            t.Project.Name,
                            t.Manager,
                            t.Project.Expires
                        })
                        .OrderBy(x => x.Name)
                        .Take(5)
                        .ToList();
                    if (myProjects != null) {
                        ViewData["Projects"] = myProjects;
                        if (myProjects.Count() <= 0) {
                            ViewData["Message"] = "No projects found";
                            ViewData["MsgType"] = "Info";
                        }
                    }
                } else {
                    var projects = _db.ProjectTeam
                        .Join(_db.User.Where(u => u.Username == userName),
                            pt => pt.UserId,
                            u => u.Id,
                            (pt, u) => new { ProjectTeam = pt, User = u })
                        .Join(_db.Project, 
                            pt2 => pt2.ProjectTeam.ProjectId,
                            p => p.Id,
                            (pt2, p) => new { pt2.ProjectTeam, Project = p })
                        .Join(_db.User, 
                            pt3 => pt3.Project.Manager,
                            m => m.Id,
                            (pt3, m) => new { pt3.ProjectTeam, pt3.Project, Manager = (m.FirstName + " " + m.LastName) })
                        .Select(t => new {
                            t.Project.Name,
                            t.Manager,
                            t.Project.Expires
                        })
                        .OrderBy(x => x.Name)
                        .Take(5)
                        .ToList();
                    if (projects != null) {
                        ViewData["Projects"] = projects;
                        if (projects.Count() <= 0) {
                            ViewData["Message"] = "No projects found";
                            ViewData["MsgType"] = "Info";
                        }
                    }
                }
            }
            return View();     
        }

        [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
