namespace BLL.DTOs.ReturnRecordDTO
{
    public class ReturnRecordDto
    {
        public int ReturnId { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Note { get; set; }
        public int? ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
        public int? FoundItemId { get; set; }
        public int? LostItemId { get; set; }
    }
}