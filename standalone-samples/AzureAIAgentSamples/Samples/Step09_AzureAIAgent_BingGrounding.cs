// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating web search and grounding concepts with Azure OpenAI.
/// Shows how to enhance AI responses with web-based information.
/// </summary>
public class Step09_AzureAIAgent_BingGrounding
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
    /// Run the sample demonstrating search grounding concepts.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 09: Search Grounding Concepts ===");
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
            await SimulateGroundedResponse(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await DemonstrateFactChecking(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task SimulateGroundedResponse(AzureAIConfig config)
    {
        Console.WriteLine("=== Simulated Grounded Response ===");
        
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
            You are an AI assistant that provides information based on reliable sources. 
            When answering questions, acknowledge when information might be time-sensitive or when you should recommend checking current sources.
            Always be transparent about the limitations of your knowledge cutoff.
            """);

        string[] questions = [
            "What are the current stock prices for major tech companies?",
            "What's the weather like today in Seattle?",
            "What are the latest developments in artificial intelligence?"
        ];

        foreach (var question in questions)
        {
            Console.WriteLine($"[User]: {question}");
            chatHistory.AddUserMessage(question);

            var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
            Console.WriteLine($"[Assistant]: {response.Content}");
            chatHistory.AddAssistantMessage(response.Content ?? "");
            Console.WriteLine();
        }
    }

    private static async Task DemonstrateFactChecking(AzureAIConfig config)
    {
        Console.WriteLine("=== Fact-Checking and Source Verification ===");
        
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("""
            You are a fact-checking assistant. When presented with claims or statements:
            1. Identify the key factual claims
            2. Assess their verifiability
            3. Suggest reliable sources for verification
            4. Note any potential biases or context needed
            Always recommend consulting primary sources and multiple viewpoints for important decisions.
            """);

        string claim = "Studies show that drinking 8 glasses of water per day is essential for optimal health and is recommended by all major health organizations.";
        
        Console.WriteLine($"[User]: Please fact-check this claim: {claim}");
        chatHistory.AddUserMessage($"Please analyze and fact-check this claim: {claim}");

        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();

        // Follow-up question about sources
        Console.WriteLine("[User]: What sources would you recommend for verifying health claims like this?");
        chatHistory.AddUserMessage("What sources would you recommend for verifying health claims like this?");

        var followupResponse = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {followupResponse.Content}");
        Console.WriteLine();
    }
}