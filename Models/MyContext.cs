using Microsoft.EntityFrameworkCore;

namespace TurnipTracker.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users {get;set;}
        public DbSet<Group> Groups {get;set;}
        public DbSet<Membership> Memberships {get;set;}
    }
}