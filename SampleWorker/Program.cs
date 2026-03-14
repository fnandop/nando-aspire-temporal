using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SampleWorker;
using Temporalio.Client;
using Temporalio.Client.EnvConfig;
using Temporalio.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);


var connectOptions = ClientEnvConfig.LoadClientConnectOptions();

// Register as IOptions<TemporalClientConnectOptions>
builder.Services.AddSingleton<IOptions<TemporalClientConnectOptions>>(
    Options.Create(connectOptions));

builder.Services.AddTemporalClient();
builder.Services
    .AddHostedTemporalWorker(taskQueue: "my-task-queue")
    .AddScopedActivities<MyActivities>()
    .AddWorkflow<SayHelloWorkflow>();

var host = builder.Build();
await host.RunAsync();