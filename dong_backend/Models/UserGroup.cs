using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dong_backend.Models
{
    public class UserGroup
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        [ForeignKey("Group")]
        public string GroupId { get; set; }
        public Group Group { get; set; }

        public DateTime? JoinedAt { get; set; }

        [Required]
        public int Status { get; set; }   //0 left, invitation declined  // 1 invitation accepted, created the group, joined via link //2 invitation pending

        public string? InvitedByEmail { get; set; }
    }
}
