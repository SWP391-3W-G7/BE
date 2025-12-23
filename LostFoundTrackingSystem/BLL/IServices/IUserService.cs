using BLL.DTOs;
using BLL.DTOs.AdminDTO;
using BLL.DTOs.UserDTO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(UserRegisterDto userRegisterDto, IFormFile studentIdCard);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto);
        Task<UserDto> GetByIdAsync(int id);
        Task<List<UserDto>> GetUsersByRoleAsync(int? roleId);
        Task<UserDto> CreateUserByAdminAsync(AdminCreateUserDto userDto);
        Task<UserDto> UpdateUserByAdminAsync(int userId, AdminUpdateUserDto updateDto);
        Task UpdateUserBanStatusAsync(int userId, bool isBan);
        Task<UserDto> GetByEmailAsync(string email);
        Task<UserLoginResponseDto> LoginWithGoogleAsync(string email, string fullName, int? campusId = null); // Updated signature
        Task<UserLoginResponseDto> LoginWithGoogleMobileAsync(GoogleTokenRequestDto request);
        Task<UserDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto userProfileDto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<List<UserDto>> GetPendingUsersAsync();
        Task<UserLoginResponseDto> ApproveUserAsync(int userId);
        Task RejectUserAsync(int userId);
        Task UploadStudentIdCardAsync(int userId, IFormFile studentIdCard);
    }
}