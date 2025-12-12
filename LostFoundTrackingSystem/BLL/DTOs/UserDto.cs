namespace BLL.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public int RoleId { get; set; }
        public required string Status { get; set; }
        public int? CampusId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleName { get; set; }
        public string? CampusName { get; set; }
    }
}
