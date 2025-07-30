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
/// Demonstrate using Bing grounding on <see cref="AzureAIAgent"/> .
/// </summary>
public class Step09_AzureAIAgent_BingGrounding
{
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

        // Access the BingGrounding connection (simplified for standalone use)
        string connectionId = azureAIConfig.BingConnectionId;
        BingGroundingSearchConfiguration bingToolConfiguration = new(connectionId);
        BingGroundingSearchToolParameters bingToolParameters = new([bingToolConfiguration]);
        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            azureAIConfig.ChatModelId,
            tools: [new BingGroundingToolDefinition(bingToolParameters)]);
        AzureAIAgent agent = new(definition, client);

        // Create a thread for the agent conversation.
        AzureAIAgentThread thread = new(client, metadata: SampleMetadata);

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
