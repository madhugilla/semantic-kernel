// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating file search and document analysis with Azure OpenAI.
/// Shows how to work with document content using Semantic Kernel.
/// </summary>
public class Step05_AzureAIAgent_FileSearch
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
    /// Run the sample demonstrating file search and document analysis.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 05: File Search and Document Analysis ===");
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
            await AnalyzeTextDocument(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await SearchInDocument(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task AnalyzeTextDocument(AzureAIConfig config)
    {
        Console.WriteLine("=== Document Analysis ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Check if we have a text document
        string documentPath = "Resources/Hamlet_full_play_summary.txt";
        if (!File.Exists(documentPath))
        {
            Console.WriteLine($"Document not found at {documentPath}. Using sample text instead.");
            await AnalyzeSampleText(chatService, kernel);
            return;
        }

        // Read the document
        string documentContent = await File.ReadAllTextAsync(documentPath);
        
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a literary analysis expert. Analyze documents and provide insights about their content, themes, and structure.");

        Console.WriteLine("[User]: Analyzing document for key themes and characters...");
        chatHistory.AddUserMessage($"Analyze this document and identify the main themes, key characters, and provide a brief summary:\n\n{documentContent}");

        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }

    private static async Task AnalyzeSampleText(IChatCompletionService chatService, Kernel kernel)
    {
        string sampleText = """
            The Great Gatsby is a 1925 novel by American writer F. Scott Fitzgerald. Set in the Jazz Age on prosperous Long Island and in New York City, the novel tells the first-person story of Nick Carraway, a young Midwesterner who moves to Long Island in 1922, intending to work in the bond business. There he befriends his mysterious neighbor, Jay Gatsby. The novel was inspired by a youthful romance Fitzgerald had with socialite Ginevra King, and the riotous parties he attended on Long Island's North Shore in 1922. Following a move to the French Riviera, Fitzgerald completed a rough draft in 1924. He submitted it to editor Maxwell Perkins, who persuaded Fitzgerald to revise the work over the following winter. After making revisions, Fitzgerald was satisfied with the text, but remained ambivalent about the book's title and considered several alternatives.
            """;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a literary analysis expert. Analyze text and provide insights about themes, characters, and literary significance.");

        Console.WriteLine("[User]: Analyzing sample text about The Great Gatsby...");
        chatHistory.AddUserMessage($"Analyze this text about The Great Gatsby and discuss its historical context and literary significance:\n\n{sampleText}");

        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }

    private static async Task SearchInDocument(AzureAIConfig config)
    {
        Console.WriteLine("=== Document Search ===");
        
        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Use JSON data file for search example
        string jsonPath = "Resources/countries.json";
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"JSON document not found at {jsonPath}. Skipping search example.");
            return;
        }

        string jsonContent = await File.ReadAllTextAsync(jsonPath);
        
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a data search assistant. Search through provided data and answer questions about it.");

        string searchQuery = "Find countries in Europe with population over 50 million";
        Console.WriteLine($"[User]: {searchQuery}");
        chatHistory.AddUserMessage($"Search this data and answer: {searchQuery}\n\nData:\n{jsonContent}");

        var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }
}