namespace BLL.DTOs.UserDTO
{
    public class UpdateUserProfileDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? CampusId { get; set; }
    }
}