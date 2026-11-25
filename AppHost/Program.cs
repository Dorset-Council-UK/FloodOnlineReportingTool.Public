using FloodOnlineReportingTool.Contracts.Topics;
using ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

const string topicSuffix = "-local";
const string inboxSubscription = "fort-report-status-local";
const string outboxSubscription = "fort-public-local";

var serviceBus = builder.AddAzureServiceBus(ConnectionStringNames.ServiceBus)
    .RunAsEmulator();

// Outbound (published by Messaging OutboxWorker)
serviceBus
    .AddServiceBusTopic($"{TopicNames.FloodReportSourceCreated}{topicSuffix}")
    .AddServiceBusSubscription("outbox-frs-created", outboxSubscription);
serviceBus
    .AddServiceBusTopic($"{TopicNames.FloodReportSourceUpdated}{topicSuffix}")
    .AddServiceBusSubscription("outbox-frs-updated", outboxSubscription);

// Inbound (consumed by Messaging InboxWorker)
serviceBus
    .AddServiceBusTopic($"{TopicNames.FloodReportExtraInfoRequest}{topicSuffix}")
    .AddServiceBusSubscription("inbox-frs-extra-info-request", inboxSubscription);
serviceBus
    .AddServiceBusTopic($"{TopicNames.FloodReportSourceVerifyContact}{topicSuffix}")
    .AddServiceBusSubscription("inbox-frs-verify-contact", inboxSubscription);

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis", "18-3.6")
    .WithVolume("postgres-data", "/var/lib/postgresql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(options => options.WithImageTag("latest"));

var databasePublic = postgres.AddDatabase(ConnectionStringNames.Public);

var migrations = builder.AddProject<Projects.MigrationService>("migrations")
    .WithReference(databasePublic)
    .WaitFor(databasePublic);

builder.AddProject<Projects.FloodOnlineReportingTool_Public>("public-web")
    .WithDeveloperCertificateTrust(true)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(databasePublic)
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

builder.AddProject<Projects.Messaging>("messaging")
    .WithReference(databasePublic)
    .WithReference(serviceBus)
    .WithEnvironment("Messaging__TopicSuffix", topicSuffix)
    .WithEnvironment("Inbox__SubscriptionName", inboxSubscription)
    .WaitFor(serviceBus)
    .WaitForCompletion(migrations);

await builder.Build().RunAsync();
