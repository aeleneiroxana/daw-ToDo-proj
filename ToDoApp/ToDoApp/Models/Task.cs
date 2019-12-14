using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using ToDoApp.Models.Enums;

namespace ToDoApp.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public TaskStatus Status { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public int? AssignedUserId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; }

        public virtual User User { get; set; }

        public virtual Project Project { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
    public class TaskDBContext : DbContext
    {
        public TaskDBContext() : base("DBConnectionString") { }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}