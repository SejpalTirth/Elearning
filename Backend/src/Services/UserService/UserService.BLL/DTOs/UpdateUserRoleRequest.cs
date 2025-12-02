namespace UserService.BLL.DTOs
{
    public class UpdateUserRoleRequest
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
    }
}
