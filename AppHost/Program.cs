var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis", "18-3.6")
    .WithVolume("postgres-data", "/var/lib/postgresql")
    //.WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(options => options.WithImageTag("latest"));

var databasePublic = postgres.AddDatabase("FloodReportingPublic");
var databaseUsers = postgres.AddDatabase("FloodReportingUsers");

var migrations = builder.AddProject<Projects.MigrationService>("migrations")
    .WithReference(databasePublic)
    .WithReference(databaseUsers)
    .WaitFor(databasePublic)
    .WaitFor(databaseUsers);

var connectionStringBoundaries = builder.AddConnectionString("Boundaries");

builder.AddProject<Projects.FloodOnlineReportingTool_Public>("public-web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(databasePublic)
    .WithReference(databaseUsers)
    .WithReference(connectionStringBoundaries)
    .WaitFor(connectionStringBoundaries)
    .WaitForCompletion(migrations);

await builder.Build().RunAsync();
