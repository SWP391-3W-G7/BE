using BLL.DTOs.FoundItemDTO;
using BLL.DTOs.UserDTO;
using System;
using System.Collections.Generic;

namespace BLL.DTOs.ClaimRequestDTO
{
    public class ClaimRequestWithNewestEvidenceDto
    {
        public int ClaimId { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string Status { get; set; }
        public int? FoundItemId { get; set; }
        public string FoundItemTitle { get; set; }
        public int? StudentId { get; set; }
        public string StudentName { get; set; }
        public EvidenceDto NewestEvidence { get; set; }
    }
}
