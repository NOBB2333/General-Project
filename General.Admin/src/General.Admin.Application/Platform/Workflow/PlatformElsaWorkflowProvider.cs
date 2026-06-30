using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Platform;

[ExposeServices(typeof(IPlatformWorkflowProvider))]
public class PlatformElsaWorkflowProvider : IPlatformWorkflowProvider, ITransientDependency
{
    private readonly IWorkflowRunner _workflowRunner;

    public PlatformElsaWorkflowProvider(IWorkflowRunner workflowRunner)
    {
        _workflowRunner = workflowRunner;
    }

    public async Task<string> StartAsync(PlatformWorkflowStartInput input)
    {
        var result = await _workflowRunner.RunAsync(new Sequence
        {
            Activities =
            {
                new WriteLine($"启动工作流：{input.WorkflowDefinitionKey}"),
                new WriteLine($"业务对象：{input.BusinessType}/{input.BusinessId}")
            }
        });

        return string.IsNullOrWhiteSpace(result.WorkflowState.Id)
            ? Guid.NewGuid().ToString("N")
            : result.WorkflowState.Id;
    }
}
