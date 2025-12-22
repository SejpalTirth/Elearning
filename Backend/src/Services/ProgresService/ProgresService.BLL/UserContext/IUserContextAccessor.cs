namespace ProgresService.BLL.UserContext
{
    public interface IUserContextAccessor
    {
        UserContextDto? Current { get; }
    }
}
