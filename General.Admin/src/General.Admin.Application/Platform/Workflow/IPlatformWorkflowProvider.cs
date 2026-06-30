namespace General.Admin.Platform;

public interface IPlatformWorkflowProvider
{
    Task<string> StartAsync(PlatformWorkflowStartInput input);
}
