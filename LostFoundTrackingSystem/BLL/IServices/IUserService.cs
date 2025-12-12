using BLL.DTOs;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto);
    }
}
