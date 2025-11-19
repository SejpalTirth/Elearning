namespace UserService.DAL.Models
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}
