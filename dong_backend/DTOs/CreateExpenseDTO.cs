using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dong_backend.DTOs
{
    public class CreateExpenseDTO
    {
        public string? GroupId { get; set; }

        [Required]
        public DateTime AddedAt { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public string Category { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        public List<AddUserExpenseDTO>? AddUserExpenses { get; set; }
    }
}
