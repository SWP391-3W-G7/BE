using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AdminDTO
{
    public class AdminUpdateUserDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? RoleId { get; set; }
        public int? CampusId { get; set; }
        public string? Status { get; set; } // Ví dụ: "Active", "Banned"
    }
}
