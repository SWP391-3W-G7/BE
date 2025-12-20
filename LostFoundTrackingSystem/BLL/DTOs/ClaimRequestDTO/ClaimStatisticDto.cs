namespace BLL.DTOs.ClaimRequestDTO
{
    public class ClaimStatisticDto
    {
        public int TotalPending { get; set; }     // Status: Pending (0)
        public int TotalApproved { get; set; }    // Status: Approved (1)
        public int TotalRejected { get; set; }    // Status: Rejected (2)
        public int TotalReturned { get; set; }    // Status: Returned (3)
        public int TotalConflicted { get; set; }  // Status: Conflicted (4)
        public int TotalClaims { get; set; }      // Tổng cộng
    }
}