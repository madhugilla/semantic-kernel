// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating Azure OpenAI vision capabilities with Semantic Kernel.
/// Shows how to analyze images using Azure OpenAI's vision models.
/// </summary>
public class Step03_AzureAIAgent_Vision
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
    /// Run the sample demonstrating Azure OpenAI vision capabilities.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 03: Azure OpenAI Vision Capabilities ===");
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
            await AnalyzeImageFromUrl(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await AnalyzeLocalImage(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task AnalyzeImageFromUrl(AzureAIConfig config)
    {
        Console.WriteLine("=== Analyzing Image from URL ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Create a chat history with vision content
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a helpful AI assistant that can analyze images. Describe what you see in detail.");

        // Add user message with image URL
        var imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/47/New_york_times_square-terabass.jpg/800px-New_york_times_square-terabass.jpg";
        var userMessage = new ChatMessageContent(AuthorRole.User, 
            new ChatMessageContentItemCollection
            {
                new TextContent("Describe this image in detail."),
                new ImageContent(new Uri(imageUrl))
            });

        chatHistory.Add(userMessage);
        Console.WriteLine("[User]: Analyzing image from URL: " + imageUrl);

        // Get response
        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }

    private static async Task AnalyzeLocalImage(AzureAIConfig config)
    {
        Console.WriteLine("=== Analyzing Local Image ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Check if we have a local image
        string imagePath = "Resources/cat.jpg";
        if (!File.Exists(imagePath))
        {
            Console.WriteLine($"Local image not found at {imagePath}. Skipping local image analysis.");
            return;
        }

        // Create a chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a helpful AI assistant that can analyze images. Look for animals and describe them.");

        // Read image and convert to base64
        byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
        string base64Image = Convert.ToBase64String(imageBytes);
        string dataUri = $"data:image/jpeg;base64,{base64Image}";

        var userMessage = new ChatMessageContent(AuthorRole.User, 
            new ChatMessageContentItemCollection
            {
                new TextContent("Is there an animal in this image? If so, describe it."),
                new ImageContent(dataUri)
            });

        chatHistory.Add(userMessage);
        Console.WriteLine("[User]: Analyzing local image for animals...");

        // Get response
        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }
}