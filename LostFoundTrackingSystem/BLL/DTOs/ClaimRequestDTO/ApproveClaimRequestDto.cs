using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.ClaimRequestDTO
{
    public class ApproveClaimRequestDto
    {
        [Required(ErrorMessage = "Pickup location is required")]
        public string PickupLocation { get; set; } = null!;

        [Required(ErrorMessage = "Pickup time is required")]
        public DateTime PickupTime { get; set; }

        public string? AdminNote { get; set; }
    }
}
