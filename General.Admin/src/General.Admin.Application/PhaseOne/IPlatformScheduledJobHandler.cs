using System.Threading;
using Volo.Abp.DependencyInjection;

namespace General.Admin.PhaseOne;

public interface IPlatformScheduledJobHandler : ITransientDependency
{
    string JobKey { get; }

    Task<string> ExecuteAsync(CancellationToken cancellationToken = default);
}
