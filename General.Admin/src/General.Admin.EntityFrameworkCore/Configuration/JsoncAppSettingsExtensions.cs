using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;

namespace General.Admin.EntityFrameworkCore;

public static class JsoncAppSettingsExtensions
{
    /// <summary>从指定基础目录下的 relativePath 子目录加载所有 .json/.jsonc 文件（供设计时工厂使用）</summary>
    public static IConfigurationBuilder AddJsoncAppSettingsDirectory(
        this IConfigurationBuilder configBuilder,
        string basePath,
        string relativePath = "appsettings")
    {
        var directoryPath = Path.Combine(basePath, relativePath);
        if (!Directory.Exists(directoryPath))
        {
            return configBuilder;
        }

        foreach (var filePath in Directory
                     .EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                     .Where(path =>
                     {
                         var extension = Path.GetExtension(path);
                         return extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
                                extension.Equals(".jsonc", StringComparison.OrdinalIgnoreCase);
                     })
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            configBuilder.Add(new JsoncFileConfigurationSource(filePath));
        }

        return configBuilder;
    }

    public static IHostBuilder AddJsoncAppSettingsDirectory(this IHostBuilder hostBuilder, string relativePath = "appsettings")
    {
        return hostBuilder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            var directoryPath = Path.Combine(context.HostingEnvironment.ContentRootPath, relativePath);
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            foreach (var filePath in Directory
                         .EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                         .Where(path =>
                         {
                             var extension = Path.GetExtension(path);
                             return extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
                                    extension.Equals(".jsonc", StringComparison.OrdinalIgnoreCase);
                         })
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                configurationBuilder.Add(new JsoncFileConfigurationSource(filePath));
            }
        });
    }

    private sealed class JsoncFileConfigurationSource : IConfigurationSource
    {
        private readonly string _filePath;

        public JsoncFileConfigurationSource(string filePath)
        {
            _filePath = filePath;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new MemoryConfigurationProvider(new MemoryConfigurationSource
            {
                InitialData = LoadKeyValuePairs(_filePath)
            });
        }

        private static IDictionary<string, string?> LoadKeyValuePairs(string filePath)
        {
            var json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            }

            var rootNode = JsonNode.Parse(
                json,
                documentOptions: new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                });

            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            FlattenNode(rootNode, parentPath: null, values);
            return values;
        }

        private static void FlattenNode(JsonNode? node, string? parentPath, IDictionary<string, string?> values)
        {
            switch (node)
            {
                case null:
                    values[parentPath ?? string.Empty] = null;
                    return;
                case JsonObject jsonObject:
                {
                    foreach (var (key, childNode) in jsonObject)
                    {
                        var childPath = string.IsNullOrWhiteSpace(parentPath) ? key : $"{parentPath}:{key}";
                        FlattenNode(childNode, childPath, values);
                    }

                    return;
                }
                case JsonArray jsonArray:
                {
                    for (var index = 0; index < jsonArray.Count; index++)
                    {
                        var childPath = string.IsNullOrWhiteSpace(parentPath) ? index.ToString() : $"{parentPath}:{index}";
                        FlattenNode(jsonArray[index], childPath, values);
                    }

                    return;
                }
                case JsonValue jsonValue:
                    values[parentPath ?? string.Empty] = jsonValue.ToJsonString().Trim('"');
                    return;
                default:
                    values[parentPath ?? string.Empty] = node.ToJsonString().Trim('"');
                    return;
            }
        }
    }
}
