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
        [HttpGet("")]
        // [Route("")]
        public IActionResult Index()
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }
		[HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            Wrapper wrapper = new Wrapper();
            wrapper.CurrentUser=dbContext.Users.Include(m => m.Groups).ThenInclude(g => g.Group).Include(c => c.Trends).FirstOrDefault(u => u.UserId == (int)LoggedInUserID);
            wrapper.AllUsers=dbContext.Users.ToList();
            return View("Dashboard", wrapper);
        }
		[HttpPost("UpdatePrices")]
        public IActionResult UpdatePrices(Wrapper wrapper)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            if(wrapper.CurrentTrend.BigSpike + wrapper.CurrentTrend.SmallSpike + wrapper.CurrentTrend.Fluctuating + wrapper.CurrentTrend.Decreasing > 1){
                ModelState.AddModelError("CurrentTrend.BigSpike","Trends should not be greater than 100%");
            }
            if(ModelState.IsValid){
                User CurrentUser = dbContext.Users.Include(t => t.Trends).FirstOrDefault(u => u.UserId == (int)LoggedInUserID);
                int ThisTrendId = CurrentUser.Trends[0].TrendId;
                Trend TrendToChange = dbContext.Trends.FirstOrDefault(t => t.TrendId == ThisTrendId);
                TrendToChange.BuyPrice = wrapper.CurrentTrend.BuyPrice;
                TrendToChange.MonAM = wrapper.CurrentTrend.MonAM;
                TrendToChange.MonPM = wrapper.CurrentTrend.MonPM;
                TrendToChange.TueAM = wrapper.CurrentTrend.TueAM;
                TrendToChange.TuePM = wrapper.CurrentTrend.TuePM;
                TrendToChange.WedAM = wrapper.CurrentTrend.WedAM;
                TrendToChange.WedPM = wrapper.CurrentTrend.WedPM;
                TrendToChange.ThurAM = wrapper.CurrentTrend.ThurAM;
                TrendToChange.ThurPM = wrapper.CurrentTrend.ThurPM;
                TrendToChange.FriAM = wrapper.CurrentTrend.FriAM;
                TrendToChange.FriPM = wrapper.CurrentTrend.FriPM;
                TrendToChange.SatAM = wrapper.CurrentTrend.SatAM;
                TrendToChange.SatPM = wrapper.CurrentTrend.SatPM;
                TrendToChange.BigSpike = wrapper.CurrentTrend.BigSpike;
                TrendToChange.SmallSpike = wrapper.CurrentTrend.SmallSpike;
                TrendToChange.Fluctuating = wrapper.CurrentTrend.Fluctuating;
                TrendToChange.Decreasing = wrapper.CurrentTrend.Decreasing;
                TrendToChange.KnownTrend = wrapper.CurrentTrend.KnownTrend;
                dbContext.Update(TrendToChange);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
        }
		[HttpGet("NewGroup")]
        public IActionResult NewGroup(string GroupName)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            return View("NewGroup");
        }
		[HttpPost("MakeGroup")]
        public IActionResult MakeGroup(Wrapper wrapper)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            Group NewGroup = dbContext.Groups.FirstOrDefault(g => g.Name == wrapper.CurrentGroup.Name);
            if(NewGroup == null)
            {
                return RedirectToAction("Dashboard");
                
            }
            else
            {
                Console.WriteLine("Fucking party!");
                return RedirectToAction("Dashboard");
            }
        }
		// [HttpGet("ShowGroup/{GroupId}")]
        // public IActionResult ShowGroup(int GroupId)
        // {
        //     int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
        //     if(LoggedInUserID==null)
        //     {
        //         return RedirectToAction("Index");
        //     }
        //     Group CurrentGroup = dbContext.Groups.FirstOrDefault(g=> g.GroupId == GroupId);
        //     // User CurrentUser = dbContext.Users.Include(u=>u.Groups).FirstOrDefault(u=> u.UserId == (int)LoggedInUserID);
        //     if(CurrentGroup == null)
        //     {
        //         return Dashboard();
        //     }
        //     return View("IndividualGroup");
        // }
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
                return RedirectToAction("Dashboard");
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
                Trend CurrentTrend = new Trend();
                CurrentTrend.TrendOwner = user;
                dbContext.Add(CurrentTrend);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("LoggedInUserID", user.UserId);
                return RedirectToAction("Dashboard");
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
