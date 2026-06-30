using System.Text.Json;
using Shouldly;
using Xunit;

namespace General.Admin.Integration;

public class IntegrationNotificationServiceTests
{
    [Theory]
    [InlineData("business.contractAmount")]
    [InlineData("business.grossProfitRate")]
    [InlineData("project.预算金额")]
    [InlineData("invoice.receivableAmount")]
    public void SanitizeExternalVariable_Should_Hide_Sensitive_Business_Values(string key)
    {
        IntegrationNotificationService
            .SanitizeExternalVariable(key, "123456.78")
            .ShouldBe("[敏感字段已隐藏]");
    }

    [Theory]
    [InlineData("project.progress")]
    [InlineData("project.schedulePerformanceIndex")]
    [InlineData("project.costPerformanceIndex")]
    [InlineData("risk.highRiskCount")]
    public void SanitizeExternalVariable_Should_Keep_Non_Amount_Management_Metrics(string key)
    {
        IntegrationNotificationService
            .SanitizeExternalVariable(key, "0.95")
            .ShouldBe("0.95");
    }

    [Fact]
    public void SanitizeJsonVariable_Should_Redact_Sensitive_Fields_In_Object()
    {
        using var document = JsonDocument.Parse("""
                                                {
                                                  "projectName": "权限模型项目",
                                                  "progress": 80,
                                                  "contractAmount": 1200000,
                                                  "grossProfitRate": 0.35,
                                                  "earnedValue": {
                                                    "schedulePerformanceIndex": 0.96,
                                                    "costPerformanceIndex": 1.02
                                                  },
                                                  "receivableItems": [
                                                    {
                                                      "receivableAmount": 300000,
                                                      "plannedDate": "2026-06-30"
                                                    }
                                                  ]
                                                }
                                                """);

        var sanitized = IntegrationNotificationService.SanitizeJsonVariable("business", document.RootElement);

        using var sanitizedDocument = JsonDocument.Parse(sanitized);
        var root = sanitizedDocument.RootElement;
        root.GetProperty("projectName").GetString().ShouldBe("权限模型项目");
        root.GetProperty("progress").GetInt32().ShouldBe(80);
        root.GetProperty("earnedValue").GetProperty("schedulePerformanceIndex").GetDecimal().ShouldBe(0.96m);
        root.GetProperty("earnedValue").GetProperty("costPerformanceIndex").GetDecimal().ShouldBe(1.02m);
        root.GetProperty("contractAmount").GetString().ShouldBe("[敏感字段已隐藏]");
        root.GetProperty("grossProfitRate").GetString().ShouldBe("[敏感字段已隐藏]");
        root.GetProperty("receivableItems")[0].GetProperty("receivableAmount").GetString().ShouldBe("[敏感字段已隐藏]");
        root.GetProperty("receivableItems")[0].GetProperty("plannedDate").GetString().ShouldBe("2026-06-30");
    }
}
