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
/// Demonstrate using code-interpreter on <see cref="AzureAIAgent"/> .
/// </summary>
public class Step03_AzureAIAgent_Vision
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

        // Upload an image
        await using Stream imageStream = EmbeddedResource.ReadStream("cat.jpg")!;
        PersistentAgentFileInfo fileInfo = await client.Files.UploadFileAsync(imageStream, PersistentAgentFilePurpose.Agents, "cat.jpg");

        // Define the agent
        PersistentAgent definition = await client.Administration.CreateAgentAsync(azureAIConfig.ChatModelId);
        AzureAIAgent agent = new(definition, client);

        // Create a thread for the agent conversation.
        AzureAIAgentThread thread = new(client, metadata: SampleMetadata);

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

        // Provide visibility for inner content (that isn't TextContent).
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation)
            {
                if (annotation.Kind == AnnotationKind.UrlCitation)
                {
                    Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: {annotation.ReferenceId} - {annotation.Title}");
                }
                else
                {
                    Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: File #{annotation.ReferenceId}");
                }
            }
            else if (item is ActionContent action)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {action.Text}");
            }
            else if (item is ReasoningContent reasoning)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {reasoning.Text.DefaultIfEmpty("Thinking...")}");
            }
            else if (item is FileReferenceContent fileReference)
            {
                Console.WriteLine($"  [{item.GetType().Name}] File #{fileReference.FileId}");
            }
            else if (item is ImageContent image)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes"}");
            }
            else if (item is FunctionCallContent functionCall)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionCall.Id}");
            }
            else if (item is FunctionResultContent functionResult)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionResult.CallId} - {functionResult.Result?.AsJson() ?? "*"}");
            }
        }

        string FormatAuthor() => message.AuthorName is not null ? $" - {message.AuthorName ?? " * "}" : string.Empty;
    }
}
