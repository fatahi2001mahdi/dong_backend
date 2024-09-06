using System.ComponentModel.DataAnnotations;

namespace dong_backend.DTOs
{
    public class UpdateUserExpenseDTO
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Share must be a percentage between 0 and 100.")]
        public decimal Share { get; set; }

        [Required]
        public int Status { get; set; }
    }
}
