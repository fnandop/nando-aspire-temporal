using Nando.Aspire.Temporal;
using System.Runtime.InteropServices;

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
//var worker = builder.AddProject<Projects.SampleWorker>("sample-worker").WithReference(temporal).WaitFor(temporal);
//var sampleApi = builder.AddProject<Projects.SampleApi>("sampl-api").WithReference(temporal).WaitFor(temporal);


var temporalDev = builder.AddTemporalDevServer("temporal-dev");
//var worker = builder.AddProject<Projects.SampleWorker>("sample-worker").WithReference(temporalDev).WaitFor(temporalDev);
//var sampleApi = builder.AddProject<Projects.SampleApi>("sampl-api").WithReference(temporalDev).WaitFor(temporalDev);



var temporalCloud = builder.AddTemporalCloud("temporal-cloud", "localhost:7233");
var worker = builder.AddProject<Projects.SampleWorker>("sample-worker").WithReference(temporalCloud);//.WaitFor(temporalCloud);
var sampleApi = builder.AddProject<Projects.SampleApi>("sampl-api").WithReference(temporalCloud).WithReference();//.WaitFor(temporalCloud);



builder.Build().Run();