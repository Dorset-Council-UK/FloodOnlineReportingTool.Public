var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    //.WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var databasePublic = postgres.AddDatabase("FloodReportingPublic");
var databaseUsers = postgres.AddDatabase("FloodReportingUsers");

var connectionStringBoundaries = builder.AddConnectionString("Boundaries");

builder.AddProject<Projects.FloodOnlineReportingTool_Public>("public-web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(databasePublic)
    .WithReference(databaseUsers)
    .WithReference(connectionStringBoundaries)    
    .WaitFor(databasePublic)
    .WaitFor(databaseUsers)
    .WaitFor(connectionStringBoundaries);

await builder.Build().RunAsync();
