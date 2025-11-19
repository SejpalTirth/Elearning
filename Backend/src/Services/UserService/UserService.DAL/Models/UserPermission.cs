namespace UserService.DAL.Models
{
    public class UserPermission
    {
        public Guid UserId { get; set; }
        public int PermissionId { get; set; }

        public virtual User User { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
