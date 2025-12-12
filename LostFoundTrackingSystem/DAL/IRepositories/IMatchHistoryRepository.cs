using DAL.Models;
using System.Threading.Tasks;

namespace DAL.IRepositories
{
    public interface IMatchHistoryRepository
    {
        Task AddAsync(MatchHistory matchHistory);
    }
}
