namespace ProgresService.BLL.UserContext
{
    public class UserContextDto
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? UserName { get; set; }
    }
}
