using BLL.DTOs;
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
                    new Claim(ClaimTypes.Role, user.Role.RoleName),
                    new Claim("CampusName", user.Campus.CampusName)
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
                CampusName = user.Campus?.CampusName
            };
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
    }
}
