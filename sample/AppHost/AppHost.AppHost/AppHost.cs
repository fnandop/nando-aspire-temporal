using Fnandop.Temporal.Hosting;
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(5432)
    .WithPgAdmin();

var temporal = builder.AddTemporal("temporal")
                      .WithPostgres(postgres)
                      .WithtTemporalAdminTools()
                      .WithtTemporalUi();

builder.Build().Run();
