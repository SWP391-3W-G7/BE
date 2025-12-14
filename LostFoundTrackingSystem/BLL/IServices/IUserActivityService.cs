using BLL.DTOs;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IUserActivityService
    {
        Task<UserActivityDto> GetUserActivityAsync(int userId);
    }
}
