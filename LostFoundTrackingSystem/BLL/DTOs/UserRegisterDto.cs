using DAL.Models;

namespace BLL.DTOs
{
    public class UserRegisterDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public CampusEnum? CampusId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
