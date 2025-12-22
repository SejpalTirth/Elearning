namespace CourseService.BLL.UserContext
{
    public interface IUserContextAccessor
    {
        UserContextDto? Current { get; }
    }
}
