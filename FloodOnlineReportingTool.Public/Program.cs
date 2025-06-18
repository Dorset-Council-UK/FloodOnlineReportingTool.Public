using Azure.Identity;
using FloodOnlineReportingTool.DataAccess.DbContexts;
using FloodOnlineReportingTool.DataAccess.Models;
using FloodOnlineReportingTool.DataAccess.Settings;
using FloodOnlineReportingTool.Public.Authentication;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using FloodOnlineReportingTool.Public.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

var isDevelopment = builder.Environment.IsDevelopment();

var section = builder.Configuration
        .GetSection(GISSettings.SectionName)
        .GetSection(KeyVaultSettings.SectionName);

var keyVaultOptions = section.Get<KeyVaultSettings>();
//if keyvault options exist then we use keyvault, otherwise we ignore and use whatever local settings (appSettings, user secrets etc.) are used
if(keyVaultOptions != null && !string.IsNullOrEmpty(keyVaultOptions.Name))
{
    using var x509Store = new X509Store(StoreLocation.LocalMachine);
    x509Store.Open(OpenFlags.ReadOnly);

    var x509Certificate = x509Store.Certificates
        .Find(X509FindType.FindByThumbprint, keyVaultOptions.AzureAd.CertificateThumbprint, validOnly: false)
        .OfType<X509Certificate2>()
        .Single();

    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultOptions.Name}.vault.azure.net/"),
        new ClientCertificateCredential(keyVaultOptions.AzureAd.DirectoryId, keyVaultOptions.AzureAd.ApplicationId, x509Certificate));
}


// Add services to the container.
builder.Services.AddApplicationInsightsTelemetry();

// Add the settings/options
var rabbitMqSection = builder.Configuration.GetSection(RabbitMqSettings.SectionName);
builder.Services.Configure<RabbitMqSettings>(rabbitMqSection);
builder.Services.Configure<GISSettings>(builder.Configuration.GetSection(GISSettings.SectionName));

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
builder.Services.AddFloodReportingHealthChecks(rabbitMqSection);

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

// Add the database connections
var floodReportingConnectionString = builder.Configuration.GetConnectionString("FloodReportingPublic");
var boundariesConnectionString = builder.Configuration.GetConnectionString("Boundaries");
var identityConnectionString = builder.Configuration.GetConnectionString("FloodReportingUsers");
builder.Services
    .AddFloodReportingDatabase(floodReportingConnectionString, isDevelopment)
    .AddBoundariesDatabase(boundariesConnectionString, isDevelopment)
    .AddFloodReportingUsersDatabase(identityConnectionString, isDevelopment);

// Add the repositories
builder.Services.AddRepositories();

// Add all the validation rules
builder.Services.AddValidatorsFromAssembly(assembly);

// Add the message system, this API only needs to publish messages, not consume them
builder.Services.AddMessageSystem(rabbitMqSection);

var app = builder.Build();

var settings = builder.Configuration.GetSection(GISSettings.SectionName).Get<GISSettings>();
var pathBase = string.IsNullOrWhiteSpace(settings?.PathBase) ? "/" : $"/{settings.PathBase}";
app.UsePathBase(pathBase);

// Configure the HTTP request pipeline.
if (isDevelopment)
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalar();
    app.MapSwagger();
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
app.MapGroup("api/auth").MapIdentityApi<FortUser>();

await app.RunAsync()
         .ConfigureAwait(false);
