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
/// This example demonstrates how to declaratively create instances of <see cref="AzureAIAgent"/>.
/// </summary>
public class Step08_AzureAIAgent_Declarative
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

    /// <summary>
    /// Demonstrates creating and using a Chat Completion Agent with a Kernel.
    /// </summary>
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

        Console.WriteLine("=== Azure AI Agent Declarative Example ===");
        
        // Create kernel
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(client);
        builder.Services.AddSingleton<TokenCredential>(new AzureCliCredential());
        var kernel = builder.Build();

        // Simple declarative agent example
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

        await InvokeAgentAsync(agent!, "Hello! Can you help me with a simple task?");
    }

    /// <summary>
    /// Invoke the agent with the user input.
    /// </summary>
    private static async Task InvokeAgentAsync(Agent agent, string input)
    {
        Microsoft.SemanticKernel.Agents.AgentThread? agentThread = null;
        try
        {
            await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, input)))
            {
                agentThread = response.Thread;
                WriteAgentChatMessage(response.Message);
            }
        }
        finally
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
    {
        var text =
            """
            type: foundry_agent
            name: MyAgent
            description: My helpful agent.
            instructions: You are helpful agent.
            model:
              id: ${AzureOpenAI:ChatModelId}
            """;
        AzureAIAgentFactory factory = new();

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot);

        await InvokeAgentAsync(agent!, "Could you please create a bar chart for the operating profit using the following data and provide the file to me? Company A: $1.2 million, Company B: $2.5 million, Company C: $3.0 million, Company D: $1.8 million");
    }

    public static async Task Main()
    public async Task AzureAIAgentWithId()
    {
        var text =
            """
            id: ${AzureAI:AgentId}
            type: foundry_agent
            instructions: You are helpful agent who always responds in French.
            """;
        AzureAIAgentFactory factory = new();

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot);

        await InvokeAgentAsync(
            agent!,
            "Could you please create a bar chart for the operating profit using the following data and provide the file to me? Company A: $1.2 million, Company B: $2.5 million, Company C: $3.0 million, Company D: $1.8 million",
            deleteAgent: false);
    }

    public static async Task Main()
    public async Task AzureAIAgentWithCodeInterpreter()
    {
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

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot);

        await InvokeAgentAsync(agent!, "Use code to determine the values in the Fibonacci sequence that that are less then the value of 101?");
    }

    public static async Task Main()
    public async Task AzureAIAgentWithFunctions()
    {
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
        this._kernel.Plugins.Add(plugin);

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot);

        await InvokeAgentAsync(agent!, "What is the special soup and how much does it cost?");
    }

    public static async Task Main()
    public async Task AzureAIAgentWithBingGrounding()
    {
        var text =
            """
            type: foundry_agent
            name: BingAgent
            instructions: Answer questions using Bing to provide grounding context.
            description: This agent answers questions using Bing to provide grounding context.
            model:
              id: ${AzureAI:ChatModelId}
              options:
                temperature: 0.4
            tools:
              - type: bing_grounding
                options:
                  tool_connections:
                    - ${AzureAI:BingConnectionId}
            """;
        AzureAIAgentFactory factory = new();

        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        this._kernel.Plugins.Add(plugin);

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot);

        await InvokeAgentAsync(agent!, "What is the latest new about the Semantic Kernel?");
    }

    public static async Task Main()
    public async Task AzureAIAgentWithFileSearch()
    {
        var text =
            """
            type: foundry_agent
            name: FileSearchAgent
            instructions: Answer questions using available files to provide grounding context.
            description: This agent answers questions using available files to provide grounding context.
            model:
              id: ${AzureAI:ChatModelId}
              optisons:
                temperature: 0.4
            tools:
              - type: file_search
                description: Grounding with available files.
                options:
                  vector_store_ids:
                    - ${AzureAI.VectorStoreId}
            """;
        AzureAIAgentFactory factory = new();

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot);

        await InvokeAgentAsync(agent!, "What are the key features of the Semantic Kernel?");
    }

    public static async Task Main()
    public async Task AzureAIAgentWithOpenAPI()
    {
        var text =
            """
            type: foundry_agent
            name: WeatherAgent
            instructions: Answer questions about the weather. For all other questions politely decline to answer.
            description: This agent answers question about the weather.
            model:
              id: ${AzureAI:ChatModelId}
              options:
                temperature: 0.4
            tools:
              - type: openapi
                id: GetCurrentWeather
                description: Retrieves current weather data for a location based on wttr.in.
                options:
                  specification: |
                    {
                      "openapi": "3.1.0",
                      "info": {
                        "title": "Get Weather Data",
                        "description": "Retrieves current weather data for a location based on wttr.in.",
                        "version": "v1.0.0"
                      },
                      "servers": [
                        {
                          "url": "https://wttr.in"
                        }
                      ],
                      "auth": [],
                      "paths": {
                        "/{location}": {
                          "get": {
                            "description": "Get weather information for a specific location",
                            "operationId": "GetCurrentWeather",
                            "parameters": [
                              {
                                "name": "location",
                                "in": "path",
                                "description": "City or location to retrieve the weather for",
                                "required": true,
                                "schema": {
                                  "type": "string"
                                }
                              },
                              {
                                "name": "format",
                                "in": "query",
                                "description": "Always use j1 value for this parameter",
                                "required": true,
                                "schema": {
                                  "type": "string",
                                  "default": "j1"
                                }
                              }
                            ],
                            "responses": {
                              "200": {
                                "description": "Successful response",
                                "content": {
                                  "text/plain": {
                                    "schema": {
                                      "type": "string"
                                    }
                                  }
                                }
                              },
                              "404": {
                                "description": "Location not found"
                              }
                            },
                            "deprecated": false
                          }
                        }
                      },
                      "components": {
                        "schemes": {}
                      }
                    }
            """;
        AzureAIAgentFactory factory = new();

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot);

        await InvokeAgentAsync(agent!, "What is the current weather in Dublin?");
    }

    public static async Task Main()
    public async Task AzureAIAgentWithOpenAPIYaml()
    {
        var text =
            """
            type: foundry_agent
            name: WeatherAgent
            instructions: Answer questions about the weather. For all other questions politely decline to answer.
            description: This agent answers question about the weather.
            model:
              id: ${AzureAI:ChatModelId}
              options:
                temperature: 0.4
            tools:
              - type: openapi
                id: GetCurrentWeather
                description: Retrieves current weather data for a location based on wttr.in.
                options:
                  specification:
                    openapi: "3.1.0"  
                    info:  
                      title: "Get Weather Data"  
                      description: "Retrieves current weather data for a location based on wttr.in."  
                      version: "v1.0.0"  
                    servers:  
                      - url: "https://wttr.in"  
                    auth: []  
                    paths:  
                      /{location}:  
                        get:  
                          description: "Get weather information for a specific location"  
                          operationId: "GetCurrentWeather"  
                          parameters:  
                            - name: "location"  
                              in: "path"  
                              description: "City or location to retrieve the weather for"  
                              required: true  
                              schema:  
                                type: "string"  
                            - name: "format"  
                              in: "query"  
                              description: "Always use j1 value for this parameter"  
                              required: true  
                              schema:  
                                type: "string"  
                                default: "j1"  
                          responses:  
                            "200":  
                              description: "Successful response"  
                              content:  
                                text/plain:  
                                  schema:  
                                    type: "string"  
                            "404":  
                              description: "Location not found"  
                          deprecated: false  
                    components:  
                      schemes: {}  
            """;
        AzureAIAgentFactory factory = new();

        var agent = await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot);

        await InvokeAgentAsync(agent!, "What is the current weather in Dublin?");
    }

    public static async Task Main()
    public async Task AzureAIAgentWithTemplate()
    {
        var text =
            """
            type: foundry_agent
            name: StoryAgent
            description: A agent that generates a story about a topic.
            instructions: Tell a story about {{$topic}} that is {{$length}} sentences long.
            model:
              id: ${AzureAI:ChatModelId}
            inputs:
                topic:
                    description: The topic of the story.
                    required: true
                    default: Cats
                length:
                    description: The number of sentences in the story.
                    required: true
                    default: 2
            outputs:
                output1:
                    description: output1 description
            template:
                format: semantic-kernel
            """;
        AzureAIAgentFactory factory = new();
        var promptTemplateFactory = new KernelPromptTemplateFactory();

        var agent =
            await factory.CreateAgentFromYamlAsync(text, new() { Kernel = this._kernel }, TestConfiguration.ConfigurationRoot) ??
            throw new InvalidOperationException("Unable to create agent");

        var options = new AgentInvokeOptions()
        {
            KernelArguments = new()
            {
                { "topic", "Dogs" },
                { "length", "3" },
            }
        };

        Microsoft.SemanticKernel.Agents.AgentThread? agentThread = null;
        try
        {
            await foreach (var response in agent!.InvokeAsync(Array.Empty<ChatMessageContent>(), agentThread, options))
            {
                agentThread = response.Thread;
                this.WriteAgentChatMessage(response);
            }
        }
        finally
        {
            var azureaiAgent = (AzureAIAgent)agent;
            await azureaiAgent.Client.Administration.DeleteAgentAsync(azureaiAgent.Id);

            if (agentThread is not null)
            {
                await agentThread.DeleteAsync();
            }
        }
    }

    public Step08_AzureAIAgent_Declarative(ITestOutputHelper output) : base(output)
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(this.Client);
        builder.Services.AddSingleton(this.CreateFoundryProjectClient());
        this._kernel = builder.Build();
    }

    #region private
    private readonly Kernel _kernel;

    /// <summary>
    /// Invoke the agent with the user input.
    /// </summary>
    private async Task InvokeAgentAsync(Agent agent, string input, bool? deleteAgent = true)
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
                Assert.NotNull(azureaiAgent);
                await azureaiAgent.Client.Administration.DeleteAgentAsync(azureaiAgent.Id);

                if (agentThread is not null)
                {
                    await agentThread.DeleteAsync();
                }
            }
        }
    }
    #endregion
}
