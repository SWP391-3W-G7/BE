using BLL.DTOs;
using BLL.DTOs.AdminDTO;
using BLL.DTOs.UserDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IImageService _imageService;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IConfiguration configuration, IImageService imageService, IEmailService emailService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _imageService = imageService;
            _emailService = emailService;
        }

        [Obsolete("This method is obsolete. Use RegisterAsync(UserRegisterDto, IFormFile) instead.")]
        public async Task<UserDto> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDto> RegisterAsync(UserRegisterDto userRegisterDto, IFormFile studentIdCard)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(userRegisterDto.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists.");
            }

            var studentIdCardUrl = await _imageService.UploadAsync(studentIdCard);

            var user = new User
            {
                Username = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password),
                FullName = userRegisterDto.FullName,
                RoleId = 1, // User
                Status = "Pending",
                CampusId = (int?)userRegisterDto.CampusId,
                PhoneNumber = userRegisterDto.PhoneNumber,
                StudentIdCardUrl = studentIdCardUrl
            };

            var addedUser = await _userRepository.AddUserAsync(user);
            var newUser = await _userRepository.GetUserByIdAsync(addedUser.UserId);


            if (newUser.RoleId == null)
            {
                throw new Exception("Role ID is null after user creation.");
            }

            return new UserDto
            {
                UserId = newUser.UserId,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName,
                RoleId = newUser.RoleId.Value,
                Status = newUser.Status,
                CampusId = newUser.CampusId,
                PhoneNumber = newUser.PhoneNumber,
                RoleName = newUser.Role?.RoleName,
                CampusName = newUser.Campus?.CampusName,
                StudentIdCardUrl = newUser.StudentIdCardUrl
            };
        }

        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password.");
            }

            if (user.Status == "Banned")
            {
                throw new Exception("Your account has been banned.");
            }

            if (user.Status == "IdCardUploadNeeded")
            {
                throw new Exception("Please upload your student ID card to complete registration.");
            }

            if (user.Status == "Pending")
            {
                throw new Exception("Your account is pending approval.");
            }

            if (user.RoleId == null)
            {
                throw new Exception("User does not have a role.");
            }

            if (user.CampusId == null)
            {
                throw new Exception("User does not have a campus.");
            }

            return GenerateJwtToken(user);
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId.Value,
                Status = user.Status,
                CampusId = user.CampusId,
                PhoneNumber = user.PhoneNumber,
                RoleName = user.Role?.RoleName,
                CampusName = user.Campus?.CampusName,
                StudentIdCardUrl = user.StudentIdCardUrl
            };
        }
        public async Task<List<UserDto>> GetUsersByRoleAsync(int? roleId)
        {

            if (roleId == 4)
            {
                throw new Exception("Cannot view Admin list.");
            }

            var users = await _userRepository.GetUsersByRoleAsync(roleId);

            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                RoleId = u.RoleId ?? 0,
                Status = u.Status,
                CampusId = u.CampusId,
                PhoneNumber = u.PhoneNumber,
                RoleName = u.Role?.RoleName,
                CampusName = u.Campus?.CampusName,
                StudentIdCardUrl = u.StudentIdCardUrl
            }).ToList();
        }
        public async Task<UserDto> CreateUserByAdminAsync(AdminCreateUserDto userDto)
        {
            if (userDto.RoleId == 4)
            {
                throw new Exception("Admin cannot create another Admin account.");
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists.");
            }

            // 3. Map dữ liệu sang Entity User
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                FullName = userDto.FullName,
                RoleId = userDto.RoleId,
                Status = "Active",
                CampusId = userDto.CampusId,
                PhoneNumber = userDto.PhoneNumber
            };

            var addedUser = await _userRepository.AddUserAsync(user);

            var newUser = await _userRepository.GetUserByIdAsync(addedUser.UserId);

            return new UserDto
            {
                UserId = newUser.UserId,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName,
                RoleId = newUser.RoleId ?? 0,
                Status = newUser.Status,
                CampusId = newUser.CampusId,
                PhoneNumber = newUser.PhoneNumber,
                RoleName = newUser.Role?.RoleName,
                CampusName = newUser.Campus?.CampusName,
                StudentIdCardUrl = newUser.StudentIdCardUrl
            };
        }
        // Thêm các hàm này vào Class UserService

        public async Task<UserDto> UpdateUserByAdminAsync(int userId, AdminUpdateUserDto updateDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            if (user.RoleId == 4)
            {
                throw new Exception("Cannot modify an Admin account.");
            }

            if (updateDto.RoleId == 4)
            {
                throw new Exception("Cannot promote user to Admin via this endpoint.");
            }

            if (!string.IsNullOrEmpty(updateDto.FullName)) user.FullName = updateDto.FullName;
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber)) user.PhoneNumber = updateDto.PhoneNumber;
            if (!string.IsNullOrEmpty(updateDto.Status)) user.Status = updateDto.Status;

            if (updateDto.RoleId.HasValue)
            {
                user.RoleId = updateDto.RoleId.Value;

                user.Role = null;
            }
            if (updateDto.CampusId.HasValue)
            {
                user.CampusId = updateDto.CampusId.Value;

                user.Campus = null;
            }
            await _userRepository.UpdateAsync(user);

            return await GetByIdAsync(userId);
        }
        public async Task UpdateUserBanStatusAsync(int userId, bool isBan)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (user.RoleId == 4)
            {
                throw new Exception("Cannot modify the status of an Admin account.");
            }

            user.Status = isBan ? "Banned" : "Active";

            await _userRepository.UpdateAsync(user);
        }

        public async Task<UserDto> GetByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return null;
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId.Value,
                Status = user.Status,
                CampusId = user.CampusId,
                PhoneNumber = user.PhoneNumber,
                RoleName = user.Role?.RoleName,
                CampusName = user.Campus?.CampusName,
                StudentIdCardUrl = user.StudentIdCardUrl
            };
        }

        // Update this method in your UserService class

        public async Task<UserLoginResponseDto> LoginWithGoogleAsync(string email, string fullName, int? campusId = null)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                // Create new user from Google login with campus
                var newUser = new User
                {
                    Username = email,
                    Email = email,
                    FullName = fullName,
                    RoleId = 1, // User role
                    Status = "IdCardUploadNeeded",
                    CampusId = campusId, // Set campus from parameter
                    PasswordHash = string.Empty, // Google users don't have password
                    StudentIdCardUrl = null
                };

                var addedUser = await _userRepository.AddUserAsync(newUser);

                // CRITICAL: Fetch the user again to ensure navigation properties are loaded
                user = await _userRepository.GetUserByIdAsync(addedUser.UserId);
            }
            else
            {
                // If user exists but doesn't have a campus, and campusId is provided, update it
                if (user.CampusId == null && campusId.HasValue)
                {
                    user.CampusId = campusId;
                    await _userRepository.UpdateAsync(user);

                    // Reload to get the updated Campus navigation property
                    user = await _userRepository.GetUserByIdAsync(user.UserId);
                }
            }

            // Check if account is banned
            if (user.Status == "Banned")
            {
                throw new Exception("Your account has been banned.");
            }

            if (user.Status == "IdCardUploadNeeded")
            {
                throw new Exception("Please upload your student ID card to complete registration.");
            }

            if (user.Status == "Pending")
            {
                throw new Exception("Your account is pending approval.");
            }

            return GenerateJwtToken(user);
        }

        public async Task<UserDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto userProfileDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            if (!string.IsNullOrEmpty(userProfileDto.FullName)) user.FullName = userProfileDto.FullName;
            if (!string.IsNullOrEmpty(userProfileDto.Email)) user.Email = userProfileDto.Email;
            if (!string.IsNullOrEmpty(userProfileDto.PhoneNumber)) user.PhoneNumber = userProfileDto.PhoneNumber;
            if (userProfileDto.CampusId.HasValue) user.CampusId = userProfileDto.CampusId.Value;

            await _userRepository.UpdateAsync(user);

            return await GetByIdAsync(userId);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.PasswordHash))
            {
                throw new Exception("Invalid old password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            await _userRepository.UpdateAsync(user);
        }

        public async Task<List<UserDto>> GetPendingUsersAsync()
        {
            var users = await _userRepository.GetUsersByStatusAsync("Pending");
            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                RoleId = u.RoleId ?? 0,
                Status = u.Status,
                CampusId = u.CampusId,
                PhoneNumber = u.PhoneNumber,
                RoleName = u.Role?.RoleName,
                CampusName = u.Campus?.CampusName,
                StudentIdCardUrl = u.StudentIdCardUrl
            }).ToList();
        }

        public async Task ApproveUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.Status = "Active";
            await _userRepository.UpdateAsync(user);

            // Send approval email
            var subject = "Your Account has been Approved";
            var body = $"<p>Dear {user.FullName},</p><p>Your account on the Lost and Found system has been approved. You can now log in and use the system.</p><p>Best regards,<br>The System Admin</p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        public async Task RejectUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.Status = "Banned";
            await _userRepository.UpdateAsync(user);

            // Send rejection email
            var subject = "Your Account has been Rejected";
            var body = $"<p>Dear {user.FullName},</p><p>We regret to inform you that your account on the Lost and Found system has been rejected. If you believe this is a mistake, please contact support.</p><p>Best regards,<br>The System Admin</p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        public async Task UploadStudentIdCardAsync(int userId, IFormFile studentIdCard)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (user.Status != "IdCardUploadNeeded")
            {
                throw new Exception("Student ID card upload is not required for this user or their status is not 'IdCardUploadNeeded'.");
            }

            var studentIdCardUrl = await _imageService.UploadAsync(studentIdCard);
            user.StudentIdCardUrl = studentIdCardUrl;
            user.Status = "Pending"; // Change status to Pending after ID card upload
            await _userRepository.UpdateAsync(user);
        }

        private UserLoginResponseDto GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("JWT Key is not configured.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? string.Empty),
                                new Claim("CampusName", user.Campus?.CampusName ?? string.Empty),
                                new Claim("CampusId", user.CampusId?.ToString() ?? string.Empty)
                            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new UserLoginResponseDto
            {
                Token = tokenString,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role?.RoleName,
                CampusName = user.Campus?.CampusName,
                CampusId = user.CampusId,
                Status = user.Status
            };
        }
    }
}
