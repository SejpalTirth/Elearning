namespace UserService.BLL.UserContext
{
    public interface IUserContextAccessor
    {
        UserContextDto? Current { get; }
    }
}
