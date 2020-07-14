using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnipTracker.Models
{
    public class LoginUser
    {
    [Required (ErrorMessage="Email is Required")]
    [EmailAddress (ErrorMessage="Please enter a valid email address")]
    public string Email {get; set;}
    [Required (ErrorMessage="A Password is Required")]
    [DataType (DataType.Password)]
    [MinLength (8, ErrorMessage="Password must be at least 8 characters!")]
    [RegularExpression("^(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)(?=.[!@#$%^&*.])([a-zA-Z0-9!@#$%^&*.]+)$", ErrorMessage="Password must contain at least 1 letter, 1 number, and 1 special character !@#$%^&*.")]
    public string Password {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public DateTime UpdatedAt {get; set;} = DateTime.Now;
    }
}