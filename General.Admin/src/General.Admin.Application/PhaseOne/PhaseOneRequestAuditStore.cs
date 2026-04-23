using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Volo.Abp.DependencyInjection;

namespace General.Admin.PhaseOne;

public class PhaseOneRequestAuditStore : ITransientDependency
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IHostEnvironment _hostEnvironment;

    public PhaseOneRequestAuditStore(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async Task AppendAsync(PhaseOneRequestAuditEntry entry)
    {
        var filePath = GetAuditFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var payload = JsonSerializer.Serialize(entry, JsonSerializerOptions);
        await File.AppendAllTextAsync(filePath, payload + Environment.NewLine, Encoding.UTF8);
    }

    public async Task<List<PhaseOneRequestAuditEntry>> ReadAsync()
    {
        var filePath = GetAuditFilePath();
        if (!File.Exists(filePath))
        {
            return [];
        }

        var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
        var result = new List<PhaseOneRequestAuditEntry>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var item = JsonSerializer.Deserialize<PhaseOneRequestAuditEntry>(line, JsonSerializerOptions);
                if (item != null)
                {
                    result.Add(item);
                }
            }
            catch
            {
                // Skip malformed lines to keep the audit pipeline resilient.
            }
        }

        return result;
    }

    private string GetAuditFilePath()
    {
        return Path.Combine(_hostEnvironment.ContentRootPath, "Logs", "phase-one-audit.jsonl");
    }
}

public class PhaseOneRequestAuditEntry
{
    public string? ActionSummary { get; set; }

    public string? BrowserInfo { get; set; }

    public string? Category { get; set; } // "api" | "pagevisit"

    public string? ClientIpAddress { get; set; }

    public int ExecutionDuration { get; set; }

    public DateTime ExecutionTime { get; set; }

    public string? ExceptionMessage { get; set; }

    public bool HasException { get; set; }

    public string? HttpMethod { get; set; }

    public int? HttpStatusCode { get; set; }

    public Guid Id { get; set; }

    public string? MenuTitle { get; set; } // populated for pagevisit category

    public string? TenantName { get; set; }

    public string? Url { get; set; }

    public string? UserName { get; set; }
}
