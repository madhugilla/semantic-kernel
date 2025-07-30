// Copyright (c) Microsoft. All rights reserved.
using System.Collections.ObjectModel;
using System.Text.Json;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace GettingStarted.AzureAgents;

/// <summary>
/// Demonstrate parsing JSON response.
/// </summary>
public class Step10_JsonResponse
{
    private const string TutorInstructions =
        """
        Think step-by-step and rate the user input on creativity and expressiveness from 1-100.

        Respond in JSON format with the following JSON schema:

        {
            "score": "integer (1-100)",
            "notes": "the reason for your score"
        }
        """;

    /// <summary>
    /// Metadata key to indicate the assistant as created for a sample.
    /// </summary>
    private const string SampleMetadataKey = "sksample";

    /// <summary>
    /// Metadata to indicate the object was created for a sample.
    /// </summary>
    private static readonly ReadOnlyDictionary<string, string> SampleMetadata =
        new(new Dictionary<string, string>
        {
            { SampleMetadataKey, bool.TrueString }
        });

    public class AzureAIConfig
    {
        public string ChatModelId { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string WorkflowEndpoint { get; set; } = string.Empty;
        public string BingConnectionId { get; set; } = string.Empty;
        public string VectorStoreId { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
    }

    public static async Task Main()
    {
        // Load configuration
        var configRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();

        var azureAIConfig = configRoot.GetSection("AzureAI").Get<AzureAIConfig>() ??
            throw new InvalidOperationException("AzureAI configuration not found.");

        // Create Azure AI Agent client
        var client = AzureAIAgent.CreateAgentsClient(azureAIConfig.Endpoint, new AzureCliCredential());

        Console.WriteLine("=== JSON Object Response ===");
        await UseJsonObjectResponse(client, azureAIConfig);

        Console.WriteLine("\n=== JSON Schema Response ===");
        await UseJsonSchemaResponse(client, azureAIConfig);
    }

    private static async Task UseJsonObjectResponse(Azure.AI.Agents.Persistent.PersistentAgentsClient client, AzureAIConfig config)
    {
        PersistentAgent definition =
            await client.Administration.CreateAgentAsync(
                config.ChatModelId,
                instructions: TutorInstructions,
                responseFormat:
                    BinaryData.FromString(
                        """
                        {
                            "type": "json_object"
                        }                        
                        """));

        AzureAIAgent agent = new(definition, client);

        await ExecuteAgent(agent);
    }

    private static async Task UseJsonSchemaResponse(Azure.AI.Agents.Persistent.PersistentAgentsClient client, AzureAIConfig config)
    {
        PersistentAgent definition =
            await client.Administration.CreateAgentAsync(
                config.ChatModelId,
                instructions: TutorInstructions,
                responseFormat: BinaryData.FromString(
                    """
                    {
                        "type": "json_schema",
                        "json_schema":
                        {
                          "type": "object",
                          "name": "scoring",
                          "schema": {
                              "type": "object",
                              "properties": {
                                  "score": {
                                      "type": "number"
                                  },
                                  "notes": {
                                      "type": "string"
                                  }
                              },
                              "required": [
                                  "score",
                                  "notes"
                              ],
                              "additionalProperties": false
                          },
                          "strict": true
                      }
                    }
                    """));

        AzureAIAgent agent = new(definition, client);

        await ExecuteAgent(agent);
    }

    private static async Task ExecuteAgent(AzureAIAgent agent)
    {
        AzureAIAgentThread thread = new(agent.Client);

        await InvokeAgentAsync("The sunset is very colorful.");
        await InvokeAgentAsync("The sunset is setting over the mountains.");
        await InvokeAgentAsync("The sunset is setting over the mountains and filled the sky with a deep red flame, setting the clouds ablaze.");

        // Local function to invoke agent and display the conversation messages.
        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            WriteAgentChatMessage(message);

            await foreach (ChatMessageContent response in agent.InvokeAsync(message, thread))
            {
                WriteAgentChatMessage(response);
            }
        }
    }

    /// <summary>
    /// Common method to write formatted agent chat content to the console.
    /// </summary>
    private static void WriteAgentChatMessage(ChatMessageContent message)
    {
        // Include ChatMessageContent.AuthorName in output, if present.
        string authorExpression = message.Role == AuthorRole.User ? string.Empty : FormatAuthor();
        // Include TextContent (via ChatMessageContent.Content), if present.
        string contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content;
        string codeMarker = " ";
        Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");

        string FormatAuthor() => message.AuthorName is not null ? $" - {message.AuthorName ?? " * "}" : string.Empty;
    }
}
