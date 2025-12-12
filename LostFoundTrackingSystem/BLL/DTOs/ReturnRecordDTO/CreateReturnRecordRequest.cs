using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.ReturnRecordDTO
{
    public class CreateReturnRecordRequest
    {
        [Required]
        public int FoundItemId { get; set; }

        public int? LostItemId { get; set; } 

        [Required]
        public int ReceiverId { get; set; }

        public string? Note { get; set; }

        public DateTime? ReturnDate { get; set; } = DateTime.UtcNow;
    }
}