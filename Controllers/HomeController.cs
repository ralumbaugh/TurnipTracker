using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TurnipTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace TurnipTracker.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login(LoginWrapper WrappedUser)
        {
            LoginUser user = WrappedUser.LoginUser;
            if(ModelState.IsValid)
            {
                User UserInDb = dbContext.Users.FirstOrDefault(u=> u.Email == user.Email);
                if(UserInDb == null)
                {
                    ModelState.AddModelError("LoginUser.Email", "The email/password combination is incorrect.");
                    return View("Index");
                }
                PasswordHasher<LoginUser> Hasher = new PasswordHasher<LoginUser>();
                var result = Hasher.VerifyHashedPassword(user, UserInDb.Password, user.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("LoginUser.Email", "The email/password combination is incorrect.");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("LoggedInUserID", UserInDb.UserId);
                return RedirectToAction("LoggedIn");
            }
            else
            {
                return View("Index");
            }
        }

        public IActionResult Register(LoginWrapper WrappedUser)
        {
            User user = WrappedUser.NewUser;
            if(dbContext.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("NewUser.Email", "Email already in use!");
            }
            if(ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("LoggedInUserID", user.UserId);
                return RedirectToAction("LoggedIn");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
