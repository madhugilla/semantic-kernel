// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Plugins;
using AzureAIAgentSamples.Utilities;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating Semantic Kernel with plugins using Azure OpenAI,
/// showing how to create and use plugins for interactive chat experiences.
/// </summary>
public class Step02_AzureAIAgent_Plugins
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private sealed class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Run the sample demonstrating Azure OpenAI with plugins.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 02: Azure OpenAI with Plugins ===");
        Console.WriteLine();

        // Load configuration
        var configRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .AddEnvironmentVariables()
            .Build();

        var config = new AzureAIConfig();
        configRoot.GetSection("AzureAI").Bind(config);

        if (string.IsNullOrEmpty(config.Endpoint) || string.IsNullOrEmpty(config.ChatModelId))
        {
            Console.WriteLine("Azure AI configuration not found. Please set AzureAI:Endpoint and AzureAI:ChatModelId in appsettings.json");
            return;
        }

        try
        {
            // Run all examples
            await UseSemanticKernelWithMenuPlugin(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await UseSemanticKernelWithWidgetFactory(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await UseSemanticKernelWithPromptFunction(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task UseSemanticKernelWithMenuPlugin(AzureAIConfig config)
    {
        Console.WriteLine("=== Using Semantic Kernel with Menu Plugin ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());

        // Add the menu plugin
        builder.Plugins.Add(KernelPluginFactory.CreateFromType<MenuPlugin>());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Create a chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a helpful restaurant host. Answer questions about the menu using the available functions.");

        // Simulate a conversation
        string[] userMessages = [
            "Hello",
            "What is the special soup and its price?",
            "What is the special drink and its price?",
            "Thank you"
        ];

        foreach (var userMessage in userMessages)
        {
            Console.WriteLine($"[User]: {userMessage}");
            chatHistory.AddUserMessage(userMessage);

            // Get response with function calling enabled
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                new AzureOpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
                kernel);

            Console.WriteLine($"[Assistant]: {response.Content}");
            chatHistory.AddAssistantMessage(response.Content ?? "");
            Console.WriteLine();
        }
    }

    private static async Task UseSemanticKernelWithWidgetFactory(AzureAIConfig config)
    {
        Console.WriteLine("=== Using Semantic Kernel with Widget Factory Plugin ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());

        // Add the widget factory plugin
        builder.Plugins.Add(KernelPluginFactory.CreateFromType<WidgetFactory>());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Create a chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a helpful widget factory assistant. Create widgets based on user requests using the available functions.");

        Console.WriteLine("[User]: Create a beautiful red colored widget for me.");
        chatHistory.AddUserMessage("Create a beautiful red colored widget for me.");

        // Get response with function calling enabled
        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            new AzureOpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
            kernel);

        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }

    private static async Task UseSemanticKernelWithPromptFunction(AzureAIConfig config)
    {
        Console.WriteLine("=== Using Semantic Kernel with Custom Prompt Function ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());

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

        // Add the prompt function as a plugin
        builder.Plugins.Add(KernelPluginFactory.CreateFromFunctions("VowelPlugin", [promptFunction]));
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Create a chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a text analysis assistant. Analyze text using the available vowel counting function.");

        string userMessage = "Who would know naught of art must learn, act, and then take his ease.";
        Console.WriteLine($"[User]: {userMessage}");
        chatHistory.AddUserMessage(userMessage);

        // Get response with function calling enabled
        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            new AzureOpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
            kernel);

        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }
}