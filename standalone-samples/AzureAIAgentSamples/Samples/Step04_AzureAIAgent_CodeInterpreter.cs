// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating code analysis and execution concepts with Azure OpenAI.
/// Shows how to work with code-related tasks using Semantic Kernel.
/// </summary>
public class Step04_AzureAIAgent_CodeInterpreter
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private sealed class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Run the sample demonstrating code interpretation concepts.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 04: Code Analysis and Generation ===");
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
            await AnalyzeCode(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await GenerateCode(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task AnalyzeCode(AzureAIConfig config)
    {
        Console.WriteLine("=== Code Analysis ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a code analysis expert. Analyze code for bugs, improvements, and explain what it does.");

        string sampleCode = """
            def fibonacci(n):
                if n <= 1:
                    return n
                else:
                    return fibonacci(n-1) + fibonacci(n-2)
            
            for i in range(10):
                print(f"F({i}) = {fibonacci(i)}")
            """;

        Console.WriteLine($"[User]: Analyze this Python code:\n{sampleCode}");
        chatHistory.AddUserMessage($"Analyze this Python code for efficiency and suggest improvements:\n\n```python\n{sampleCode}\n```");

        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }

    private static async Task GenerateCode(AzureAIConfig config)
    {
        Console.WriteLine("=== Code Generation ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a programming assistant. Generate clean, well-documented code based on requirements.");

        string requirement = "Create a C# method that calculates the area of different shapes (circle, rectangle, triangle)";
        
        Console.WriteLine($"[User]: {requirement}");
        chatHistory.AddUserMessage(requirement);

        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }
}