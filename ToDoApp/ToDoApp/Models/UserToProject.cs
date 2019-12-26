using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ToDoApp.Models
{
    public class UserToProject
    {
        [Key, Column(Order = 0)]
        public int ProjectId { get; set; }

        [Key, Column(Order = 1)]
        public string UserId { get; set; }

        public virtual Project Project { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

}