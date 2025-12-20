using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.StaffDTO
{
    public class RequestDropOffDto
    {
        [Required(ErrorMessage = "Storage Location is required")]
        public string StorageLocation { get; set; } = null!;

        public string? Note { get; set; }
    }
}
