using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dong_backend.Models
{
    public class UserExpense
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        [ForeignKey("Expense")]
        public int ExpenseId { get; set; }
        public Expense Expense { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Share must be a percentage between 0 and 100.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Share { get; set; }

        [Required]
        public int Status { get; set; }  //0 not_payed // 1 payed //not_users
    }
}
