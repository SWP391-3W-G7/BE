namespace BLL.DTOs
{
    public class AdminCreateUserDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
        public required int RoleId { get; set; }
        public int? CampusId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}