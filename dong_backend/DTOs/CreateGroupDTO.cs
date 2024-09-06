using System.ComponentModel.DataAnnotations;

namespace dong_backend.DTOs
{
    public class CreateGroupDTO
    {
        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
