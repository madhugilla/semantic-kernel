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
/// Standalone example demonstrating how to define function tools for an <see cref="AzureAIAgent"/>
/// when the agent is created. This is useful if you want to retrieve the agent later and
/// then dynamically check what function tools it requires.
/// </summary>
public class Step07_AzureAIAgent_Functions_Standalone
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
    }

    private const string HostName = "Host";
    private const string HostInstructions = "Answer questions about the menu.";

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
            { "Sample", "Step07_AzureAIAgent_Functions_Standalone" }
        });

        await UseSingleAgentWithFunctionTools(client, config, sampleMetadata);
    }

    private static async Task UseSingleAgentWithFunctionTools(AgentsClient client, AzureAIConfig config, ReadOnlyDictionary<string, string> sampleMetadata)
    {
        Console.WriteLine("=== UseSingleAgentWithFunctionTools ===");
        
        // Define the agent
        // In this sample the function tools are added to the agent this is
        // important if you want to retrieve the agent later and then dynamically check
        // what function tools it requires.
        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        var tools = plugin.Select(f => f.ToToolDefinition(plugin.Name));

        PersistentAgent definition = await client.Administration.CreateAgentAsync(
            model: config.ChatModelId,
            name: HostName,
            description: null,
            instructions: HostInstructions,
            tools: tools);
        AzureAIAgent agent = new(definition, client);

        // Add plugin to the agent's Kernel (same as direct Kernel usage).
        agent.Kernel.Plugins.Add(plugin);

        // Create a thread for the agent conversation.
        AgentThread thread = new AzureAIAgentThread(client, metadata: sampleMetadata);

        // Respond to user input
        try
        {
            await InvokeAgentAsync("Hello");
            await InvokeAgentAsync("What is the special soup and its price?");
            await InvokeAgentAsync("What is the special drink and its price?");
            await InvokeAgentAsync("Thank you");
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