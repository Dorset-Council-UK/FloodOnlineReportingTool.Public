using FloodOnlineReportingTool.Contracts.Topics;

var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder.AddAzureServiceBus("service-bus")
    .RunAsEmulator(e => e.WithLifetime(ContainerLifetime.Persistent));

serviceBus.AddServiceBusTopic(TopicNames.EligibilityCheckCreated)
    .AddServiceBusSubscription($"{TopicNames.EligibilityCheckCreated}-testing");
serviceBus.AddServiceBusTopic(TopicNames.FloodReportCreated)
    .AddServiceBusSubscription($"{TopicNames.FloodReportCreated}-testing");

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis", "18-3.6")
    .WithVolume("postgres-data", "/var/lib/postgresql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(options => options.WithImageTag("latest"));

var databasePublic = postgres.AddDatabase("FloodReportingPublic");

var migrations = builder.AddProject<Projects.MigrationService>("migrations")
    .WithReference(databasePublic)
    .WaitFor(databasePublic);

var connectionStringBoundaries = builder.AddConnectionString("Boundaries");

builder.AddProject<Projects.FloodOnlineReportingTool_Public>("public-web")
    .WithDeveloperCertificateTrust(true)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(databasePublic)
    .WithReference(connectionStringBoundaries)
    .WithReference(serviceBus)
    .WaitFor(connectionStringBoundaries)
    .WaitFor(serviceBus)
    .WaitForCompletion(migrations)
    .WithUrls(context =>
    {
        // shorten public-web URLs in the Dashboard
        foreach (var resource in context.Urls)
        {
            resource.DisplayText ??= resource.Endpoint?.Scheme.ToLowerInvariant();
        }

        // add Scalar, Swagger, and Test helper URLs in the Dashboard
        foreach (var url in context.Urls.Where(u => u.Endpoint?.Scheme is "https").ToList())
        {
            context.Urls.AddRange(
                new ResourceUrlAnnotation { DisplayText = "Scalar", Url = $"{url.Url}/scalar" },
                new ResourceUrlAnnotation { DisplayText = "Swagger", Url = $"{url.Url}/swagger" },
                new ResourceUrlAnnotation { DisplayText = "Test", Url = $"{url.Url}/test" }
            );
        }
    });

await builder.Build().RunAsync();
