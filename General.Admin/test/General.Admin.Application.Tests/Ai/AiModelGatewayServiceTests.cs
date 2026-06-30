using System;
using System.Collections.Generic;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace General.Admin.Ai;

public class AiModelGatewayServiceTests
{
    [Fact]
    public void ParseChatCompletion_Should_Read_OpenAi_Tool_Calls()
    {
        const string responseJson = """
                                    {
                                      "model": "qwen-plus",
                                      "choices": [
                                        {
                                          "finish_reason": "tool_calls",
                                          "message": {
                                            "role": "assistant",
                                            "content": null,
                                            "tool_calls": [
                                              {
                                                "id": "call_001",
                                                "type": "function",
                                                "function": {
                                                  "name": "project__dot__overview__dot__read",
                                                  "arguments": "{\"projectId\":\"50000000-0000-0000-0000-000000000001\"}"
                                                }
                                              }
                                            ]
                                          }
                                        }
                                      ],
                                      "usage": {
                                        "prompt_tokens": 12,
                                        "completion_tokens": 6
                                      }
                                    }
                                    """;

        var result = AiModelGatewayService.ParseChatCompletion(responseJson);

        result.Success.ShouldBeTrue();
        result.Content.ShouldBeEmpty();
        result.FinishReason.ShouldBe("tool_calls");
        result.PromptTokens.ShouldBe(12);
        result.CompletionTokens.ShouldBe(6);
        result.ToolCalls.Count.ShouldBe(1);
        result.ToolCalls[0].Id.ShouldBe("call_001");
        result.ToolCalls[0].Name.ShouldBe("project__dot__overview__dot__read");
        result.ToolCalls[0].ArgumentsJson.ShouldBe("{\"projectId\":\"50000000-0000-0000-0000-000000000001\"}");
    }

    [Fact]
    public void ParseChatCompletion_Should_Normalize_Invalid_Tool_Arguments()
    {
        const string responseJson = """
                                    {
                                      "choices": [
                                        {
                                          "message": {
                                            "tool_calls": [
                                              {
                                                "id": "call_invalid",
                                                "function": {
                                                  "name": "project__dot__raid__dot__read",
                                                  "arguments": "not-json"
                                                }
                                              }
                                            ]
                                          }
                                        }
                                      ]
                                    }
                                    """;

        var result = AiModelGatewayService.ParseChatCompletion(responseJson);

        result.Success.ShouldBeTrue();
        result.ToolCalls.Count.ShouldBe(1);
        result.ToolCalls[0].ArgumentsJson.ShouldBe("{}");
    }

    [Fact]
    public void BuildToolPayloads_Should_Limit_And_Normalize_Schemas()
    {
        var tools = new List<AiGatewayToolDefinition>();
        for (var index = 1; index <= 25; index++)
        {
            tools.Add(new AiGatewayToolDefinition(
                $"tool_{index}",
                $"测试工具 {index}",
                index == 1 ? "{\"type\":\"object\",\"properties\":{\"projectId\":{\"type\":\"string\"}}}" : "bad-json"));
        }

        var payloads = AiModelGatewayService.BuildToolPayloads(tools);

        payloads.Count.ShouldBe(20);
        var firstPayloadJson = JsonSerializer.Serialize(payloads[0], new JsonSerializerOptions(JsonSerializerDefaults.Web));
        firstPayloadJson.ShouldContain("\"type\":\"function\"");
        firstPayloadJson.ShouldContain("\"name\":\"tool_1\"");
        firstPayloadJson.ShouldContain("\"projectId\"");

        var secondPayloadJson = JsonSerializer.Serialize(payloads[1], new JsonSerializerOptions(JsonSerializerDefaults.Web));
        secondPayloadJson.ShouldContain("\"type\":\"object\"");
    }

    [Fact]
    public void BuildAnthropicToolPayloads_Should_Use_Input_Schema()
    {
        var payloads = AiModelGatewayService.BuildAnthropicToolPayloads([
            new AiGatewayToolDefinition(
                "project__dot__overview__dot__read",
                "读取项目概览",
                "{\"type\":\"object\",\"properties\":{\"projectId\":{\"type\":\"string\"}}}")
        ]);

        var payloadJson = JsonSerializer.Serialize(payloads[0], new JsonSerializerOptions(JsonSerializerDefaults.Web));

        payloads.Count.ShouldBe(1);
        payloadJson.ShouldContain("\"name\":\"project__dot__overview__dot__read\"");
        payloadJson.ShouldContain("\"input_schema\"");
        payloadJson.ShouldContain("\"projectId\"");
    }

    [Fact]
    public void ParseAnthropicMessage_Should_Read_Tool_Use()
    {
        const string responseJson = """
                                    {
                                      "id": "msg_001",
                                      "type": "message",
                                      "role": "assistant",
                                      "model": "deepseek-v4-pro",
                                      "stop_reason": "tool_use",
                                      "content": [
                                        {
                                          "type": "tool_use",
                                          "id": "toolu_001",
                                          "name": "project__dot__overview__dot__read",
                                          "input": {
                                            "projectId": "50000000-0000-0000-0000-000000000001"
                                          }
                                        }
                                      ],
                                      "usage": {
                                        "input_tokens": 21,
                                        "output_tokens": 7
                                      }
                                    }
                                    """;

        var result = AiModelGatewayService.ParseAnthropicMessage(responseJson);

        result.Success.ShouldBeTrue();
        result.Content.ShouldBeEmpty();
        result.FinishReason.ShouldBe("tool_use");
        result.PromptTokens.ShouldBe(21);
        result.CompletionTokens.ShouldBe(7);
        result.ToolCalls.Count.ShouldBe(1);
        result.ToolCalls[0].Id.ShouldBe("toolu_001");
        result.ToolCalls[0].Name.ShouldBe("project__dot__overview__dot__read");
        result.ToolCalls[0].ArgumentsJson.ShouldBe("{\"projectId\":\"50000000-0000-0000-0000-000000000001\"}");
    }

    [Fact]
    public void BuildEndpoint_Should_Append_Anthropic_V1_Messages_For_DeepSeek_Base_Url()
    {
        var provider = new AiProvider(
            Guid.NewGuid(),
            "deepseek",
            AiProviderProtocolNames.AnthropicMessages,
            "https://api.deepseek.com/anthropic",
            "cipher",
            null);

        var endpoint = AiModelGatewayService.BuildEndpoint(provider, "messages");

        endpoint.ShouldBe("https://api.deepseek.com/anthropic/v1/messages");
    }

    [Fact]
    public void ResolveBuiltInModels_Should_Return_DeepSeek_Anthropic_Models()
    {
        var provider = new AiProvider(
            Guid.NewGuid(),
            "DeepSeek",
            AiProviderProtocolNames.AnthropicMessages,
            "https://api.deepseek.com/anthropic",
            "cipher",
            null);

        var models = AiModelGatewayService.ResolveBuiltInModels(provider);

        models.Count.ShouldBe(2);
        models[0].ModelCode.ShouldBe("deepseek-v4-pro");
        models[1].ModelCode.ShouldBe("deepseek-v4-flash");
        models.ShouldAllBe(item => item.ModelType == "standard");
    }

    [Fact]
    public void FromGatewayToolName_Should_Restore_Tool_Code()
    {
        AiToolRegistryService
            .FromGatewayToolName("business__dot__contract__dot__budget__dot__receivable__dot__read")
            .ShouldBe("business.contract.budget.receivable.read");
    }
}
