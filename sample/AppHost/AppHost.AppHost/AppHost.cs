using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(5432)
    .WithPgAdmin();



//var temporal = builder.AddTemporal("temporal")
//                      .WithPostgres(postgres)
//                      .WithtTemporalAdminTools()
//                      .WithtTemporalUi();


var temporal = builder.AddTemporalDevServer("temporal-dev");



var worker = builder.AddProject<Projects.SampleWorker>("sample-worker").WithReference(temporal).WaitFor(temporal);
var sampleApi = builder.AddProject<Projects.SampleApi>("sampl-api").WithReference(temporal).WaitFor(temporal);

builder.Build().Run();