// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating declarative AI approaches with Azure OpenAI.
/// Shows how to create declarative prompts and workflows.
/// </summary>
public class Step08_AzureAIAgent_Declarative
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
    /// Run the sample demonstrating declarative AI patterns.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 08: Declarative AI Patterns ===");
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
            await UseDeclarativePrompts(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await UseTemplateWorkflow(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task UseDeclarativePrompts(AzureAIConfig config)
    {
        Console.WriteLine("=== Declarative Prompt Templates ===");
        
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();

        // Create declarative prompt templates
        var emailTemplate = """
            You are a professional email assistant. 

            Write a {{$tone}} email with the following details:
            - To: {{$recipient}}
            - Subject: {{$subject}}
            - Main message: {{$message}}

            Make it professional, clear, and appropriate for business communication.
            """;

        var summaryTemplate = """
            You are a content summarizer.

            Create a {{$length}} summary of the following content:
            {{$content}}

            Focus on the key points and main takeaways.
            """;

        // Test email generation
        var emailFunction = kernel.CreateFunctionFromPrompt(emailTemplate);
        var emailArgs = new KernelArguments
        {
            ["tone"] = "formal",
            ["recipient"] = "John Smith",
            ["subject"] = "Project Update",
            ["message"] = "The quarterly project review has been completed and we're on track to meet our deadlines."
        };

        Console.WriteLine("[User]: Generate a formal email about project update");
        var emailResult = await kernel.InvokeAsync(emailFunction, emailArgs);
        Console.WriteLine($"[Assistant]: {emailResult}");
        Console.WriteLine();

        // Test content summarization
        var summaryFunction = kernel.CreateFunctionFromPrompt(summaryTemplate);
        var summaryArgs = new KernelArguments
        {
            ["length"] = "brief",
            ["content"] = "Cloud computing represents a paradigm shift in how organizations access and manage their IT resources. Instead of maintaining physical servers and infrastructure on-premises, companies can leverage cloud services provided by major platforms like Amazon Web Services, Microsoft Azure, and Google Cloud Platform. This approach offers numerous benefits including cost savings through pay-as-you-use models, enhanced scalability to handle varying workloads, improved accessibility allowing remote work capabilities, and robust security features managed by cloud providers. However, organizations must also consider challenges such as potential vendor lock-in, data privacy concerns, and the need for reliable internet connectivity."
        };

        Console.WriteLine("[User]: Create a brief summary of cloud computing content");
        var summaryResult = await kernel.InvokeAsync(summaryFunction, summaryArgs);
        Console.WriteLine($"[Assistant]: {summaryResult}");
        Console.WriteLine();
    }

    private static async Task UseTemplateWorkflow(AzureAIConfig config)
    {
        Console.WriteLine("=== Template-Based Workflow ===");
        
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());
        
        var kernel = builder.Build();

        // Create a multi-step declarative workflow
        var analyzeTemplate = """
            Analyze the following text and identify:
            1. Main topic
            2. Key themes
            3. Sentiment (positive/negative/neutral)
            4. Target audience

            Text: {{$input}}
            """;

        var improveTemplate = """
            Based on this analysis:
            {{$analysis}}

            Suggest 3 specific improvements for the original text:
            {{$original}}
            """;

        var originalText = "Our new product is okay and might be useful for some people. It has features that work fine and the price is reasonable. Maybe you should consider buying it if you need something like this.";

        // Step 1: Analyze
        var analyzeFunction = kernel.CreateFunctionFromPrompt(analyzeTemplate);
        var analyzeArgs = new KernelArguments { ["input"] = originalText };

        Console.WriteLine("[User]: Analyze this product description");
        var analysisResult = await kernel.InvokeAsync(analyzeFunction, analyzeArgs);
        Console.WriteLine($"[Assistant - Analysis]: {analysisResult}");
        Console.WriteLine();

        // Step 2: Improve based on analysis
        var improveFunction = kernel.CreateFunctionFromPrompt(improveTemplate);
        var improveArgs = new KernelArguments 
        { 
            ["analysis"] = analysisResult.ToString(),
            ["original"] = originalText 
        };

        Console.WriteLine("[User]: Suggest improvements based on the analysis");
        var improvementResult = await kernel.InvokeAsync(improveFunction, improveArgs);
        Console.WriteLine($"[Assistant - Improvements]: {improvementResult}");
        Console.WriteLine();
    }
}