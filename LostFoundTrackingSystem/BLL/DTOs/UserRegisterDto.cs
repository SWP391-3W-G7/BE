namespace BLL.DTOs
{
    public class UserRegisterDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
        public int? CampusId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
