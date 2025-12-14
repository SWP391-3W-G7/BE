using System;

namespace BLL.DTOs
{
    public class ItemActionLogDto
    {
        public int ActionId { get; set; }
        public int? LostItemId { get; set; }
        public int? FoundItemId { get; set; }
        public int? ClaimRequestId { get; set; }
        public string? ActionType { get; set; }
        public string? ActionDetails { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public DateTime? ActionDate { get; set; }
        public int? PerformedBy { get; set; }
        public string? PerformedByName { get; set; }
        public int? CampusId { get; set; }
        public string? CampusName { get; set; }
    }
}
