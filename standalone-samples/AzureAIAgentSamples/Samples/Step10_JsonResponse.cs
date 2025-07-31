// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;
using System.Text.Json;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating JSON response formatting with Azure OpenAI.
/// Shows how to get structured JSON responses from the AI model.
/// </summary>
public class Step10_JsonResponse
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
    /// Response model for creativity scoring.
    /// </summary>
    private sealed class CreativityScore
    {
        public int Score { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    private const string TutorInstructions =
        """
        Think step-by-step and rate the user input on creativity and expressiveness from 1-100.

        Respond in JSON format with the following JSON schema:

        {
            "score": "integer (1-100)",
            "notes": "the reason for your score"
        }
        """;

    /// <summary>
    /// Run the sample demonstrating JSON response formatting.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 10: JSON Response Formatting ===");
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
            await UseJsonObjectResponse(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await UseStructuredJsonResponse(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task UseJsonObjectResponse(AzureAIConfig config)
    {
        Console.WriteLine("=== Using JSON Object Response ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Create a chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(TutorInstructions);

        // Test inputs to evaluate
        string[] testInputs = [
            "The quick brown fox jumps over the lazy dog.",
            "In a world where gravity flows upward, the tears of joy fall toward heaven, creating rainbow bridges that connect the hearts of lonely stars.",
            "I like pizza."
        ];

        foreach (var input in testInputs)
        {
            Console.WriteLine($"[User]: {input}");
            chatHistory.AddUserMessage(input);

            // Get response with JSON formatting
            var executionSettings = new AzureOpenAIPromptExecutionSettings
            {
                ResponseFormat = "json_object"
            };

            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                kernel);

            Console.WriteLine($"[Assistant]: {response.Content}");
            
            // Try to parse the JSON response
            try
            {
                var scoreData = JsonSerializer.Deserialize<CreativityScore>(response.Content ?? "");
                Console.WriteLine($"Parsed Score: {scoreData?.Score}, Notes: {scoreData?.Notes}");
            }
            catch (JsonException)
            {
                Console.WriteLine("Failed to parse JSON response");
            }
            
            Console.WriteLine();
            chatHistory.Clear();
            chatHistory.AddSystemMessage(TutorInstructions);
        }
    }

    private static async Task UseStructuredJsonResponse(AzureAIConfig config)
    {
        Console.WriteLine("=== Using Structured JSON Response with Schema ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Create a chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            You are a creative writing evaluator. Rate the creativity and expressiveness of text samples.
            Always respond with a JSON object containing a score (1-100) and detailed notes explaining your reasoning.
            """);

        string creativeInput = "The clockwork butterfly emerged from its chrysalis of gears and springs, spreading wings that chimed like silver bells in the morning light of a steampunk dawn.";
        
        Console.WriteLine($"[User]: {creativeInput}");
        chatHistory.AddUserMessage($"Rate this text for creativity and expressiveness: {creativeInput}");

        // Create a structured response format
        var executionSettings = new AzureOpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object"
        };

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel);

        Console.WriteLine($"[Assistant]: {response.Content}");
        
        // Parse and display the structured response
        try
        {
            using JsonDocument doc = JsonDocument.Parse(response.Content ?? "");
            JsonElement root = doc.RootElement;
            
            if (root.TryGetProperty("score", out JsonElement scoreElement) &&
                root.TryGetProperty("notes", out JsonElement notesElement))
            {
                Console.WriteLine($"\nStructured Response:");
                Console.WriteLine($"  Score: {scoreElement.GetInt32()}/100");
                Console.WriteLine($"  Notes: {notesElement.GetString()}");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse structured JSON: {ex.Message}");
        }
        
        Console.WriteLine();
    }
}