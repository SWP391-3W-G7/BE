namespace BLL.DTOs
{
    public class UserLoginResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public string CampusName { get; set; }
    }
}
