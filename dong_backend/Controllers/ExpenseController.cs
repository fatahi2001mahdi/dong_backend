using dong_backend.Data;
using dong_backend.DTOs;
using dong_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace dong_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private static readonly HashSet<string> _categories = new HashSet<string>
    {
        "Food",
        "Shopping",
        "Debt",
        "Transportation",
        "Vehicle",
        "House",
        "Entertainment",
        "Personal",
        "Healthcare",
        "Other"
    };

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("CreateExpense")]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDTO createExpenseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (createExpenseDto.Category is string category && !_categories.Contains(category))
                return BadRequest("Invalid category. Allowed categories are: " + string.Join(", ", _categories));

            var expense = new Expense
            {
                GroupId = createExpenseDto.GroupId,
                Category = createExpenseDto.Category,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                AddedAt = createExpenseDto.AddedAt,
                Title = createExpenseDto.Title,
                Description = createExpenseDto.Description,
                Amount = createExpenseDto.Amount
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            if (createExpenseDto.AddUserExpenses != null && createExpenseDto.AddUserExpenses.Any())
            {
                foreach (var addUserExpenseDTO in createExpenseDto.AddUserExpenses)
                {
                    var userExpense = new UserExpense
                    {
                        UserId = addUserExpenseDTO.UserId,
                        ExpenseId = expense.Id,
                        Share = addUserExpenseDTO.Share,
                        Status = 0
                    };

                    _context.UserExpenses.Add(userExpense);
                }
                await _context.SaveChangesAsync();
            }

            var response = new ExpenseResponseDTO
            {
                Id = expense.Id,
                CreatedBy = expense.CreatedBy,
                CreatedAt = expense.CreatedAt,
                AddedAt = expense.AddedAt,
                Title = expense.Title,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount
            };

            return Ok(response);
        }


        [Authorize]
        [HttpPut("UpdateExpenseById")]
        public async Task<IActionResult> UpdateExpenseById([FromBody] UpdateExpenseDTO updateExpenseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var expense = await _context.Expenses.Include(e => e.UserExpenses)
                                                 .FirstOrDefaultAsync(e => e.Id == updateExpenseDto.Id);

            if (expense == null)
            {
                return NotFound("Expense not found.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (expense.CreatedBy != userId)
            {
                return Unauthorized("You do not have permission to update this expense.");
            }

            if (updateExpenseDto.Category is string category && !_categories.Contains(category))
                return BadRequest("Invalid category. Allowed categories are: " + string.Join(", ", _categories));

            expense.Category = updateExpenseDto.Category;
            expense.CreatedBy = userId;
            expense.AddedAt = updateExpenseDto.AddedAt;
            expense.Title = updateExpenseDto.Title;
            expense.Description = updateExpenseDto.Description;
            expense.Amount = updateExpenseDto.Amount;

            if (updateExpenseDto.UpdateUserExpenses != null && updateExpenseDto.UpdateUserExpenses.Any())
            {
                foreach (var updateUserExpenseDTO in updateExpenseDto.UpdateUserExpenses)
                {
                    var existingUserExpense = expense.UserExpenses.FirstOrDefault(ue => ue.UserId == updateUserExpenseDTO.UserId);

                    if (existingUserExpense != null)
                    {
                        existingUserExpense.Share = updateUserExpenseDTO.Share;
                    }
                    else
                    {
                        var newUserExpense = new UserExpense
                        {
                            UserId = updateUserExpenseDTO.UserId,
                            ExpenseId = expense.Id,
                            Share = updateUserExpenseDTO.Share,
                            Status = 0
                        };
                        _context.UserExpenses.Add(newUserExpense);
                    }
                }

                var updatedUserIds = updateExpenseDto.UpdateUserExpenses.Select(ue => ue.UserId).ToList();
                var userExpensesToRemove = expense.UserExpenses.Where(ue => !updatedUserIds.Contains(ue.UserId)).ToList();

                _context.UserExpenses.RemoveRange(userExpensesToRemove);
            }

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();

            var response = new ExpenseResponseDTO
            {
                Id = expense.Id,
                CreatedBy = expense.CreatedBy,
                CreatedAt = expense.CreatedAt,
                AddedAt = expense.AddedAt,
                Title = expense.Title,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount
            };

            return Ok(response);
        }


        [Authorize]
        [HttpDelete("DeleteExpenseById/{id}")]
        public async Task<IActionResult> DeleteExpenseById(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (expense.CreatedBy != userId)
            {
                return Unauthorized("You do not have permission to delete this expense.");
            }

            if (expense == null)
            {
                return NotFound("Expense not found.");
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return Ok("Expense deleted successfully.");
        }

        [Authorize]
        [HttpGet("GetExpenseById/{id}")]
        public async Task<IActionResult> GetExpenseById(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound("Expense not found.");
            }

            var response = new ExpenseResponseDTO
            {
                Id = expense.Id,
                CreatedBy = expense.CreatedBy,
                CreatedAt = expense.CreatedAt,
                AddedAt = expense.AddedAt,
                Title = expense.Title,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAllExpenses")]
        public async Task<IActionResult> GetAllExpenses()
        {
            var expenses = await _context.Expenses.ToListAsync();

            var response = expenses.Select(expense => new ExpenseResponseDTO
            {
                Id = expense.Id,
                CreatedBy = expense.CreatedBy,
                CreatedAt = expense.CreatedAt,
                AddedAt = expense.AddedAt,
                Title = expense.Title,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount
            });

            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAllUserExpenses")]
        public async Task<IActionResult> GetAllUserExpenses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var expenses = await _context.Expenses
                .Where(e => e.CreatedBy == userId && e.GroupId == null)
                .ToListAsync();

            if (expenses == null || !expenses.Any())
            {
                return NotFound("No expenses found for this user.");
            }

            var response = expenses.Select(expense => new ExpenseResponseDTO
            {
                Id = expense.Id,
                CreatedBy = expense.CreatedBy,
                CreatedAt = expense.CreatedAt,
                AddedAt = expense.AddedAt,
                Title = expense.Title,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount
            });

            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAllExpenseUsers/{expenseId}")]
        public async Task<IActionResult> GetAllExpenseUsers(int expenseId)
        {
            var usersInExpense = await _context.UserExpenses
                .Where(ue => ue.ExpenseId == expenseId)
                .Select(ue => ue.User)
                .ToListAsync();

            if (usersInExpense == null || !usersInExpense.Any())
            {
                return NotFound();
            }

            var response = usersInExpense.Select(user => new UserResponseDTO
            {
                Id = user.Id,
                RegisteredAt = user.RegisteredAt,
                Name = user.Name,
                Email = user.Email,
            });

            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        [Route("PayExpense/{expenseId}")]
        public async Task<IActionResult> PayExpense(int expenseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userExpense = await _context.UserExpenses
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExpenseId == expenseId);

            if (userExpense == null)
            {
                return NotFound("Expense not found for this user.");
            }

            userExpense.Status = 1;

            _context.UserExpenses.Update(userExpense);
            await _context.SaveChangesAsync();

            return Ok("Expense has been marked as paid.");
        }

        [Authorize]
        [HttpGet]
        [Route("CheckExpenseCreatedByUser/{expenseId}")]
        public async Task<IActionResult> CheckExpenseCreatedByUser(int expenseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == expenseId && e.CreatedBy == userId);

            if (expense == null)
            {
                return NotFound("Expense not found or not created by the user.");
            }

            return Ok("User has created this expense.");
        }

        [Authorize]
        [HttpPost("GetExpensesSummary")]
        public async Task<IActionResult> GetExpensesSummary(ExpenseSummaryInputDTO expenseSummaryInputDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (expenseSummaryInputDTO.StartDate > expenseSummaryInputDTO.EndDate)
                return BadRequest("StartDate cannot be after EndDate");

            var expensesSummary = new List<ExpenseSummaryResponseDTO>();

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@StartDate", expenseSummaryInputDTO.StartDate),
                new SqlParameter("@EndDate", expenseSummaryInputDTO.EndDate)
            };

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "GetUserExpensesByPeriod";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddRange(parameters);

                await _context.Database.OpenConnectionAsync();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        expensesSummary.Add(new ExpenseSummaryResponseDTO
                        {
                            Period = result.GetDateTime(0),
                            TotalExpenses = result.GetDecimal(1)
                        });
                    }
                }
            }

            return Ok(expensesSummary);
        }

    }
}
