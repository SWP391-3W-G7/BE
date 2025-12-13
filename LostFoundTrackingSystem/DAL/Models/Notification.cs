#nullable disable
using System;

namespace DAL.Models
{
    public partial class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } // 'match', 'claim', 'return'
        public int? ReferenceId { get; set; } // matchId, claimId, etc.
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public virtual User User { get; set; }
    }
}