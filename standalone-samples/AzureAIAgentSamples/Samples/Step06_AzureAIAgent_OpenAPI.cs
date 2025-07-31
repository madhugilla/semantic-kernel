// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating OpenAPI integration concepts with Azure OpenAI.
/// Shows how to work with API specifications and generate API-related content.
/// </summary>
public class Step06_AzureAIAgent_OpenAPI
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
    /// Run the sample demonstrating OpenAPI concepts.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 06: OpenAPI Integration Concepts ===");
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
            await GenerateOpenAPISpec(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await AnalyzeAPIEndpoint(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task GenerateOpenAPISpec(AzureAIConfig config)
    {
        Console.WriteLine("=== Generate OpenAPI Specification ===");
        
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are an API design expert. Generate OpenAPI specifications for web services.");

        string apiRequest = "Create an OpenAPI specification for a simple user management API with endpoints for creating, reading, updating, and deleting users";
        
        Console.WriteLine($"[User]: {apiRequest}");
        chatHistory.AddUserMessage(apiRequest);

        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }

    private static async Task AnalyzeAPIEndpoint(AzureAIConfig config)
    {
        Console.WriteLine("=== Analyze API Endpoint ===");
        
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are an API analysis expert. Analyze API endpoints and provide documentation and usage examples.");

        string apiEndpoint = "POST /api/users - Creates a new user with fields: name (string), email (string), age (number)";
        
        Console.WriteLine($"[User]: Analyze this API endpoint: {apiEndpoint}");
        chatHistory.AddUserMessage($"Analyze this API endpoint and provide example requests, responses, and potential error scenarios: {apiEndpoint}");

        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }
}