using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwH.Models;
using SwH.Models.EF;
using Microsoft.AspNetCore.Authorization;
using System.Dynamic;

namespace SwH.Controllers
{
    public class ProjectController : Controller
    {
        private static SwHDbContext _db = new SwHDbContext(); // tässä luotiin ef olio

        private List<User> GetEmployees()
        {
            var employees = _db.User
                .Where(u => !u.Role.Equals("Admin"))
                .Where(u => !u.Role.Equals("Manager"))
                .Where(u => !u.Role.Equals("Head"))
                .ToList();
            return employees;
        }
        private List<User> GetManagers()
        {
            var managers = _db.User
                .Where(u => u.Role.Equals("Manager"))
                .ToList();
            return managers;
        }
        private List<Project> GetAllProjects()
        {
            var projects = _db.Project.ToList();
            return projects;
        }
        
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult MyTasks()
        {
            var userName = HttpContext.User.Identity.Name;
            var result = _db.Tasks
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
                .ToList();
            if (result != null) {
                ViewData["MyTasks"] = result;
                if (result.Count() <= 0) {
                    ViewData["Message"] = "No tasks found";
                    ViewData["MsgType"] = "Info";
                }
            }
            return View();
        }

        [HttpPost]  
        public ActionResult ChangeTaskStatus([FromBody]MyTasksViewModel task)  
        {
            var jsonResult = new { Success = false, Message = "Something went wrong" };
            if (task != null)
            {
                if (task.Id != null && task.Status != null)
                {
                    var currentTask = _db.Tasks.Where(t => t.Id == Int32.Parse(task.Id)).First();
                    currentTask.Status = task.Status;
                    _db.SaveChanges();
                    jsonResult = new { Success = true, Message = "" };
                }
            }
            return Json(jsonResult);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult ManageTasks()
        {
            ManageTasksViewModel model = new ManageTasksViewModel();

            var userName = HttpContext.User.Identity.Name;
            
            var employees = GetEmployees();
            if (employees != null) {
                model.Employees = employees;
            }
            var projects = GetAllProjects();
            if (projects != null) {
                model.Projects = projects;
                
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public IActionResult ManageTasks(ManageTasksViewModel model) {
            if (model != null)
            {
                Tasks task = new Tasks();
                task.Name = model.TaskName;
                task.UserId = model.UserId;
                task.ProjectId = model.ProjectId;
                task.Descr = model.Descr;
                task.Status = model.Status;
                task.Expires = model.Expires;

                _db.Add(task);
                _db.SaveChanges();
                
                ViewData["Message"] = "New task added";
                ViewData["MsgType"] = "Positive";
            }

            var employees = GetEmployees();
            if (employees != null) {
                model.Employees = employees;
            }
            var projects = _db.Project.ToList();
            if (projects != null) {
                model.Projects = projects;
                return View(model);
            }

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult ManageProjectTeams()
        {
            ManageTasksViewModel model = new ManageTasksViewModel();

            var userName = HttpContext.User.Identity.Name;
            var projects = _db.Project
                .Join(_db.User.Where(u => u.Username == userName),
                    p => p.Manager,
                    u => u.Id,
                    (p, u) => new { Project = p, User = u })
                .Select(t => new {
                    t.Project.Id,
                    t.Project.Name
                })
                .OrderBy(x => x.Name)
                .ToList();
                if (projects != null) {
                    ViewData["Projects"] = projects;
                } else {
                    ViewData["Message"] = "No projects found";
                    ViewData["MsgType"] = "Info";
                }
            var teamMembers = _db.Project
                .Join(_db.User.Where(u => u.Username == userName),
                    p => p.Manager,
                    u => u.Id,
                    (p, u) => new { Project = p, User = u })
                .Join(_db.ProjectTeam,
                    p2 => p2.Project.Id,
                    pt => pt.ProjectId,
                    (p2, pt) => new { p2.Project, ProjectTeam = pt })
                .Join(_db.User,
                    pt2 => pt2.ProjectTeam.UserId,
                    u => u.Id,
                    (pt2, u) => new { ProjectId = pt2.Project.Id, Id = u.Id, FirstName = u.FirstName, LastName = u.LastName, Role = u.Role })
                .Select(t => new {
                    t.Id,
                    t.ProjectId,
                    t.FirstName,
                    t.LastName,
                    t.Role
                })
                .OrderBy(x => x.LastName)
                .ToList();
            if (teamMembers != null) {
                ViewData["TeamMembers"] = teamMembers;
            }
            var employees = _db.User
                .Where(u => !u.Role.Equals("Admin"))
                .Where(u => !u.Role.Equals("Manager"))
                .Where(u => !u.Role.Equals("Head"))
                .Where(u => !u.Username.Equals(userName))
                .Select(t => new {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Role
                })
                .ToList();

            if (employees != null) {
                ViewData["Employees"] = employees;
            }
            
            //Delete this later
            var tempEmployees = GetEmployees();
            if (tempEmployees != null) {
                model.Employees = tempEmployees;
            }
            var tempProjects = _db.Project.ToList();
            if (tempProjects != null) {
                model.Projects = tempProjects;
            }

            return View(model);
        }

        [HttpPost]  
        public ActionResult SaveProjectTeam([FromBody]ManageProjectTeamsViewModel projectTeam)  
        {
            var jsonResult = new { Success = false };
            if (projectTeam != null)
            {
                var projectId = projectTeam.ProjectId;
                if (projectTeam.ProjectTeam != null && projectTeam.Employees != null)
                {
                    var userInProject = new ProjectTeam();
                    foreach (User user in projectTeam.ProjectTeam)
                    {
                        userInProject = _db.ProjectTeam.Where(t => t.UserId == user.Id && t.ProjectId == projectId).FirstOrDefault();
                        if (userInProject == null)
                        {
                            //Assing to project
                            ProjectTeam addUser = new ProjectTeam();
                            addUser.UserId = user.Id;
                            addUser.ProjectId = projectId;
                            _db.Add(addUser);
                            _db.SaveChanges();
                        }

                    };
                    foreach (User user in projectTeam.Employees)
                    {
                        userInProject = _db.ProjectTeam.Where(t => t.UserId == user.Id && t.ProjectId == projectId).FirstOrDefault();
                        if (userInProject != null)
                        {
                            //Remove from project team
                            _db.Remove(userInProject);
                            _db.SaveChanges();
                        }

                    };
                    jsonResult = new { Success = true };
                }
            }
            //return updated orgEmployees and orgTeamMembers from database
            return Json(jsonResult);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public IActionResult ManageProjectTeams(ManageTasksViewModel model) {
            if (model != null)
            {
                ProjectTeam projectTeam = new ProjectTeam();
                projectTeam.UserId = model.UserId;
                projectTeam.ProjectId = model.ProjectId;

                _db.Add(projectTeam);
                _db.SaveChanges();

                ViewData["Message"] = "Employee assigned to project";
                ViewData["MsgType"] = "Positive";
            }

            var userName = HttpContext.User.Identity.Name;
            var projects = _db.Project
                .Join(_db.User.Where(u => u.Username == userName),
                    p => p.Manager,
                    u => u.Id,
                    (p, u) => new { Project = p, User = u })
                .Select(t => new {
                    t.Project.Id,
                    t.Project.Name
                })
                .OrderBy(x => x.Name)
                .ToList();
                if (projects != null) {
                    ViewData["Projects"] = projects;
                } else {
                    ViewData["Message"] = "No projects found";
                    ViewData["MsgType"] = "Info";
                }
            var teamMembers = _db.Project
                .Join(_db.User.Where(u => u.Username == userName),
                    p => p.Manager,
                    u => u.Id,
                    (p, u) => new { Project = p, User = u })
                .Join(_db.ProjectTeam,
                    p2 => p2.Project.Id,
                    pt => pt.ProjectId,
                    (p2, pt) => new { p2.Project, ProjectTeam = pt })
                .Join(_db.User,
                    pt2 => pt2.ProjectTeam.UserId,
                    u => u.Id,
                    (pt2, u) => new { ProjectId = pt2.Project.Id, Id = u.Id, FirstName = u.FirstName, LastName = u.LastName, Role = u.Role })
                .Select(t => new {
                    t.Id,
                    t.ProjectId,
                    t.FirstName,
                    t.LastName,
                    t.Role
                })
                .OrderBy(x => x.LastName)
                .ToList();
            if (teamMembers != null) {
                ViewData["TeamMembers"] = teamMembers;
            }
            var employees = _db.User
                .Where(u => !u.Role.Equals("Admin"))
                .Where(u => !u.Role.Equals("Manager"))
                .Where(u => !u.Role.Equals("Head"))
                .Where(u => !u.Username.Equals(userName))
                .Select(t => new {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Role
                })
                .ToList();

            if (employees != null) {
                ViewData["Employees"] = employees;
            }
            
            //Delete this later
            var tempEmployees = GetEmployees();
            if (tempEmployees != null) {
                model.Employees = tempEmployees;
            }
            var tempProjects = _db.Project.ToList();
            if (tempProjects != null) {
                model.Projects = tempProjects;
            }

            return View(model);
        }

        [Authorize]
        public IActionResult ProjectSummaries()
        {
            var userName = HttpContext.User.Identity.Name;
            var headRole = HttpContext.User.IsInRole("Head");
            var managerRole = HttpContext.User.IsInRole("Manager");
            if (headRole)
            {
                var projects = _db.Project.OrderBy(x => x.Name).ToList();
                if (projects != null) {
                    ViewData["Projects"] = projects;
                    if (projects.Count() <= 0) {
                        ViewData["Message"] = "No projects found";
                        ViewData["MsgType"] = "Info";
                    }
                }
                var tasks = _db.Tasks.OrderBy(x => x.Status).ToList();
                if (tasks != null) {
                    ViewData["Tasks"] = tasks;
                }
            } else if (managerRole)
            {
                var projects = _db.ProjectTeam
                    .Join(_db.Project, 
                        pt => pt.ProjectId,
                        p => p.Id,
                        (pt, p) => new { ProjectTeam = pt, Project = p })
                    .Join(_db.User.Where(u => u.Username == userName),
                        p2 => p2.Project.Manager,
                        u => u.Id,
                        (p2, u) => new { Projects = p2})
                    .Select(t => new {
                        t.Projects.Project.Id,
                        t.Projects.Project.Name
                    })
                    .OrderBy(x => x.Name)
                    .Distinct() //Eliminates duplicates
                    .ToList();
                if (projects != null) {
                    ViewData["Projects"] = projects;
                    if (projects.Count() <= 0) {
                        ViewData["Message"] = "No projects found";
                        ViewData["MsgType"] = "Info";
                    }
                }
                var managerId = _db.User.Where(x => x.Username == userName)
                        .Select(x => new { x.Id }).FirstOrDefault();
                var tasks = _db.Tasks
                    .Join(_db.Project,
                        t => t.ProjectId,
                        p => p.Id,
                        (t, p) => new { Task = t, Project = p })
                    .Join(_db.ProjectTeam,
                        p1 => p1.Project.Id,
                        pt => pt.Id,
                        (p1, pt) => new { p1.Project, p1.Task, ProjectTeam = pt })
                    .Join(_db.User.Where(u => u.Username == userName),
                        p2 => p2.Project.Manager,
                        u => u.Id,
                        (p2, u) => new { p2.Task, ProjectId = p2.Project.Id })
                    .Select(t => new {
                        t.Task.Name,
                        t.Task.Status,
                        t.ProjectId
                    })
                    .OrderBy(x => x.Status)
                    .ToList();
                if (tasks != null) {
                    ViewData["Tasks"] = tasks;
                }

            } else
            {
                var tasks = _db.Tasks
                    .Join(_db.User.Where(u => u.Username == userName),
                        t1 => t1.UserId,
                        u => u.Id,
                        (t1, u) => new { Task = t1, User = u })
                    .Join(_db.Project,
                        t2 => t2.Task.ProjectId,
                        p => p.Id,
                        (t2, p) => new { t2.Task, ProjectId = p.Id })
                    .Select(t => new {
                        t.Task.Name,
                        t.Task.Status,
                        t.ProjectId
                    })
                    .OrderBy(x => x.Status)
                    .ToList();
                if (tasks != null) {
                    ViewData["Tasks"] = tasks;
                    if (tasks.Count() <= 0) {
                        ViewData["Message"] = "No tasks found";
                        ViewData["MsgType"] = "Info";
                    }
                }
                var projects = _db.ProjectTeam
                    .Join(_db.User.Where(u => u.Username == userName),
                        pt => pt.UserId,
                        u => u.Id,
                        (pt, u) => new { ProjectTeam = pt, User = u })
                    .Join(_db.Project, 
                        pt2 => pt2.ProjectTeam.ProjectId,
                        p => p.Id,
                        (pt2, p) => new { pt2.ProjectTeam, Project = p })
                    .Select(t => new {
                        t.Project.Id,
                        t.Project.Name
                    })
                    .OrderBy(x => x.Name)
                    .ToList();
                if (projects != null) {
                    ViewData["Projects"] = projects;
                }
            }
            return View();
        }

        [Authorize]
        public IActionResult Projects()
        {
            var userName = HttpContext.User.Identity.Name;
            var headRole = HttpContext.User.IsInRole("Head");
            var managerRole = HttpContext.User.IsInRole("Manager");

            if (headRole) {
                var result = _db.Project
                    .Join(_db.User,
                        p => p.Manager,
                        u => u.Id,
                        (p, u) => new { Project = p, User = u, Manager = (u.FirstName + " " + u.LastName) })
                    .Select(t => new {
                        t.Project.Name,
                        t.Manager,
                        t.Project.Expires
                    })
                    .OrderBy(x => x.Name)
                    .ToList();
                if (result != null) {
                    ViewData["Projects"] = result;
                    if (result.Count() <= 0) {
                        ViewData["Message"] = "No projects found";
                        ViewData["MsgType"] = "Info";
                    }
                }

            } else if (managerRole) {
                var result = _db.Project
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
                    .ToList();
                if (result != null) {
                    ViewData["Projects"] = result;
                    if (result.Count() <= 0) {
                        ViewData["Message"] = "No projects found";
                        ViewData["MsgType"] = "Info";
                    }
                    return View();
                }
            } else {
                var result = _db.ProjectTeam
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
                    .ToList();
                if (result != null) {
                    ViewData["Projects"] = result;
                    if (result.Count() <= 0) {
                        ViewData["Message"] = "No projects found";
                        ViewData["MsgType"] = "Info";
                    }
                }
            }
            return View();
        }

        [Authorize(Roles = "Head")]
        public IActionResult ManageProjects()
        {
            ManageProjectsViewModel model = new ManageProjectsViewModel();

            var userName = HttpContext.User.Identity.Name; 
            try {
                var result = GetManagers();
                if (result != null) {
                    model.Managers = result;
                }
            } catch (Exception ex) {
                ViewData["Message"] = "Something went wrong";
                ViewData["MsgType"] = "Error";
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Head")]
        [ValidateAntiForgeryToken]
        public IActionResult ManageProjects(ManageProjectsViewModel model) {
            if (model != null)
            {
                Project project = new Project();
                project.Name = model.ProjectName;
                project.Manager = model.ManagerId;
                project.Expires = model.Expires;

                _db.Add(project);
                _db.SaveChanges();

                ViewData["Message"] = "New project added";
                ViewData["MsgType"] = "Positive";
            }

            var result = GetManagers();
            if (result != null) {
                model.Managers = result;
            }

            return View(model);
        }

        [Authorize(Roles = "Head")]
        public IActionResult Employees()
        {
            var result = _db.User.Where(u => u.Role != "Admin" && u.Role != "Head")
                .Select(t => new {
                    t.FirstName,
                    t.LastName,
                    t.Role
                })
                .OrderBy(x => x.LastName)
                .ToList();
            if (result != null) {
                ViewData["Employees"] = result;
                if (result.Count() <= 0) {
                    ViewData["Message"] = "No employees found";
                    ViewData["MsgType"] = "Info";
                }
                
            }
            return View();
        }
    }
}