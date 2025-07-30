// Copyright (c) Microsoft. All rights reserved.
using System.Collections.ObjectModel;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Resources;

namespace GettingStarted.AzureAgents;

/// <summary>
/// Standalone example demonstrating invoking Open API functions using <see cref="AzureAIAgent" />.
/// </summary>
/// <remarks>
/// Note: Open API invocation does not involve kernel function calling or kernel filters.
/// Azure Function invocation is managed entirely by the Azure AI Agent service.
/// </remarks>
public class Step06_AzureAIAgent_OpenAPI_Standalone
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
    }

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

        // Define sample metadata
        var sampleMetadata = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            { "Created", DateTime.UtcNow.ToString("u") },
            { "Sample", "Step06_AzureAIAgent_OpenAPI_Standalone" }
        });

        await UseOpenAPIToolWithAgent(client, config, sampleMetadata);
    }

    private static async Task UseOpenAPIToolWithAgent(AgentsClient client, AzureAIConfig config, ReadOnlyDictionary<string, string> sampleMetadata)
    {
        Console.WriteLine("=== UseOpenAPIToolWithAgent ===");
        
        // Retrieve Open API specifications
        string apiCountries = EmbeddedResource.Read("countries.json");
        string apiWeather = EmbeddedResource.Read("weather.json");

        // Define the agent
        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            config.ChatModelId,
            tools:
            [
                new OpenApiToolDefinition("RestCountries", "Retrieve country information", BinaryData.FromString(apiCountries), new OpenApiAnonymousAuthDetails()),
                new OpenApiToolDefinition("Weather", "Retrieve weather by location", BinaryData.FromString(apiWeather), new OpenApiAnonymousAuthDetails())
            ]);
        AzureAIAgent agent = new(definition, client);

        // Create a thread for the agent conversation.
        Microsoft.SemanticKernel.Agents.AgentThread thread = new AzureAIAgentThread(client, metadata: sampleMetadata);

        // Respond to user input
        try
        {
            await InvokeAgentAsync("What is the name and population of the country that uses currency with abbreviation THB");
            await InvokeAgentAsync("What is the weather in the capitol city of that country?");
        }
        finally
        {
            await thread.DeleteAsync();
            await client.Administration.DeleteAgentAsync(agent.Id);
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