using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiArtifactDto
{
    public string ArtifactCode { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string Description { get; set; } = string.Empty;

    public string ExamplePayloadJson { get; set; } = "{}";

    public Guid Id { get; set; }

    public bool IsBuiltin { get; set; }

    public bool IsEnabled { get; set; }

    public string JsonSchema { get; set; } = "{}";

    public string Name { get; set; } = string.Empty;

    public string RendererType { get; set; } = string.Empty;

    public int Sort { get; set; }

    public string SystemHint { get; set; } = string.Empty;
}

public class AiArtifactSaveInput
{
    [Required]
    [MaxLength(128)]
    public string ArtifactCode { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Category { get; set; } = "Chart";

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(16_000)]
    public string ExamplePayloadJson { get; set; } = "{}";

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(16_000)]
    public string JsonSchema { get; set; } = "{}";

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string RendererType { get; set; } = "metric-card";

    public int Sort { get; set; }

    [MaxLength(4000)]
    public string SystemHint { get; set; } = string.Empty;
}

public class AiArtifactQueryInput
{
    [MaxLength(64)]
    public string? Category { get; set; }

    public bool IncludeDisabled { get; set; } = true;

    [MaxLength(64)]
    public string? Keyword { get; set; }

    [MaxLength(64)]
    public string? RendererType { get; set; }
}
