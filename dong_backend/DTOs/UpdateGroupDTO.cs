using System.ComponentModel.DataAnnotations;

namespace dong_backend.DTOs
{
    public class UpdateGroupDTO
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
