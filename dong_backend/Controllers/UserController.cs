using dong_backend.Data;
using dong_backend.DTOs;
using dong_backend.Helpers;
using dong_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace dong_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;


        public UserController(UserManager<User> userManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = JwtHelper.GenerateJwtToken(user, _configuration);
            var refreshToken = JwtHelper.GenerateRefreshToken();
            var expiresIn = Convert.ToDouble(_configuration.GetSection("Jwt")["DurationInMinutes"]);

            var response = new
            {
                status = "success",
                message = "Login successful",

                accessToken = token,
                refreshToken = refreshToken,
                expiresIn = expiresIn,

            };

            return Ok(response);
        }

        [HttpPost("Signup")]
        public async Task<IActionResult> Signup([FromBody] SignupDTO signupDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                UserName = signupDto.Email,
                Email = signupDto.Email,
                Name = signupDto.Name
            };

            var result = await _userManager.CreateAsync(user, signupDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User created successfully.");
        }

        [Authorize]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO updateUserDto)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Name = updateUserDto.Name;
            user.Email = updateUserDto.Email;
            user.UserName = updateUserDto.Email;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var passwordResult = await _userManager.RemovePasswordAsync(user);
            if (!passwordResult.Succeeded)
            {
                return BadRequest(passwordResult.Errors);
            }

            passwordResult = await _userManager.AddPasswordAsync(user, updateUserDto.Password);
            if (!passwordResult.Succeeded)
            {
                return BadRequest(passwordResult.Errors);
            }

            return Ok("User updated successfully.");
        }

        [Authorize]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User deleted successfully.");
        }

        [Authorize]
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new { user.Id, user.Name, user.Email });
        }

        [Authorize]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new { u.Id, u.Name, u.Email })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize]
        [HttpPost]
        [Route("InviteUserToGroup")]
        public async Task<IActionResult> InviteUserToGroup(UserInviteDTO userInviteDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userInviteDTO.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == userInviteDTO.GroupId);
            if (group == null)
            {
                return NotFound("Group not found.");
            }

            var existingUserGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == user.Id && ug.GroupId == userInviteDTO.GroupId);

            if (existingUserGroup != null)
            {
                if (existingUserGroup.Status == 2)
                {
                    return BadRequest("An invitation has already been sent to this user.");
                }
                if (existingUserGroup.Status == 1)
                {
                    return BadRequest("This user is already in the group.");
                }
                if (existingUserGroup.Status == 0)
                {
                    return BadRequest("This user has already rejected your invitation.");
                }
            }

            var invitedByEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var userGroup = new UserGroup
            {
                UserId = user.Id,
                GroupId = group.Id,
                JoinedAt = DateTime.Now,
                Status = 2, // Status 2 means invitation pending
                InvitedByEmail = invitedByEmail
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            return Ok("User invited to group successfully.");
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllPendingInvitations")]
        public async Task<IActionResult> GetAllPendingInvitations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var pendingInvitations = await _context.UserGroups
                .Where(ug => ug.UserId == userId && ug.Status == 2)
                .Select(ug => new PendingInvitationDto
                {
                    GroupId = ug.GroupId,
                    GroupName = ug.Group.Name,
                    GroupDescription = ug.Group.Description,
                    InvitedByEmail = ug.InvitedByEmail
                })
                .ToListAsync();

            if (!pendingInvitations.Any())
            {
                return NotFound("No pending invitations found.");
            }

            return Ok(pendingInvitations);
        }

        [Authorize]
        [HttpPost]
        [Route("AnswerInvitation")]
        public async Task<IActionResult> AnswerInvitation(AnswerInvitationDTO answerInvitationDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == answerInvitationDTO.GroupId && ug.Status == 2);

            if (userGroup == null)
            {
                return NotFound("Pending invitation not found.");
            }

            if (answerInvitationDTO.Status < 0 || answerInvitationDTO.Status > 2)
            {
                return BadRequest("Invalid status.");
            }

            userGroup.Status = answerInvitationDTO.Status;

            if (answerInvitationDTO.Status == 1)
            {
                userGroup.JoinedAt = DateTime.Now;
            }

            _context.UserGroups.Update(userGroup);
            await _context.SaveChangesAsync();

            return Ok("Invitation status updated successfully.");
        }


    }
}
