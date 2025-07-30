// Copyright (c) Microsoft. All rights reserved.
using System.Collections.ObjectModel;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Plugins;

namespace GettingStarted.AzureAgents;

/// <summary>
/// Demonstrate creation of <see cref="AzureAIAgent"/> with a <see cref="KernelPlugin"/>,
/// and then eliciting its response to explicit user messages.
/// </summary>
public class Step02_AzureAIAgent_Plugins
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

        Console.WriteLine("=== Using Azure Agent with Plugin ===");
        await UseAzureAgentWithPlugin(client, azureAIConfig);

        Console.WriteLine("\n=== Using Azure Agent with Plugin Enum Parameter ===");
        await UseAzureAgentWithPluginEnumParameter(client, azureAIConfig);

        Console.WriteLine("\n=== Using Azure Agent with Prompt Function ===");
        await UseAzureAgentWithPromptFunction(client, azureAIConfig);
    }

    private static async Task UseAzureAgentWithPlugin(PersistentAgentsClient client, AzureAIConfig config)
    {
        // Define the agent
        AzureAIAgent agent = await CreateAzureAgentAsync(
                client, config,
                plugin: KernelPluginFactory.CreateFromType<MenuPlugin>(),
                instructions: "Answer questions about the menu.",
                name: "Host");

        // Create a thread for the agent conversation.
        AgentThread thread = new AzureAIAgentThread(client, metadata: SampleMetadata);

        // Respond to user input
        try
        {
            await InvokeAgentAsync(agent, thread, "Hello");
            await InvokeAgentAsync(agent, thread, "What is the special soup and its price?");
            await InvokeAgentAsync(agent, thread, "What is the special drink and its price?");
            await InvokeAgentAsync(agent, thread, "Thank you");
        }
        finally
        {
            await thread.DeleteAsync();
            await client.Administration.DeleteAgentAsync(agent.Id);
        }
    }

    private static async Task UseAzureAgentWithPluginEnumParameter(PersistentAgentsClient client, AzureAIConfig config)
    {
        // Define the agent
        AzureAIAgent agent = await CreateAzureAgentAsync(client, config, plugin: KernelPluginFactory.CreateFromType<WidgetFactory>());

        // Create a thread for the agent conversation.
        AgentThread thread = new AzureAIAgentThread(client, metadata: SampleMetadata);

        // Respond to user input
        try
        {
            await InvokeAgentAsync(agent, thread, "Create a beautiful red colored widget for me.");
        }
        finally
        {
            await thread.DeleteAsync();
            await client.Administration.DeleteAgentAsync(agent.Id);
        }
    }

    private static async Task UseAzureAgentWithPromptFunction(PersistentAgentsClient client, AzureAIConfig config)
    {
        // Define prompt function
        KernelFunction promptFunction =
            KernelFunctionFactory.CreateFromPrompt(
                promptTemplate:
                    """
                    Count the number of vowels in INPUT and report as a markdown table.

                    INPUT:
                    {{$input}}
                    """,
                description: "Counts the number of vowels");

        // Define the agent
        AzureAIAgent agent =
            await CreateAzureAgentAsync(
                client, config,
                KernelPluginFactory.CreateFromFunctions("AgentPlugin", [promptFunction]),
                instructions: "You job is to only and always analyze the vowels in the user input without confirmation.");

        // Add a filter to the agent's kernel to log function invocations.
        agent.Kernel.FunctionInvocationFilters.Add(new PromptFunctionFilter());

        // Create the chat history thread to capture the agent interaction.
        AzureAIAgentThread thread = new(agent.Client);

        // Respond to user input, invoking functions where appropriate.
        await InvokeAgentAsync(agent, thread, "Who would know naught of art must learn, act, and then take his ease.");
    }

    private static async Task<AzureAIAgent> CreateAzureAgentAsync(PersistentAgentsClient client, AzureAIConfig config, KernelPlugin plugin, string? instructions = null, string? name = null)
    {
        // Define the agent
        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            config.ChatModelId,
            name,
            null,
            instructions);

        AzureAIAgent agent =
            new(definition, client)
            {
                Kernel = CreateKernelWithChatCompletion(config),
            };

        // Add to the agent's Kernel
        if (plugin != null)
        {
            agent.Kernel.Plugins.Add(plugin);
        }

        return agent;
    }

    private static Kernel CreateKernelWithChatCompletion(AzureAIConfig config)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            config.ChatModelId,
            config.Endpoint,
            new AzureCliCredential());
        return builder.Build();
    }

    // Local function to invoke agent and display the conversation messages.
    private static async Task InvokeAgentAsync(AzureAIAgent agent, AgentThread thread, string input)
    {
        ChatMessageContent message = new(AuthorRole.User, input);
        WriteAgentChatMessage(message);

        await foreach (ChatMessageContent response in agent.InvokeAsync(message, thread))
        {
            WriteAgentChatMessage(response);
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

    private sealed class PromptFunctionFilter : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            Console.WriteLine($"\nINVOKING: {context.Function.Name}");
            await next.Invoke(context);
            Console.WriteLine($"\nRESULT: {context.Result}");
        }
    }
}
