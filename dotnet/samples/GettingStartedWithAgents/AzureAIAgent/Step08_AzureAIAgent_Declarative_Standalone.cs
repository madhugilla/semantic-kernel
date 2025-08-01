// Copyright (c) Microsoft. All rights reserved.
using System.Collections.ObjectModel;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Plugins;

namespace GettingStarted.AzureAgents;

/// <summary>
/// Standalone example demonstrating how to declaratively create instances of <see cref="AzureAIAgent"/>.
/// </summary>
public class Step08_AzureAIAgent_Declarative_Standalone
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public string BingConnectionId { get; set; } = string.Empty;
        public string VectorStoreId { get; set; } = string.Empty;
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

        // Create kernel with services
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(client);
        builder.Services.AddSingleton<TokenCredential>(new AzureCliCredential());
        var kernel = builder.Build();

        // Run different declarative examples
        await AzureAIAgentWithConfiguration(client, config, configRoot, kernel);
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        await AzureAIAgentWithCodeInterpreter(client, config, configRoot, kernel);
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        await AzureAIAgentWithFunctions(client, config, configRoot, kernel);
    }

    private static async Task AzureAIAgentWithConfiguration(AgentsClient client, AzureAIConfig config, IConfigurationRoot configRoot, Kernel kernel)
    {
        Console.WriteLine("=== AzureAIAgentWithConfiguration ===");
        
        var text =
            """
            type: foundry_agent
            name: MyAgent
            description: My helpful agent.
            instructions: You are helpful agent.
            model:
              id: ${AzureAI:ChatModelId}
              connection:
                connection_string: ${AzureAI:ConnectionString}
            """;
        AzureAIAgentFactory factory = new();

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = kernel }, configRoot);

        await InvokeAgentAsync(agent!, "Could you please create a bar chart for the operating profit using the following data and provide the file to me? Company A: $1.2 million, Company B: $2.5 million, Company C: $3.0 million, Company D: $1.8 million");
    }

    private static async Task AzureAIAgentWithCodeInterpreter(AgentsClient client, AzureAIConfig config, IConfigurationRoot configRoot, Kernel kernel)
    {
        Console.WriteLine("=== AzureAIAgentWithCodeInterpreter ===");
        
        var text =
            """
            type: foundry_agent
            name: CodeInterpreterAgent
            instructions: Use the code interpreter tool to answer questions which require code to be generated and executed.
            description: Agent with code interpreter tool.
            model:
              id: ${AzureAI:ChatModelId}
            tools:
              - type: code_interpreter
            """;
        AzureAIAgentFactory factory = new();

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = kernel }, configRoot);

        await InvokeAgentAsync(agent!, "Use code to determine the values in the Fibonacci sequence that that are less then the value of 101?");
    }

    private static async Task AzureAIAgentWithFunctions(AgentsClient client, AzureAIConfig config, IConfigurationRoot configRoot, Kernel kernel)
    {
        Console.WriteLine("=== AzureAIAgentWithFunctions ===");
        
        var text =
            """
            type: foundry_agent
            name: FunctionCallingAgent
            instructions: Use the provided functions to answer questions about the menu.
            description: This agent uses the provided functions to answer questions about the menu.
            model:
              id: ${AzureAI:ChatModelId}
              options:
                temperature: 0.4
            tools:
              - id: GetSpecials
                type: function
                description: Get the specials from the menu.
              - id: GetItemPrice
                type: function
                description: Get the price of an item on the menu.
                options:
                  parameters:
                    - name: menuItem
                      type: string
                      required: true
                      description: The name of the menu item.  
            """;
        AzureAIAgentFactory factory = new();

        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        kernel.Plugins.Add(plugin);

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = kernel }, configRoot);

        await InvokeAgentAsync(agent!, "What is the special soup and how much does it cost?");
    }

    /// <summary>
    /// Invoke the agent with the user input.
    /// </summary>
    private static async Task InvokeAgentAsync(Agent agent, string input, bool? deleteAgent = true)
    {
        Microsoft.SemanticKernel.Agents.AgentThread? agentThread = null;
        try
        {
            await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, input)))
            {
                agentThread = response.Thread;
                WriteAgentChatMessage(response);
            }
        }
        finally
        {
            if (deleteAgent ?? true)
            {
                var azureaiAgent = agent as AzureAIAgent;
                if (azureaiAgent != null)
                {
                    await azureaiAgent.Client.Administration.DeleteAgentAsync(azureaiAgent.Id);

                    if (agentThread is not null)
                    {
                        await agentThread.DeleteAsync();
                    }
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