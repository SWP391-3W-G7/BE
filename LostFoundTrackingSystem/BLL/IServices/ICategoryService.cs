using BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
    }
}
