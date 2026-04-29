namespace General.Admin.Platform;

public interface IPlatformFileStorageProviderResolver
{
    IPlatformFileStorageProvider Resolve(string? providerName = null);
}
