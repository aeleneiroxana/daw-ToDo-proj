using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ToDoApp.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Project> Projects { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }

        public virtual ICollection<Team> Teams { get; set; }

    }

    public class UserDBContext : DbContext
    {
        public UserDBContext() : base("DBConnectionString") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Team> Teams { get; set; }
    }
}