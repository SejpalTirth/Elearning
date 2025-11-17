namespace UserService.BLL.DTOs
{
    public class UserAuthDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}
