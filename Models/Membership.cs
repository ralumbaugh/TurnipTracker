using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnipTracker.Models
{
    public class Membership
    {
    [Key]
    public int MembershipId {get; set;}
    public int UserId {get; set;}
    public User User {get; set;}
    public int GroupId {get; set;}
    public Group Group {get; set;}
    public bool AcceptedToGroup {get; set;} = false;
    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}