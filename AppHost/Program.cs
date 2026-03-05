var builder = DistributedApplication.CreateBuilder(args);

// From 18+ PGDATA is /var/lib/postgresql/18/docker VOLUME is /var/lib/postgresql
var db = builder.AddPostgres("postgres")
    .WithImage("postgis/postgis", "18.3.6")
    .WithDataVolume("/var/lib/postgresql")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("database");

builder.AddProject<Projects.FloodOnlineReportingTool_Public>("webfrontend")
    .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db);

await builder.Build().RunAsync();
