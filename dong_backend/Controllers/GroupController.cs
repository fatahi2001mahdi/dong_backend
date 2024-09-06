using dong_backend.Data;
using dong_backend.DTOs;
using dong_backend.Models;
using dong_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace dong_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDTO createGroupDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            const int maxRetryCount = 5;
            int retryAttempt = 0;

            while (retryAttempt < maxRetryCount)
            {
                var groupId = GroupIdHelper.GenerateGroupId();
                var group = new Group
                {
                    Id = groupId,
                    Owner = userId,
                    CreatedAt = DateTime.UtcNow,
                    Name = createGroupDTO.Name,
                    Description = createGroupDTO.Description
                };

                var userGroup = new UserGroup
                {
                    UserId = userId,
                    GroupId = group.Id,
                    Status = 1
                };

                _context.Groups.Add(group);
                _context.UserGroups.Add(userGroup);

                try
                {
                    await _context.SaveChangesAsync();

                    var response = new GroupResponseDTO
                    {
                        Id = group.Id,
                        Owner = group.Owner,
                        CreatedAt = group.CreatedAt,
                        Name = group.Name,
                        Description = group.Description
                    };

                    return Ok(response);
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.Message.Contains("duplicate key value") == true)
                    {
                        retryAttempt++;

                        if (retryAttempt >= maxRetryCount)
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to create a unique group ID. Please try again.");
                        }

                        continue;
                    }

                    throw;
                }
            }

            return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error occurred.");
        }

        [Authorize]
        [HttpPut("UpdateGroupById")]
        public async Task<IActionResult> UpdateGroupById(UpdateGroupDTO updateGroupDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var group = await _context.Groups.FindAsync(updateGroupDTO.Id);
            if (group == null)
            {
                return NotFound("Group not found.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (group.Owner != userId)
            {
                return Unauthorized("You do not have permission to update this group.");
            }

            group.Name = updateGroupDTO.Name;
            group.Description = updateGroupDTO.Description;

            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            var response = new GroupResponseDTO
            {
                Id = group.Id,
                Owner = group.Owner,
                CreatedAt = group.CreatedAt,
                Name = group.Name,
                Description = group.Description
            };

            return Ok(response);
        }

        [Authorize]
        [HttpDelete("DeleteGroupById/{id}")]
        public async Task<IActionResult> DeleteGroupById(string id)
        {
            var group = await _context.Groups.FindAsync(id);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (group == null)
            {
                return NotFound("Group not found.");
            }

            if (group.Owner != userId)
            {
                return Unauthorized("You do not have permission to delete this group.");
            }

            var relatedExpenses = _context.Expenses.Where(e => e.GroupId == id);
            _context.Expenses.RemoveRange(relatedExpenses);

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return Ok("Group and its related expenses deleted successfully.");
        }

        [Authorize]
        [HttpGet("GetGroupById/{id}")]
        public async Task<IActionResult> GetGroupById(string id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound("Group not found.");
            }

            var response = new GroupResponseDTO
            {
                Id = group.Id,
                Owner = group.Owner,
                CreatedAt = group.CreatedAt,
                Name = group.Name,
                Description = group.Description
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAllGroups")]
        public async Task<ActionResult<IEnumerable<GroupResponseDTO>>> GetAllGroups()
        {
            var groups = await _context.Groups.ToListAsync();

            var response = groups.Select(group => new GroupResponseDTO
            {
                Id = group.Id,
                Owner = group.Owner,
                CreatedAt = group.CreatedAt,
                Name = group.Name,
                Description = group.Description
            });

            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAllUserGroups")]
        public async Task<IActionResult> GetAllUserGroups()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var groups = await _context.UserGroups
            .Where(ug => ug.UserId == userId && ug.Status == 1)
            .Select(ug => ug.Group)
            .ToListAsync();

            if (groups == null || groups.Count == 0)
            {
                return NotFound();
            }

            var response = groups.Select(group => new GroupResponseDTO
            {
                Id = group.Id,
                Owner = group.Owner,
                CreatedAt = group.CreatedAt,
                Name = group.Name,
                Description = group.Description
            });

            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAllGroupUsers/{groupId}")]
        public async Task<IActionResult> GetAllGroupUsers(string groupId)
        {
            var usersInGroup = await _context.UserGroups
                .Where(ug => ug.GroupId == groupId && ug.Status == 1)
                .Select(ug => ug.User)
                .ToListAsync();

            if (usersInGroup == null || !usersInGroup.Any())
            {
                return NotFound();
            }

            var response = usersInGroup.Select(user => new UserResponseDTO
            {
                Id = user.Id,
                RegisteredAt = user.RegisteredAt,
                Name = user.Name,
                Email = user.Email,
            });

            return Ok(response);
        }


        [Authorize]
        [HttpGet]
        [Route("GetAllGroupExpenses/{groupId}")]
        public async Task<IActionResult> GetAllGroupExpenses(string groupId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var groupExpenses = await _context.Expenses
                .Where(e => e.GroupId == groupId)
                .Select(e => new GroupExpensesResponseDTO
                {
                    Id = e.Id,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt,
                    AddedAt = e.AddedAt,
                    Title = e.Title,
                    Category = e.Category,
                    Description = e.Description,
                    Amount = e.Amount,
                    ShareAmount = e.UserExpenses
                                    .Where(ue => ue.UserId == userId)
                                    .Select(ue => ue.Share * e.Amount / 100)
                                    .FirstOrDefault(),
                    Status = e.UserExpenses
                                    .Where(ue => ue.UserId == userId)
                                    .Select(ue => ue.Status)
                                    .FirstOrDefault()
                })
                .ToListAsync();

            foreach (var expense in groupExpenses)
            {
                if (expense.ShareAmount == 0)
                {
                    expense.Status = 2;
                }
            }

            return Ok(groupExpenses);
        }

        [Authorize]
        [HttpPost]
        [Route("JoinGroup/{groupId}")]
        public async Task<IActionResult> JoinGroup(string groupId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound("Group not found.");
            }

            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup != null)
            {
                if (userGroup.Status == 1)
                {
                    return BadRequest("User is already a member of the group.");
                }
                else if (userGroup.Status == 2)
                {
                    userGroup.Status = 1;
                    userGroup.JoinedAt = DateTime.Now;
                    _context.UserGroups.Update(userGroup);
                    await _context.SaveChangesAsync();
                    return Ok("User joined the group successfully.");
                }
                else if (userGroup.Status == 0)
                {
                    userGroup.Status = 1;
                    userGroup.JoinedAt = DateTime.Now;
                    _context.UserGroups.Update(userGroup);
                    await _context.SaveChangesAsync();
                    return Ok("User rejoined the group successfully.");
                }
            }
            else
            {
                var newUserGroup = new UserGroup
                {
                    UserId = userId,
                    GroupId = groupId,
                    Status = 1,
                    JoinedAt = DateTime.Now
                };

                _context.UserGroups.Add(newUserGroup);
                await _context.SaveChangesAsync();
                return Ok("User joined the group successfully.");
            }

            return BadRequest("Failed to join the group.");
        }


        [Authorize]
        [HttpPost]
        [Route("LeaveGroup/{groupId}")]
        public async Task<IActionResult> LeaveGroup(string groupId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup == null)
            {
                return NotFound("User is not a member of this group.");
            }

            userGroup.Status = 0;

            _context.UserGroups.Update(userGroup);
            await _context.SaveChangesAsync();

            return Ok("User has left the group successfully.");
        }

        [Authorize]
        [HttpGet]
        [Route("IsOwner/{groupId}")]
        public async Task<IActionResult> IsOwner(string groupId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var isOwner = await _context.Groups
                .AnyAsync(g => g.Id == groupId && g.Owner == userId);

            if (!isOwner)
            {
                return Ok(false);
            }

            return Ok(true);
        }



    }
}
