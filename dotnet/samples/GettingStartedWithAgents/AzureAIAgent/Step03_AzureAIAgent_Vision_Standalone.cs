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
/// Standalone example demonstrating using vision capabilities on <see cref="AzureAIAgent"/> .
/// </summary>
public class Step03_AzureAIAgent_Vision_Standalone
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
            { "Sample", "Step03_AzureAIAgent_Vision_Standalone" }
        });

        await UseImageContentWithAgent(client, config, sampleMetadata);
    }

    private static async Task UseImageContentWithAgent(AgentsClient client, AzureAIConfig config, ReadOnlyDictionary<string, string> sampleMetadata)
    {
        Console.WriteLine("=== UseImageContentWithAgent ===");
        
        // Upload an image
        await using Stream imageStream = EmbeddedResource.ReadStream("cat.jpg")!;
        PersistentAgentFileInfo fileInfo = await client.Files.UploadFileAsync(imageStream, PersistentAgentFilePurpose.Agents, "cat.jpg");

        // Define the agent
        PersistentAgent definition = await client.Administration.CreateAgentAsync(config.ChatModelId);
        AzureAIAgent agent = new(definition, client);

        // Create a thread for the agent conversation.
        AzureAIAgentThread thread = new(client, metadata: sampleMetadata);

        // Respond to user input
        try
        {
            // Refer to public image by url
            await InvokeAgentAsync(CreateMessageWithImageUrl("Describe this image.", "https://upload.wikimedia.org/wikipedia/commons/thumb/4/47/New_york_times_square-terabass.jpg/1200px-New_york_times_square-terabass.jpg"));
            await InvokeAgentAsync(CreateMessageWithImageUrl("What are is the main color in this image?", "https://upload.wikimedia.org/wikipedia/commons/5/56/White_shark.jpg"));
            // Refer to uploaded image by file-id.
            await InvokeAgentAsync(CreateMessageWithImageReference("Is there an animal in this image?", fileInfo.Id));
        }
        finally
        {
            await thread.DeleteAsync();
            await client.Administration.DeleteAgentAsync(agent.Id);
            await client.Files.DeleteFileAsync(fileInfo.Id);
        }

        // Local function to invoke agent and display the conversation messages.
        async Task InvokeAgentAsync(ChatMessageContent input)
        {
            WriteAgentChatMessage(input);

            await foreach (ChatMessageContent response in agent.InvokeAsync(input, thread))
            {
                WriteAgentChatMessage(response);
            }
        }
    }

    private static ChatMessageContent CreateMessageWithImageUrl(string input, string url)
        => new(AuthorRole.User, [new TextContent(input), new ImageContent(new Uri(url))]);

    private static ChatMessageContent CreateMessageWithImageReference(string input, string fileId)
        => new(AuthorRole.User, [new TextContent(input), new FileReferenceContent(fileId)]);

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