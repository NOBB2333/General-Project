using Volo.Abp.DependencyInjection;

namespace General.Admin.Platform;

public interface IFileBusinessLinkResolver : ITransientDependency
{
    bool CanResolve(string businessType);

    Task<Guid?> ResolveProjectIdAsync(string businessType, string businessId);
}
