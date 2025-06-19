using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.Account;

public class Register
{
    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? Email { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Password)]
    public string? Password { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Password)]
    public string? ConfirmPassword { get; set; }
}