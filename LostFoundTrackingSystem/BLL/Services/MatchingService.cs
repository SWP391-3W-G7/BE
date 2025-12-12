using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly IMatchingRepository _matchingRepository;
        private readonly ILostItemRepository _lostItemRepository;
        private readonly IMatchHistoryRepository _matchHistoryRepository;

        public MatchingService(IMatchingRepository matchingRepository, ILostItemRepository lostItemRepository, IMatchHistoryRepository matchHistoryRepository)
        {
            _matchingRepository = matchingRepository;
            _lostItemRepository = lostItemRepository;
            _matchHistoryRepository = matchHistoryRepository;
        }

        public async Task FindAndCreateMatchesAsync(int lostItemId)
        {
            var lostItem = await _lostItemRepository.GetByIdAsync(lostItemId);
            if (lostItem == null)
            {
                // Or handle this case as you see fit
                throw new Exception("Lost item not found.");
            }

            var potentialMatches = await _matchingRepository.GetPotentialMatchesAsync(lostItem);

            foreach (var foundItem in potentialMatches)
            {
                var itemMatch = new ItemMatch
                {
                    LostItemId = lostItem.LostItemId,
                    FoundItemId = foundItem.FoundItemId,
                    MatchStatus = "Matched", // System-generated match
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending" // Pending staff review
                };
                await _matchingRepository.AddMatchAsync(itemMatch);
            }
        }

        public async Task<IEnumerable<ItemMatch>> GetMatchesForFoundItemAsync(int foundItemId)
        {
            return await _matchingRepository.GetMatchesForFoundItemAsync(foundItemId);
        }

        public async Task FindAndCreateMatchesForAllLostItemsAsync()
        {
            var allLostItems = await _lostItemRepository.GetAllAsync();
            foreach (var lostItem in allLostItems)
            {
                if (lostItem.Status == "Lost") // or whatever status indicates it's still an active lost item
                {
                    await FindAndCreateMatchesAsync(lostItem.LostItemId);
                }
            }
        }

        public async Task ConfirmMatchAsync(int matchId, int staffUserId)
        {
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match != null)
            {
                match.MatchStatus = "Confirmed";
                match.Status = "Resolved";
                await _matchingRepository.UpdateMatchAsync(match);

                await _matchHistoryRepository.AddAsync(new MatchHistory
                {
                    MatchId = matchId,
                    Action = "Confirmed",
                    ActionDate = DateTime.UtcNow,
                    ActionBy = staffUserId
                });
            }
        }

        public async Task DismissMatchAsync(int matchId, int staffUserId)
        {
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match != null)
            {
                match.MatchStatus = "Dismissed";
                await _matchingRepository.UpdateMatchAsync(match);

                await _matchHistoryRepository.AddAsync(new MatchHistory
                {
                    MatchId = matchId,
                    Action = "Dismissed",
                    ActionDate = DateTime.UtcNow,
                    ActionBy = staffUserId
                });
            }
        }
    }
}
