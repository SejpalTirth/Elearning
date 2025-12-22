namespace AssessmentService.BLL.UserContext
{
    public interface IUserContextAccessor
    {
        UserContextDto? Current { get; }
    }
}
