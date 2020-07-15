using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnipTracker.Models
{
    public class Group
    {
    [Key]
    public int GroupId {get; set;}
    public string Name {get; set;}
    public int UserId {get; set;}
    public User Owner {get; set;}
    public List<User> Members {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}