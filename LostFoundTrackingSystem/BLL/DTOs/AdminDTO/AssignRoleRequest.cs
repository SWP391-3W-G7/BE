using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.AdminDTO
{
    public class AssignRoleRequest
    {
        [Required]
        public int UserId { get; set; } 

        [Required]
        public int CampusId { get; set; } 

        [Required]
        public int RoleId { get; set; } 
    }
}