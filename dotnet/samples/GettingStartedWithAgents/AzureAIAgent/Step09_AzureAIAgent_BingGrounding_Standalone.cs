// Copyright (c) Microsoft. All rights reserved.
using System.Collections.ObjectModel;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace GettingStarted.AzureAgents;

/// <summary>
/// Standalone example demonstrating using Bing grounding on <see cref="AzureAIAgent"/> .
/// </summary>
public class Step09_AzureAIAgent_BingGrounding_Standalone
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
        public string BingConnectionId { get; set; } = string.Empty;
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

        if (string.IsNullOrEmpty(config.BingConnectionId))
        {
            Console.WriteLine("Bing connection ID not found. Please set AzureAI:BingConnectionId in appsettings.Development.json");
            return;
        }

        // Create Azure AI client
        var client = AzureAIAgent.CreateAgentsClient(new Uri(config.Endpoint), new AzureCliCredential());

        // Define sample metadata
        var sampleMetadata = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            { "Created", DateTime.UtcNow.ToString("u") },
            { "Sample", "Step09_AzureAIAgent_BingGrounding_Standalone" }
        });

        await UseBingGroundingToolWithAgent(client, config, sampleMetadata);
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        await UseBingGroundingToolWithStreaming(client, config, sampleMetadata);
    }

    private static async Task UseBingGroundingToolWithAgent(AgentsClient client, AzureAIConfig config, ReadOnlyDictionary<string, string> sampleMetadata)
    {
        Console.WriteLine("=== UseBingGroundingToolWithAgent ===");
        
        // Access the BingGrounding connection
        string connectionId = config.BingConnectionId;
        BingGroundingSearchConfiguration bingToolConfiguration = new(connectionId);
        BingGroundingSearchToolParameters bingToolParameters = new([bingToolConfiguration]);
        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            config.ChatModelId,
            tools: [new BingGroundingToolDefinition(bingToolParameters)]);
        AzureAIAgent agent = new(definition, client);

        // Create a thread for the agent conversation.
        AzureAIAgentThread thread = new(client, metadata: sampleMetadata);

        // Respond to user input
        try
        {
            //await InvokeAgentAsync("How does wikipedia explain Euler's Identity?");
            await InvokeAgentAsync("What is the current price of gold?");
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

    private static async Task UseBingGroundingToolWithStreaming(AgentsClient client, AzureAIConfig config, ReadOnlyDictionary<string, string> sampleMetadata)
    {
        Console.WriteLine("=== UseBingGroundingToolWithStreaming ===");
        
        // Access the BingGrounding connection
        string connectionId = config.BingConnectionId;
        BingGroundingSearchConfiguration bingToolConfiguration = new(connectionId);
        BingGroundingSearchToolParameters bingToolParameters = new([bingToolConfiguration]);

        // Define the agent
        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            config.ChatModelId,
            tools: [new BingGroundingToolDefinition(bingToolParameters)]);
        AzureAIAgent agent = new(definition, client);

        // Create a thread for the agent conversation.
        AzureAIAgentThread thread = new(client, metadata: sampleMetadata);

        // Respond to user input
        try
        {
            await InvokeAgentAsync("What is the current price of gold?");

            // Display chat history
            Console.WriteLine("\n================================");
            Console.WriteLine("CHAT HISTORY");
            Console.WriteLine("================================");

            await foreach (ChatMessageContent message in thread.GetMessagesAsync())
            {
                WriteAgentChatMessage(message);
            }
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

            bool isFirst = false;
            await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(message, thread))
            {
                if (!isFirst)
                {
                    Console.WriteLine($"\n# {response.Role} - {response.AuthorName ?? "*"}:");
                    isFirst = true;
                }

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    Console.WriteLine($"\t> streamed: {response.Content}");
                }

                foreach (StreamingAnnotationContent? annotation in response.Items.OfType<StreamingAnnotationContent>())
                {
                    Console.WriteLine($"\t            {annotation.ReferenceId} - {annotation.Title}");
                }
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