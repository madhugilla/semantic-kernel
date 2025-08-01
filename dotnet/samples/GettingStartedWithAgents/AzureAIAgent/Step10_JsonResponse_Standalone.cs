// Copyright (c) Microsoft. All rights reserved.
using System.Collections.ObjectModel;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace GettingStarted.AzureAgents;

/// <summary>
/// Standalone example demonstrating parsing JSON response.
/// </summary>
public class Step10_JsonResponse_Standalone
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
    }

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
    /// Standalone main method to run the sample.
    /// </summary>
    public static async Task Main()
    {
        // Load configuration
        var configRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();

        var config = new AzureAIConfig();
        configRoot.GetSection("AzureAI").Bind(config);

        if (string.IsNullOrEmpty(config.Endpoint) || string.IsNullOrEmpty(config.ChatModelId))
        {
            Console.WriteLine("Azure AI configuration not found. Please set AzureAI:Endpoint and AzureAI:ChatModelId in appsettings.Development.json");
            return;
        }

        // Create Azure AI client
        var client = AzureAIAgent.CreateAgentsClient(new Uri(config.Endpoint), new AzureCliCredential());

        await UseJsonObjectResponse(client, config);
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        await UseJsonSchemaResponse(client, config);
    }

    private static async Task UseJsonObjectResponse(AgentsClient client, AzureAIConfig config)
    {
        Console.WriteLine("=== UseJsonObjectResponse ===");
        
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

    private static async Task UseJsonSchemaResponse(AgentsClient client, AzureAIConfig config)
    {
        Console.WriteLine("=== UseJsonSchemaResponse ===");
        
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

        try
        {
            await InvokeAgentAsync("The sunset is very colorful.");
            await InvokeAgentAsync("The sunset is setting over the mountains.");
            await InvokeAgentAsync("The sunset is setting over the mountains and filled the sky with a deep red flame, setting the clouds ablaze.");
        }
        finally
        {
            await thread.DeleteAsync();
            await agent.Client.Administration.DeleteAgentAsync(agent.Id);
        }

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
    /// Utility method for formatting agent chat messages.
    /// </summary>
    private static void WriteAgentChatMessage(ChatMessageContent message)
    {
        Console.WriteLine($"[{message.Role}]: {message.Content}");
        if (message.Metadata is { Count: > 0 })
        {
            Console.WriteLine($"[metadata]: {string.Join(", ", message.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        }
    }
}