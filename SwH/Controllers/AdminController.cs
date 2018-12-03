using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwH.Models;
using SwH.Models.EF;
using BCrypt;
//Testing
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SwH.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private static SwHDbContext _db = new SwHDbContext();

        public IActionResult Register()
        {

            AdminViewModel model = new AdminViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(AdminViewModel model)
        {
            if (model != null)
            {
                User user = new User();
                user.Username = model.Username;
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Role = model.Role;

                if (BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    _db.Add(user);
                    _db.SaveChanges();
                    ViewData["Message"] = "New employee added";
                    ViewData["MsgType"] = "Positive";
                }
                else
                {
                    ViewData["Message"] = "Something went wrong";
                    ViewData["MsgType"] = "Error";
                }
            }
            return View();
        }

        [Authorize]
        public IActionResult Users()
        {
            var result = _db.User.Where(u => u.Role != "Admin")
                .Select(t => new {
                    t.Id,
                    t.Username,
                    t.FirstName,
                    t.LastName,
                    t.Role
                })
                .OrderBy(x => x.Id)
                .ToList();
            if (result != null) {
                ViewData["Users"] = result;
                if (result.Count() <= 0) {
                    ViewData["Message"] = "No users found";
                    ViewData["MsgType"] = "Info";
                }
                
            }
            return View();
        }
    }
}
