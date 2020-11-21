using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoApp.Models
{
    public class UserToTeam
    {
        [Key, Column(Order = 0)]
        [Required]
        public int TeamId { get; set; }

        [Key, Column(Order = 1)]
        [Required]
        public string UserId { get; set; }

        public virtual Team Team { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}