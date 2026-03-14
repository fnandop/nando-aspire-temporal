namespace SampleWorker;

using Temporalio.Activities;
using Temporalio.Workflows;

[Workflow]
public class SayHelloWorkflow
{
    [WorkflowRun]
    public async Task<string> RunAsync(string name)
    {
        // This workflow just runs a simple activity to completion.
        // StartActivityAsync could be used to just start and there are many
        // other things that you can do inside a workflow.
        return await Workflow.ExecuteActivityAsync(
            // This is a lambda expression where the instance is typed. If this
            // were static, you wouldn't need a parameter.
            (MyActivities act) => act.SayHello(name),
            new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );
    }
}


public class MyActivities
{
    // Activities can be async and/or static too! We just demonstrate instance
    // methods since many will use them that way.
    [Activity]
    public string SayHello(string name) => $"Hello, {name}!";
}