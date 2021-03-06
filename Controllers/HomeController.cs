﻿using System;
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
            Wrapper wrapper = new Wrapper()
            {
                CurrentUser=dbContext.Users.Include(t => t.Trends).FirstOrDefault(u => u.UserId == (int)LoggedInUserID),
                AllGroups=dbContext.Groups.Include(m => m.Members).ThenInclude(u => u.User).ThenInclude(t => t.Trends).Where(m => m.Members.Any(u => u.UserId == (int)LoggedInUserID && u.AcceptedToGroup == true)).ToList(),
                GroupsNotIn=dbContext.Groups.Where(m => !m.Members.Any(u => u.UserId == (int)LoggedInUserID)).ToList()
            };
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
            int TrendNumber = 0;
            if(wrapper.LastWeekTrend != null)
            {
                TrendNumber = 1;
                wrapper.CurrentTrend = wrapper.LastWeekTrend;
            }
            if(wrapper.CurrentTrend.BigSpike + wrapper.CurrentTrend.SmallSpike + wrapper.CurrentTrend.Fluctuating + wrapper.CurrentTrend.Decreasing != 100){
                if(TrendNumber == 1)
                {
                    ModelState.AddModelError("LastWeekTrend.BigSpike","Trends should add up to 100%");
                }
                else
                {
                    ModelState.AddModelError("CurrentTrend.BigSpike","Trends should add up to 100%");
                }
            }
            if(ModelState.IsValid){
                User CurrentUser = dbContext.Users.Include(t => t.Trends).FirstOrDefault(u => u.UserId == (int)LoggedInUserID);
                int ThisTrendId = CurrentUser.Trends[TrendNumber].TrendId;
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
                if(TrendToChange.BigSpike == 100 || TrendToChange.SmallSpike == 100 || TrendToChange.Fluctuating == 100 || TrendToChange.Decreasing == 100)
                {
                    TrendToChange.KnownTrend = true;
                }
                else
                {
                    TrendToChange.KnownTrend = false;
                }
                dbContext.Update(TrendToChange);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
        }
		[HttpGet("NewGroup")]
        public IActionResult NewGroup()
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
            Group NewGroup = dbContext.Groups.FirstOrDefault(g => g.Name == wrapper.MakeGroup.Name);
            if(NewGroup != null)
            {
                ModelState.AddModelError("MakeGroup.Name", "This group name is already taken. Try another one!");
            }
            NewGroup = wrapper.MakeGroup;
            NewGroup.UserId = (int)LoggedInUserID;
            if(ModelState.IsValid)
            {
                dbContext.Groups.Add(NewGroup);
                dbContext.SaveChanges();
                Membership NewMembership = new Membership(){UserId = (int)LoggedInUserID, GroupId = NewGroup.GroupId, AcceptedToGroup = true};
                dbContext.Memberships.Add(NewMembership);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
        }
		[HttpPost("JoinGroup")]
        public IActionResult JoinGroup(Wrapper wrapper)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            User CurrentUser = dbContext.Users.FirstOrDefault(u => u.UserId == (int)LoggedInUserID);
            Group GroupToAdd = dbContext.Groups.Include(m => m.Members).FirstOrDefault(g => g.Name == wrapper.JoinGroup.Name);
            Membership TestMembership = dbContext.Memberships.FirstOrDefault(m => m.GroupId == GroupToAdd.GroupId && m.UserId == (int)LoggedInUserID);
            if(GroupToAdd==null)
            {
                ModelState.AddModelError("JoinGroup.Name","This group doesn't exist!");
            }
            else if(TestMembership != null)
            {
                if(TestMembership.AcceptedToGroup== false)
                {
                    ModelState.AddModelError("JoinGroup.Name","You've already applied to this group. You are awaiting membership approval.");
                }
                else
                {
                    ModelState.AddModelError("JoinGroup.Name","You're already in this group. Try joining another one!");
                }
            }
            if(ModelState.IsValid)
            {
                Membership NewMembership = new Membership{UserId = (int)LoggedInUserID, GroupId = GroupToAdd.GroupId};
                if(GroupToAdd.NeedsMembershipApproval == false)
                {
                    NewMembership.AcceptedToGroup = true;
                }
                dbContext.Memberships.Add(NewMembership);
                dbContext.SaveChanges();
            }
            return Dashboard();
        }
		[HttpPost("LeaveGroup")]
        public IActionResult LeaveGroup(Wrapper wrapper)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            Membership MembershipToDelete = dbContext.Memberships.FirstOrDefault(m => m.GroupId == wrapper.LeaveGroup.GroupId && m.UserId == (int)LoggedInUserID);
            Group GroupToDelete = dbContext.Groups.FirstOrDefault(g => g.GroupId == wrapper.LeaveGroup.GroupId);
            if(MembershipToDelete == null)
            {
                ModelState.AddModelError("LeaveGroup.Name","You can't leave a group you're not in");
            }
            if(ModelState.IsValid)
            {
                if(GroupToDelete.UserId == (int)LoggedInUserID)
                {
                    dbContext.Groups.Remove(GroupToDelete);
                }
                dbContext.Memberships.Remove(MembershipToDelete);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
        }
		[HttpGet("TransferOwnership/{GroupId}/{NewOwnerId}")]
        public IActionResult TransferOwnership(int GroupId, int NewOwnerId)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            Group TransferredGroup = dbContext.Groups.FirstOrDefault(g => g.GroupId == GroupId);
            User NewOwner = dbContext.Users.FirstOrDefault(u => u.UserId == NewOwnerId);
            Membership MembershipToCheck = dbContext.Memberships.FirstOrDefault(m => m.UserId == NewOwnerId && m.GroupId == GroupId);
            if(TransferredGroup == null)
            {
                ModelState.AddModelError("CurrentGroup.Name","This group doesn't exist!");
            }
            else if(TransferredGroup.UserId != (int)LoggedInUserID)
            {
                ModelState.AddModelError("CurrentGroup.Name","This isn't your group to give away");
            }
            if(NewOwner == null)
            {
                ModelState.AddModelError("CurrentGroup.UserId","This user doesn't exist!");
            }
            else if(MembershipToCheck == null)
            {
                ModelState.AddModelError("CurrentGroup.UserId","This user isn't a part of that group!");
            }
            if(ModelState.IsValid)
            {
                TransferredGroup.UserId = NewOwner.UserId;
                TransferredGroup.Owner = NewOwner;
                dbContext.Update(TransferredGroup);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
        }
		[HttpPost("EditGroup")]
        public IActionResult EditGroup(Wrapper wrapper)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            Group GroupToEdit = dbContext.Groups.FirstOrDefault(g => g.GroupId == wrapper.CurrentGroup.GroupId);
            if(GroupToEdit == null)
            {
                ModelState.AddModelError("CurrentGroup.Name","This group doesn't exist!");
            }
            else if(GroupToEdit.UserId != (int)LoggedInUserID)
            {
                ModelState.AddModelError("CurrentGroup.Name","You aren't the owner of this group.");
            }
            else if(dbContext.Groups.FirstOrDefault(g => g.Name == wrapper.CurrentGroup.Name && g.GroupId != wrapper.CurrentGroup.GroupId) != null)
            {
                ModelState.AddModelError("CurrentGroup.Name","A group with this name already exists. Try another one!");
            }
            if(ModelState.IsValid)
            {
                GroupToEdit.Name = wrapper.CurrentGroup.Name;
                GroupToEdit.NeedsMembershipApproval = wrapper.CurrentGroup.NeedsMembershipApproval;
                dbContext.Update(GroupToEdit);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
        }
		[HttpPost("IslandLink")]
        public IActionResult IslandLink(Wrapper wrapper)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            User LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == (int)LoggedInUserID);
            LoggedInUser.IslandLink = wrapper.CurrentUser.IslandLink;
            dbContext.Update(LoggedInUser);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }
		[HttpGet("AcceptOrReject/{MemberId}/{GroupId}/{Acceptance}")]
        public IActionResult AcceptOrReject(int MemberId, int GroupId, bool Acceptance)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            Group GroupToAccept = dbContext.Groups.FirstOrDefault(g => g.GroupId == GroupId);
            Membership MembershipInQuestion = dbContext.Memberships.FirstOrDefault(m => m.UserId == MemberId && m.GroupId == GroupId && m.AcceptedToGroup == false);
            if(GroupToAccept == null)
            {
                ModelState.AddModelError("CurrentGroup.Name","This group doesn't exist!");
            }
            else if(GroupToAccept.UserId != (int)LoggedInUserID)
            {
                ModelState.AddModelError("CurrentGroup.Name","You're not the leader of this group!");
            }
            if(MembershipInQuestion == null)
            {
                ModelState.AddModelError("CurrentGroup.Name","This user is not awaiting membership to this group.");
            }
            if(ModelState.IsValid)
            {
                if(Acceptance == true)
                {
                    MembershipInQuestion.AcceptedToGroup = true;
                    dbContext.Update(MembershipInQuestion);
                }
                else if(Acceptance == false)
                {
                    dbContext.Memberships.Remove(MembershipInQuestion);
                }
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
        }
		[HttpGet("RemoveMember/{GroupId}/{UserId}")]
        public IActionResult RemoveMember(int GroupId, int UserId)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            Membership MembershipToRemove = dbContext.Memberships.FirstOrDefault(m => m.UserId == UserId && m.GroupId == GroupId);
            Group GroupToRemoveFrom = dbContext.Groups.FirstOrDefault(g => g.GroupId == GroupId);
            if(GroupToRemoveFrom == null)
            {
                ModelState.AddModelError("CurrentGroup.Name","This group doesn't exist");
            }
            else if(MembershipToRemove == null)
            {
                ModelState.AddModelError("CurrentGroup.Name","This member is not in this group");
            }
            else if(GroupToRemoveFrom.UserId != (int)LoggedInUserID)
            {
                ModelState.AddModelError("CurrentGroup.Name","You are not the leader of this group. You can't remove members from it.");
            }
            else if(UserId == (int)LoggedInUserID)
            {
                ModelState.AddModelError("CurrentGroup.Name","The group must have a leader. Please either transfer ownership or delete the group before removing yourself");
            }
            if(ModelState.IsValid)
            {
                dbContext.Memberships.Remove(MembershipToRemove);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
        }
		[HttpPost("EditUser")]
        public IActionResult EditUser(Wrapper wrapper)
        {
            int? LoggedInUserID = HttpContext.Session.GetInt32("LoggedInUserID");
            if(LoggedInUserID==null)
            {
                return RedirectToAction("Index");
            }
            User UserToEdit = dbContext.Users.FirstOrDefault(u => u.UserId == (int)LoggedInUserID);
            UserToEdit.FirstName = wrapper.CurrentUser.FirstName;
            UserToEdit.LastName = wrapper.CurrentUser.LastName;
            UserToEdit.Email = wrapper.CurrentUser.Email;
            UserToEdit.Password = wrapper.CurrentUser.Password;
            if(dbContext.Users.FirstOrDefault(u => u.Email == UserToEdit.Email && u.UserId != UserToEdit.UserId) != null)
            {
                ModelState.AddModelError("CurrentUser.Email","This email address is already being used");
            }
            if(ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                UserToEdit.Password = Hasher.HashPassword(UserToEdit, UserToEdit.Password);
                dbContext.Update(UserToEdit);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return Dashboard();
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
                Trend LastWeekTrend = new Trend();
                CurrentTrend.TrendOwner = user;
                LastWeekTrend.TrendOwner = user;
                dbContext.Add(CurrentTrend);
                dbContext.Add(LastWeekTrend);
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
