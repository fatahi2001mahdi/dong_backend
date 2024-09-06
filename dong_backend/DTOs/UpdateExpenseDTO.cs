using System;
using System.ComponentModel.DataAnnotations;

namespace dong_backend.DTOs
{
    public class UpdateExpenseDTO
    {
        [Required]
        public int Id { get; set; }

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

        public List<UpdateUserExpenseDTO>? UpdateUserExpenses { get; set; }
    }
}
