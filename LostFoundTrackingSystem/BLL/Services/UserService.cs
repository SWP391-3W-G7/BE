using BLL.DTOs;
using BLL.DTOs.AdminDTO;
using BLL.DTOs.UserDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
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

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<UserDto> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(userRegisterDto.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists.");
            }

            var user = new User
            {
                Username = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password),
                FullName = userRegisterDto.FullName,
                RoleId = 1, // User
                Status = "Active",
                CampusId = (int?)userRegisterDto.CampusId,
                PhoneNumber = userRegisterDto.PhoneNumber
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
                CampusName = newUser.Campus?.CampusName
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
                CampusName = user.Campus?.CampusName
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
                CampusName = u.Campus?.CampusName
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
                CampusName = newUser.Campus?.CampusName
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

            if (updateDto.RoleId.HasValue) user.RoleId = updateDto.RoleId.Value;
            if (updateDto.CampusId.HasValue) user.CampusId = updateDto.CampusId.Value;

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
                CampusName = user.Campus?.CampusName
            };
        }

        public async Task<UserLoginResponseDto> LoginWithGoogleAsync(string email, string fullName)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                        var newUser = new User
                        {
                            Email = email,
                            FullName = fullName,
                            RoleId = 1, // User
                            Status = "Active",
                            CampusId = null
                        };
                        var addedUser = await _userRepository.AddUserAsync(newUser);
                        user = await _userRepository.GetUserByIdAsync(addedUser.UserId);
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
                            CampusId = user.CampusId
                        };
                    }    }
}
