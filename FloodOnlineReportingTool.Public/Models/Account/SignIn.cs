using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.Account;

public class SignIn
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? Email { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Password)]
    public string? Password { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public bool RememberMe { get; set; }
}