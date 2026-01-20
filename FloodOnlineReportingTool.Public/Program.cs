using FloodOnlineReportingTool.Database.Compliance;
using FloodOnlineReportingTool.Database.Models.Contact;
using FloodOnlineReportingTool.Database.Options;
using FloodOnlineReportingTool.Public.Models.Order;
using FloodOnlineReportingTool.Public.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

// Configure Key Vault access
builder.AddKeyVaults();

// Configure all the options.
var gisOptions = builder.AddOptions_Required<GISOptions>(GISOptions.SectionName);

// Configure authentication 
var identityOptions = builder.AddOptions_Required<MicrosoftIdentityOptions>(Constants.AzureAd);
builder.AddAuthentication();
builder.Services.AddScoped<SessionStateService>();

// Configure messaging system
builder.AddMessageSystem();
builder.AddGovNotify();
builder.Services.AddTransient<IEmailSender<FortUser>, FortEmailSender>();

// Configure API versioning and OpenAPI
builder.Services.AddFloodReportingVersioning();
builder.Services.AddFloodReportingOpenApi(identityOptions);

// Configure logging
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.EnableRedaction();
builder.Services.AddRedaction(x =>
{
    // Configure erasing redactor for personal data
    x.SetRedactor<StarRedactor>(new DataClassificationSet(PersonalDataClassifications.Pii));

    // Or use HMAC redactor if you need to correlate values
    var maskingKey = builder.Configuration["GIS:MaskingKey"];
    if (maskingKey is not string key)
    {
        throw new InvalidOperationException("HMAC key not configured");
    }
    x.SetHmacRedactor(options =>
    {
        options.Key = key;
        options.KeyId = 862;
    }, new DataClassificationSet(PersonalDataClassifications.PiiRedaction));
});

// Add health checks
builder.Services.AddFloodReportingHealthChecks();

// Add Blazor services
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// Add the database connections
var floodReportingConnectionString = builder.Configuration.GetConnectionString("FloodReportingPublic");
var boundariesConnectionString = builder.Configuration.GetConnectionString("Boundaries");
var identityConnectionString = builder.Configuration.GetConnectionString("FloodReportingUsers");
builder.Services
    .AddFloodReportingDatabase(floodReportingConnectionString)
    .AddBoundariesDatabase(boundariesConnectionString)
    .AddFloodReportingUsersDatabase(identityConnectionString);

// Add all the validation rules
builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddScoped<TestService>();

var app = builder.Build();

var pathBase = string.IsNullOrWhiteSpace(gisOptions.PathBase) ? "/" : $"/{gisOptions.PathBase}";
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
app.MapAuthenticationEndpoints();

// Map all identity endpoints
app.MapGroup("/api/auth").MapIdentityApi<FortUser>();

await app.RunAsync();
