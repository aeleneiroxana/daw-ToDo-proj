using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace ToDoApp.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        public string Title { get; set; }

        public string UserId { get; set; }

        public int TeamId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual Team Team { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }


    public class ProjectDBContext : DbContext
    {
        public ProjectDBContext() : base("DBConnectionString") { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Tasks { get; set; }
    }
}