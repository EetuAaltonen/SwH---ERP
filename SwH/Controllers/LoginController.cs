using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwH.Models;
using SwH.Models.EF;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;



namespace SwH.Controllers
{
    public class LoginController : Controller
    {
        private static SwHDbContext _db = new SwHDbContext();

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserCredentials UC)
        {
            if (!Valid(UC.username, UC.password))
            {
                ViewData["Message"] = "Wrong username/password";
                ViewData["MsgType"] = "Error";
                return View();
            }

            var result = _db.User.Where(x => x.Username == UC.username)
                        .Select(x => new { x.Id, x.Role}).FirstOrDefault();
            Int32 id = result.Id;
            string role = result.Role;

            if (role == null || role == "")
            {
                role = "User";
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, UC.username),
                new Claim(ClaimTypes.Role, role)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");
             

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                    scheme: "SwH",
                    principal: principal);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(LoginViewModel model)
        {
            if (model != null)
            {
                if (model.NewPassword == model.ConfPassword)
                {
                    var username = HttpContext.User.Identity.Name;
                    var result = _db.User.Where(x => x.Username == username)
                            .Select(x => new { x.Password }).FirstOrDefault();

                    if (BCrypt.Net.BCrypt.Verify(model.OldPassword, result.Password))
                    {
                        //Check that new password isn't same as old one
                        if (!BCrypt.Net.BCrypt.Verify(model.NewPassword, result.Password))
                        {
                            try
                            {
                                var newPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                                var user = _db.User.Where(x => x.Username == username).FirstOrDefault();
                                user.Password = newPassword;
                                _db.SaveChanges();

                                ViewData["Message"] = "Password changed";
                                ViewData["MsgType"] = "Positive";
                            } catch (Exception ex)
                            {
                                ViewData["Message"] = "Something went wrong";
                                ViewData["MsgType"] = "Error";
                            }
                        } else {
                            ViewData["Message"] = "New password can't be same as old one";
                            ViewData["MsgType"] = "Error";
                        }
                    } else {
                        ViewData["Message"] = "Wrong old password";
                        ViewData["MsgType"] = "Error";
                    }
                } else {
                    ViewData["Message"] = "New passwords doesn't match";
                    ViewData["MsgType"] = "Error";
                }
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                    scheme: "SwH");

            return RedirectToAction("Index");
        }

        private bool Valid(string username, string password)
        {
            User user = _db.User.Where(x => x.Username == username).FirstOrDefault();
            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    return true;
                }
            }

            return false;
        }

        [AllowAnonymous]
        public IActionResult Denied()
        {
            return View();
        }
    }
}