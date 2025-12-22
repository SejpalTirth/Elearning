namespace NotificationService.BLL.UserContext
{
    public interface IUserContextAccessor
    {
        UserContextDto? Current { get; }
    }
}
