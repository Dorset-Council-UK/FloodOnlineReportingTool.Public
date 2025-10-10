namespace FloodOnlineReportingTool.Public.Services;

internal interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    string UserId { get; }
    string Name { get; }
    string Email { get; }
}
