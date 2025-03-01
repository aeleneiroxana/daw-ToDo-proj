﻿using System;
using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; }

        public virtual Task Task { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}