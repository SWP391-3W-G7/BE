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
        private readonly INotificationService _notificationService;

        public MatchingService(IMatchingRepository matchingRepository, ILostItemRepository lostItemRepository, IMatchHistoryRepository matchHistoryRepository, INotificationService notificationService)
        {
            _matchingRepository = matchingRepository;
            _lostItemRepository = lostItemRepository;
            _matchHistoryRepository = matchHistoryRepository;
            _notificationService = notificationService;
        }

        public async Task FindAndCreateMatchesAsync(int lostItemId)
        {
            var lostItem = await _lostItemRepository.GetByIdAsync(lostItemId);
            if (lostItem == null)
            {
                // Or handle this case as you see fit
                throw new Exception("Lost item not found.");
            }
            int matchCount = 0;

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
                matchCount++;
            }
            if (matchCount > 0 && lostItem.CreatedBy.HasValue)
            {
                string userId = lostItem.CreatedBy.Value.ToString();
                string message = matchCount == 1
                    ? $"We found a potential match for your lost item '{lostItem.Title}'!"
                    : $"We found {matchCount} potential matches for your lost item '{lostItem.Title}'!";

                try
                {
                    await _notificationService.SendMatchNotificationAsync(
                        userId,
                        lostItemId,
                        message
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send match notification: {ex.Message}");
                }
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
                if (lostItem.Status == "Lost") 
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
                match.MatchStatus = "Approved";
                match.Status = "Resolved";
                await _matchingRepository.UpdateMatchAsync(match);

                await _matchHistoryRepository.AddAsync(new MatchHistory
                {
                    MatchId = matchId,
                    Action = "Approved",
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
