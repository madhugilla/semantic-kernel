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
/// Standalone example demonstrating using <see cref="AzureAIAgent"/> with file search.
/// </summary>
public class Step05_AzureAIAgent_FileSearch_Standalone
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

        await UseFileSearchToolWithAgent(client, config);
    }

    private static async Task UseFileSearchToolWithAgent(AgentsClient client, AzureAIConfig config)
    {
        Console.WriteLine("=== UseFileSearchToolWithAgent ===");
        
        // Define sample metadata
        var sampleMetadataKey = "Test.Unit";
        var sampleMetadata = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            { "Created", DateTime.UtcNow.ToString("u") },
            { "Sample", "Step05_AzureAIAgent_FileSearch_Standalone" }
        });

        // Define the agent
        await using Stream stream = EmbeddedResource.ReadStream("employees.pdf")!;

        PersistentAgentFileInfo fileInfo = await client.Files.UploadFileAsync(stream, PersistentAgentFilePurpose.Agents, "employees.pdf");
        PersistentAgentsVectorStore fileStore =
            await client.VectorStores.CreateVectorStoreAsync(
                [fileInfo.Id],
                metadata: new Dictionary<string, string>() { { sampleMetadataKey, bool.TrueString } });
        PersistentAgent agentModel = await client.Administration.CreateAgentAsync(
            config.ChatModelId,
            tools: [new FileSearchToolDefinition()],
            toolResources: new()
            {
                FileSearch = new()
                {
                    VectorStoreIds = { fileStore.Id },
                }
            },
            metadata: new Dictionary<string, string>() { { sampleMetadataKey, bool.TrueString } });
        AzureAIAgent agent = new(agentModel, client);

        // Create a thread associated for the agent conversation.
        Microsoft.SemanticKernel.Agents.AgentThread thread = new AzureAIAgentThread(client, metadata: sampleMetadata);

        // Respond to user input
        try
        {
            await InvokeAgentAsync("Who is the youngest employee?");
            await InvokeAgentAsync("Who works in sales?");
            await InvokeAgentAsync("I have a customer request, who can help me?");
        }
        finally
        {
            await thread.DeleteAsync();
            await client.Administration.DeleteAgentAsync(agent.Id);
            await client.VectorStores.DeleteVectorStoreAsync(fileStore.Id);
            await client.Files.DeleteFileAsync(fileInfo.Id);
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