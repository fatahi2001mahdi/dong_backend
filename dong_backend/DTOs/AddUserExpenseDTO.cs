using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace dong_backend.DTOs
{
    public class AddUserExpenseDTO
    {
        [Required]
        public string UserId { get; set; }

        [Range(0, 100, ErrorMessage = "Share must be a percentage between 0 and 100.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Share { get; set; }

        [Required]
        public int Status { get; set; }
    }
}
