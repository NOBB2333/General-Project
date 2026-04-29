using System.IO;

namespace General.Admin.Infrastructure;

internal sealed class TemporaryFileReadStream : FileStream
{
    private readonly string _path;

    public TemporaryFileReadStream(string path)
        : base(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.SequentialScan)
    {
        _path = path;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        TryDelete();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        TryDelete();
    }

    private void TryDelete()
    {
        try
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
        catch
        {
            // Best-effort cleanup for temporary download buffers.
        }
    }
}
