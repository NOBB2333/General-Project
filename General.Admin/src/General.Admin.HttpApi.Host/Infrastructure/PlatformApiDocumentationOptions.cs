namespace General.Admin.Infrastructure;

public class PlatformApiDocumentationOptions
{
    public const string SectionName = "ApiDocumentation";

    public string OpenApiRoutePattern { get; set; } = "/swagger/{documentName}/swagger.json";

    public string ScalarRoutePrefix { get; set; } = "/scalar";

    public string SwaggerRoutePrefix { get; set; } = "swagger";

    public string Title { get; set; } = "General Admin 接口文档";

    public List<PlatformApiDocumentOptions> Documents { get; set; } = [];

    public string Ui { get; set; } = "Scalar";

    public bool IsEnabled()
    {
        return !string.Equals(Ui, "None", StringComparison.OrdinalIgnoreCase);
    }

    public bool UseScalar()
    {
        return string.Equals(Ui, "Scalar", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Ui, "Both", StringComparison.OrdinalIgnoreCase);
    }

    public bool UseSwagger()
    {
        return string.Equals(Ui, "Swagger", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Ui, "Both", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<PlatformApiDocumentOptions> ResolveDocuments()
    {
        return Documents.Count > 0
            ? Documents
            :
            [
                new(ApiDocGroups.Platform, "平台 API", true),
                new(ApiDocGroups.Project, "项目 API", false),
                new(ApiDocGroups.Business, "业务 API", false)
            ];
    }
}

public class PlatformApiDocumentOptions
{
    public PlatformApiDocumentOptions()
    {
    }

    public PlatformApiDocumentOptions(string name, string title, bool isDefault)
    {
        Name = name;
        Title = title;
        IsDefault = isDefault;
    }

    public bool IsDefault { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
