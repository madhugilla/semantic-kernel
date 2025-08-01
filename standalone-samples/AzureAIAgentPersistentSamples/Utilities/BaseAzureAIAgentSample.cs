// Copyright (c) Microsoft. All rights reserved.

using System.Collections.ObjectModel;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AzureAIAgentPersistentSamples.Utilities;

/// <summary>
/// Base class for Azure AI Agent samples that use the Persistent Agents API.
/// </summary>
public abstract class BaseAzureAIAgentSample
{
    /// <summary>
    /// Metadata to indicate the object was created for a sample.
    /// </summary>
    protected static readonly ReadOnlyDictionary<string, string> SampleMetadata =
        new(new Dictionary<string, string>
        {
            { "sample", "true" }
        });

    /// <summary>
    /// The Azure AI configuration.
    /// </summary>
    protected readonly AzureAIConfiguration Configuration;

    /// <summary>
    /// The Persistent Agents client.
    /// </summary>
    protected readonly PersistentAgentsClient Client;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAzureAIAgentSample"/> class.
    /// </summary>
    protected BaseAzureAIAgentSample()
    {
        Configuration = ConfigurationManager.GetAzureAIConfiguration();
        Client = AzureAIAgent.CreateAgentsClient(Configuration.Endpoint, new AzureCliCredential());
    }

    /// <summary>
    /// Creates an Azure AI agent with the specified parameters.
    /// </summary>
    protected async Task<AzureAIAgent> CreateAzureAIAgentAsync(
        string instructions,
        string? name = null,
        string? description = null,
        Kernel? kernel = null,
        IEnumerable<Azure.AI.Agents.Persistent.ToolDefinition>? tools = null)
    {
        PersistentAgent definition = await Client.Administration.CreateAgentAsync(
            Configuration.ChatModelId,
            name,
            description,
            instructions,
            tools);

        return new AzureAIAgent(definition, Client)
        {
            Kernel = kernel ?? new Kernel(),
        };
    }

    /// <summary>
    /// Invokes an agent with optional arguments and displays the response.
    /// </summary>
    protected async Task InvokeAgentAsync(AzureAIAgent agent, AgentThread thread, KernelArguments? arguments = null)
    {
        await foreach (ChatMessageContent response in agent.InvokeAsync(thread, new() { KernelArguments = arguments }))
        {
            WriteAgentResponse(response);
        }
    }

    /// <summary>
    /// Invokes an agent with a user message and displays the response.
    /// </summary>
    protected async Task InvokeAgentAsync(AzureAIAgent agent, AgentThread thread, string userMessage)
    {
        ChatMessageContent message = new(AuthorRole.User, userMessage);
        WriteAgentResponse(message);

        await foreach (ChatMessageContent response in agent.InvokeAsync(message, thread))
        {
            WriteAgentResponse(response);
        }
    }

    /// <summary>
    /// Writes an agent response to the console.
    /// </summary>
    protected static void WriteAgentResponse(ChatMessageContent message)
    {
        string authorExpression = message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";
        string contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content;
        
        Console.WriteLine($"\n# {message.Role.Label.ToUpperInvariant()}{authorExpression}: {contentExpression}");

        // Display inner content if available
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: File #{annotation.ReferenceId}");
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
                Console.WriteLine($"  [{item.GetType().Name}] {functionResult.CallId}");
            }
        }
    }

    /// <summary>
    /// Runs the sample.
    /// </summary>
    public abstract Task RunAsync();
}