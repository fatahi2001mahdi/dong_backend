using System.ComponentModel.DataAnnotations;

namespace dong_backend.DTOs
{
    public class UserInviteDTO
    {
        [EmailAddress]
        public string Email { get; set; }
        public string GroupId { get; set; }

    }
}
