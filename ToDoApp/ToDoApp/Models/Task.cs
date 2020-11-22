using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ToDoApp.Models.Enums;

namespace ToDoApp.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [Required]
        public TaskStatus Status { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public string AssignedUserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual Project Project { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}