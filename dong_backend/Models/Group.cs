using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dong_backend.Models
{
    public class Group
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string Owner { get; set; }
        public User User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<UserGroup> UserGroups { get; set; }
    }
}
