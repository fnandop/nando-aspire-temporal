using Microsoft.Extensions.Options;
using SampleWorker;
using Temporalio.Client;
using Temporalio.Client.EnvConfig;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register as IOptions<TemporalClientConnectOptions>
var connectOptions = ClientEnvConfig.LoadClientConnectOptions();
builder.Services.AddSingleton<IOptions<TemporalClientConnectOptions>>(
    Options.Create(connectOptions));

builder.Services.AddTemporalClient();

var app = builder.Build();

app.Map("/say-hello", async (ITemporalClient temporalClient) =>
{
    var result = await temporalClient.ExecuteWorkflowAsync(
      (SayHelloWorkflow wf) => wf.RunAsync("Temporal"),
      new(id: $"my-workflow-id-{Guid.NewGuid()}", taskQueue: "my-task-queue")
  );
});

app.MapOpenApi();
app.Run();
