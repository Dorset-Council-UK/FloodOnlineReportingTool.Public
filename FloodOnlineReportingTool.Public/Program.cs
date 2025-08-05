using FloodOnlineReportingTool.DataAccess.DbContexts;
using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

// Configure all the settings.
var (keyVaultSettings, messagingSettings, gisSettings) = builder.Services.AddFloodReportingSettings(builder.Configuration);

//if keyvault options exist then we use keyvault, otherwise we ignore and use whatever local settings (appSettings, user secrets etc.) are used
if(keyVaultSettings != null)
{
    builder.Configuration.AddFloodReportingKeyVault(keyVaultSettings);
}

// Add services to the container.
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddFloodReportingVersioning();
builder.Services.AddFloodReportingOpenApi();

// Add the HttpClient and configure it with the standard policies
builder.Services
    .AddHttpClient()
    .ConfigureHttpClientDefaults(o =>
    {
        o.AddStandardResilienceHandler();
    });

// Add health checks
builder.Services.AddFloodReportingHealthChecks(messagingSettings);

// Setup identity
builder.Services
    .AddIdentityCore<FortUser>(options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 14;
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<UserDbContext>()
    .AddApiEndpoints();
builder.Services.AddTransient<IEmailSender<FortUser>, FortEmailSender>();

// Add the authentication and authorization
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "FloodReportCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ReturnUrlParameter = "returnUrl";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.LoginPath = "/" + AccountPages.SignIn.Url;
        options.LogoutPath = "/" + AccountPages.SignOut.Url;
        options.AccessDeniedPath = "/" + GeneralPages.AccessDenied;
        options.SlidingExpiration = true;
    });
builder.Services.AddCascadingAuthenticationState();
builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(PolicyNames.Identity, policy =>
    {
        policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
        policy.RequireAuthenticatedUser();
    });

// Add Blazor services
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<GdsBlazorComponents.IGdsJsInterop, GdsBlazorComponents.GdsJsInterop>();

// Add the database connections
var floodReportingConnectionString = builder.Configuration.GetConnectionString("FloodReportingPublic");
var boundariesConnectionString = builder.Configuration.GetConnectionString("Boundaries");
var identityConnectionString = builder.Configuration.GetConnectionString("FloodReportingUsers");
builder.Services
    .AddFloodReportingDatabase(floodReportingConnectionString)
    .AddBoundariesDatabase(boundariesConnectionString)
    .AddFloodReportingUsersDatabase(identityConnectionString);

// Add the repositories
builder.Services.AddRepositories();

// Add all the validation rules
builder.Services.AddValidatorsFromAssembly(assembly);

// Add the message system
builder.Services.AddMessageSystem(messagingSettings);

var app = builder.Build();

var pathBase = string.IsNullOrWhiteSpace(gisSettings.PathBase) ? "/" : $"/{gisSettings.PathBase}";
app.UsePathBase(pathBase);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseScalar();
    app.UseSwagger();
}
else
{
    app.UseExceptionHandler(GeneralPages.Error.Url, createScopeForErrors: true);
    app.UseHsts(); 
}

app.UseHttpsRedirection();
app.MapFloodReportingHealthChecks();
app.UseRouting();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<FloodOnlineReportingTool.Public.Components.App>()
   .AddInteractiveServerRenderMode();

// Map all identity endpoints
app.MapGroup($"{pathBase}/api/auth").MapIdentityApi<FortUser>();

await app.RunAsync()
         .ConfigureAwait(false);
