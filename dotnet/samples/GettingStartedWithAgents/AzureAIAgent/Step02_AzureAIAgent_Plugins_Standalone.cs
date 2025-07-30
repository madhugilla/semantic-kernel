// Copyright (c) Microsoft. All rights reserved.
using System.Collections.ObjectModel;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Plugins;

namespace GettingStarted.AzureAgents;

/// <summary>
/// Standalone example demonstrating creation of <see cref="AzureAIAgent"/> with a <see cref="KernelPlugin"/>,
/// and then eliciting its response to explicit user messages.
/// </summary>
public class Step02_AzureAIAgent_Plugins_Standalone
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
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
            { "Sample", "Step02_AzureAIAgent_Plugins_Standalone" }
        });

        // Run all examples
        await UseAzureAgentWithPlugin(client, config, sampleMetadata);
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        await UseAzureAgentWithPluginEnumParameter(client, config, sampleMetadata);
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        await UseAzureAgentWithPromptFunction(client, config, sampleMetadata);
    }

    private static async Task UseAzureAgentWithPlugin(AgentsClient client, AzureAIConfig config, ReadOnlyDictionary<string, string> sampleMetadata)
    {
        Console.WriteLine("=== UseAzureAgentWithPlugin ===");
        
        // Define the agent
        AzureAIAgent agent = await CreateAzureAgentAsync(
                client, config,
                plugin: KernelPluginFactory.CreateFromType<MenuPlugin>(),
                instructions: "Answer questions about the menu.",
                name: "Host");

        // Create a thread for the agent conversation.
        AgentThread thread = new AzureAIAgentThread(client, metadata: sampleMetadata);

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

    private static async Task UseAzureAgentWithPluginEnumParameter(AgentsClient client, AzureAIConfig config, ReadOnlyDictionary<string, string> sampleMetadata)
    {
        Console.WriteLine("=== UseAzureAgentWithPluginEnumParameter ===");
        
        // Define the agent
        AzureAIAgent agent = await CreateAzureAgentAsync(client, config, plugin: KernelPluginFactory.CreateFromType<WidgetFactory>());

        // Create a thread for the agent conversation.
        AgentThread thread = new AzureAIAgentThread(client, metadata: sampleMetadata);

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

    private static async Task UseAzureAgentWithPromptFunction(AgentsClient client, AzureAIConfig config, ReadOnlyDictionary<string, string> sampleMetadata)
    {
        Console.WriteLine("=== UseAzureAgentWithPromptFunction ===");
        
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
        AzureAIAgentThread thread = new(client);

        // Respond to user input, invoking functions where appropriate.
        await InvokeAgentAsync(agent, thread, "Who would know naught of art must learn, act, and then take his ease.");
    }

    private static async Task<AzureAIAgent> CreateAzureAgentAsync(AgentsClient client, AzureAIConfig config, KernelPlugin? plugin = null, string? instructions = null, string? name = null)
    {
        // Define the agent
        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            config.ChatModelId,
            name,
            null,
            instructions);

        // Create kernel with chat completion
        var kernel = Kernel.CreateBuilder().Build();
        if (!string.IsNullOrEmpty(config.ApiKey))
        {
            kernel.Services.AddAzureOpenAIChatCompletion(config.ChatModelId, config.Endpoint, config.ApiKey);
        }
        else
        {
            kernel.Services.AddAzureOpenAIChatCompletion(config.ChatModelId, new Uri(config.Endpoint), new AzureCliCredential());
        }

        AzureAIAgent agent = new(definition, client)
        {
            Kernel = kernel,
        };

        // Add to the agent's Kernel
        if (plugin != null)
        {
            agent.Kernel.Plugins.Add(plugin);
        }

        return agent;
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