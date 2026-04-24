using System.Threading.Channels;
using System.Threading;
using Volo.Abp.DependencyInjection;

namespace General.Admin.PhaseOne;

public class PhaseOneRequestAuditQueue : ISingletonDependency
{
    private readonly Channel<PhaseOneRequestAuditEntry> _channel = Channel.CreateUnbounded<PhaseOneRequestAuditEntry>(
        new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask<PhaseOneRequestAuditEntry> ReadAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }

    public bool TryEnqueue(PhaseOneRequestAuditEntry entry)
    {
        return _channel.Writer.TryWrite(entry);
    }

    public bool TryRead(out PhaseOneRequestAuditEntry? entry)
    {
        var success = _channel.Reader.TryRead(out var value);
        entry = value;
        return success;
    }
}
