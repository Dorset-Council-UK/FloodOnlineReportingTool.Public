namespace FloodOnlineReportingTool.Public.Services;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    string UserId { get; }
    string Name { get; }
    string Email { get; }
}
