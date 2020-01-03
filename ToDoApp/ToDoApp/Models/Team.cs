using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ToDoApp.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        public string Title { get; set; }

        //[Required]
        public string UserId { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }
}