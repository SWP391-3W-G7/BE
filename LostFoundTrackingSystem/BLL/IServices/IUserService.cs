using BLL.DTOs;
using BLL.DTOs.AdminDTO;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto);
        Task<UserDto> GetByIdAsync(int id);
        Task<List<UserDto>> GetUsersByRoleAsync(int? roleId);
        Task<UserDto> CreateUserByAdminAsync(AdminCreateUserDto userDto);
        Task<UserDto> UpdateUserByAdminAsync(int userId, AdminUpdateUserDto updateDto);
        Task UpdateUserBanStatusAsync(int userId, bool isBan);
        Task<UserDto> GetByEmailAsync(string email);
        Task<UserLoginResponseDto> LoginWithGoogleAsync(string email, string fullName);
    }
}

