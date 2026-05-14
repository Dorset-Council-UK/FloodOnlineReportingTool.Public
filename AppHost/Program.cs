using FloodOnlineReportingTool.Contracts.Topics;
using ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder.AddAzureServiceBus(ConnectionStringNames.ServiceBus)
    .RunAsEmulator(e => e.WithLifetime(ContainerLifetime.Persistent));

serviceBus.AddServiceBusTopic($"test-{TopicNames.FloodReportSourceCreated}")
    .AddServiceBusSubscription("test-floodreport-public");
serviceBus.AddServiceBusTopic($"test-{TopicNames.FloodReportSourceUpdated}")
    .AddServiceBusSubscription("test-floodreport-public");

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis", "18-3.6")
    .WithVolume("postgres-data", "/var/lib/postgresql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(options => options.WithImageTag("latest"));

var databasePublic = postgres.AddDatabase(ConnectionStringNames.Public);

var migrations = builder.AddProject<Projects.MigrationService>("migrations")
    .WithReference(databasePublic)
    .WaitFor(databasePublic);

var connectionStringBoundaries = builder.AddConnectionString(ConnectionStringNames.Boundaries);

builder.AddProject<Projects.FloodOnlineReportingTool_Public>("public-web")
    .WithDeveloperCertificateTrust(true)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(databasePublic)
    .WithReference(connectionStringBoundaries)
    .WaitFor(connectionStringBoundaries)
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

builder.AddProject<Projects.Outbox>("outbox")
    .WithReference(databasePublic)
    .WithReference(serviceBus)
    .WaitFor(serviceBus)
    .WaitForCompletion(migrations);

await builder.Build().RunAsync();
