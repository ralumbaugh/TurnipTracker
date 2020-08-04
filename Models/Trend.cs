using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnipTracker.Models
{
    public class Trend
    {
    [Key]
    public int TrendId {get; set;}
    public int UserId {get; set;}
    public User TrendOwner {get; set;}
    public int? BuyPrice {get; set;} = null;
    public int? MonAM {get; set;} = null;
    public int? MonPM {get; set;} = null;
    public int? TueAM {get; set;} = null;
    public int? TuePM {get; set;} = null;
    public int? WedAM {get; set;} = null;
    public int? WedPM {get; set;} = null;
    public int? ThurAM {get; set;} = null;
    public int? ThurPM {get; set;} = null;
    public int? FriAM {get; set;} = null;
    public int? FriPM {get; set;} = null;
    public int? SatAM {get; set;} = null;
    public int? SatPM {get; set;} = null;
    public float BigSpike {get; set;}
    public float SmallSpike {get; set;}
    public float Fluctuating {get; set;}
    public float Decreasing {get; set;}
    public bool KnownTrend {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}