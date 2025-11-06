using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

// Configure all the settings.
builder.Configuration.AddFloodReportingKeyVault();
var (messagingSettings, gisSettings, identityOptions) = builder.Services.AddFloodReportingSettings(builder.Configuration);

// Add services to the container.
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddFloodReportingAuthentication(builder.Configuration);
builder.Services.AddFloodReportingVersioning();
builder.Services.AddFloodReportingOpenApi(identityOptions);
builder.AddGovNotify();

// Add the HttpClient and configure it with the standard policies
builder.Services
    .AddHttpClient()
    .ConfigureHttpClientDefaults(o =>
    {
        o.AddStandardResilienceHandler();
    });

// Add health checks
builder.Services.AddFloodReportingHealthChecks();

// Setup identity

//builder.Services
//    .AddIdentityCore<FortUser>(options =>
//    {
//        options.SignIn.RequireConfirmedEmail = false;
//        options.Password.RequireDigit = false;
//        options.Password.RequiredLength = 14;
//        options.Password.RequiredUniqueChars = 0;
//        options.Password.RequireLowercase = false;
//        options.Password.RequireNonAlphanumeric = false;
//        options.Password.RequireUppercase = false;
//    })
//    .AddEntityFrameworkStores<UserDbContext>()
//    .AddApiEndpoints();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<SessionStateService>();
builder.Services.AddTransient<IEmailSender<FortUser>, FortEmailSender>();

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

builder.Services.AddScoped<TestService>();

var app = builder.Build();

var pathBase = string.IsNullOrWhiteSpace(gisSettings.PathBase) ? "/" : $"/{gisSettings.PathBase}";
app.UsePathBase(pathBase);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseScalar(identityOptions);
    app.UseSwagger(identityOptions);
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
app.MapGroup("/api/auth").MapIdentityApi<FortUser>();

await app.RunAsync();
