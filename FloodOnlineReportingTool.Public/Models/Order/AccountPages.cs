namespace FloodOnlineReportingTool.Public.Models.Order;

internal static class AccountPages
{
    private static ReadOnlySpan<char> BaseUrl => "account";

    public static readonly PageInfo SignIn = new(BaseUrl, "/signin", "Sign in");
    public static readonly PageInfo SignOut = new(BaseUrl, "/signout", "Sign out");
    public static readonly PageInfo SignInWithTwoFactor = new(BaseUrl, "/twofactor/signin", "Sign in");
    public static readonly PageInfo Register = new(BaseUrl, "/register", "Register");
    public static readonly PageInfo RegisterConfirmation = new(BaseUrl, "/register/confirmation", "Registration confirmation");
    public static readonly PageInfo Lockout = new(BaseUrl, "/lockout", "Locked out");
    public static readonly PageInfo EmailConfirm = new(BaseUrl, "/email/confirm", "Confirm your email address");
    public static readonly PageInfo EmailResendConfirm = new(BaseUrl, "/email/confirm/resend", "Resend email confirmation");
    public static readonly PageInfo PasswordForgot = new(BaseUrl, "/password/forgot", "Forgot your password?");
}
