using System.Threading.Channels;
using System.Threading;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Platform;

public class PlatformRequestAuditQueue : ISingletonDependency
{
    private readonly Channel<PlatformRequestAuditEntry> _channel = Channel.CreateUnbounded<PlatformRequestAuditEntry>(
        new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask<PlatformRequestAuditEntry> ReadAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }

    public bool TryEnqueue(PlatformRequestAuditEntry entry)
    {
        return _channel.Writer.TryWrite(entry);
    }

    public bool TryRead(out PlatformRequestAuditEntry? entry)
    {
        var success = _channel.Reader.TryRead(out var value);
        entry = value;
        return success;
    }
}
