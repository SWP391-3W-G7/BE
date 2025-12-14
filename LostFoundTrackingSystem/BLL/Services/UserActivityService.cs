using BLL.DTOs;
using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.LostItemDTO;
using BLL.IServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly IClaimRequestService _claimRequestService;
        private readonly ILostItemService _lostItemService;

        public UserActivityService(IClaimRequestService claimRequestService, ILostItemService lostItemService)
        {
            _claimRequestService = claimRequestService;
            _lostItemService = lostItemService;
        }

        public async Task<UserActivityDto> GetUserActivityAsync(int userId)
        {
            var claims = await _claimRequestService.GetMyClaimsAsync(userId);
            var lostItems = await _lostItemService.GetMyLostItemsAsync(userId);

            return new UserActivityDto
            {
                ClaimRequests = claims,
                LostItems = lostItems
            };
        }
    }
}