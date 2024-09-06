using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace dong_backend.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        
        public ICollection<Group> Groups { get; set; }  
        public ICollection<Expense> Expenses { get; set; }  

        public ICollection<UserExpense> UserExpenses { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; }
    }
}
