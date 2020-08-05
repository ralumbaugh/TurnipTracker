using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnipTracker.Models
{
    public class Wrapper
    {
        public User CurrentUser {get; set;}
        public LoginUser LoginUser {get; set;}
        public Group CurrentGroup {get; set;}
        public List<User> AllUsers {get; set;}
        public Trend CurrentTrend {get; set;}
    }
}