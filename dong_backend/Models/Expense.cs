using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dong_backend.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string CreatedBy { get; set; }

        public User User { get; set; }

        [ForeignKey("Group")]
        public string? GroupId { get; set; }

        public Group Group { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime AddedAt { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Category { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public ICollection<UserExpense> UserExpenses { get; set; }
    }
}
