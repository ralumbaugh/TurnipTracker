using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnipTracker.Models
{
    public class Wrapper
    {
        public User CurrentUser {get; set;}
        public Group CurrentGroup {get; set;}
        public LoginUser LoginUser {get; set;}
        public Group JoinGroup {get; set;}
        public Group MakeGroup {get; set;}
        public Group LeaveGroup {get; set;}
        public Trend CurrentTrend {get; set;}
        public Trend LastWeekTrend {get; set;}
        public List<User> AllUsers {get; set;}
        public List<Group> AllGroups {get; set;}
        public List<Group> GroupsNotIn {get; set;}
        public List<Membership> AllMemberships {get; set;}
    }
}