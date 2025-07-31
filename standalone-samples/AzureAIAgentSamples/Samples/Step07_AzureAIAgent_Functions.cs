// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;
using System.ComponentModel;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating custom functions with Azure OpenAI.
/// Shows how to create and use custom kernel functions for specific tasks.
/// </summary>
public class Step07_AzureAIAgent_Functions
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
    /// Custom plugin for mathematical operations.
    /// </summary>
    public class MathPlugin
    {
        [KernelFunction, Description("Calculate the square root of a number")]
        public double SquareRoot([Description("The number to calculate square root for")] double number)
        {
            return Math.Sqrt(number);
        }

        [KernelFunction, Description("Calculate factorial of a number")]
        public long Factorial([Description("The number to calculate factorial for (must be non-negative)")] int number)
        {
            if (number < 0) throw new ArgumentException("Number must be non-negative");
            if (number == 0 || number == 1) return 1;
            
            long result = 1;
            for (int i = 2; i <= number; i++)
            {
                result *= i;
            }
            return result;
        }

        [KernelFunction, Description("Check if a number is prime")]
        public bool IsPrime([Description("The number to check for primality")] int number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            for (int i = 3; i * i <= number; i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Run the sample demonstrating custom functions.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 07: Custom Functions ===");
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
            await UseMathFunctions(config);
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            await UsePromptFunctions(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task UseMathFunctions(AzureAIConfig config)
    {
        Console.WriteLine("=== Using Custom Math Functions ===");
        
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());

        // Add custom math plugin
        builder.Plugins.Add(KernelPluginFactory.CreateFromType<MathPlugin>());
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a math tutor. Use the available math functions to solve problems and explain the results.");

        string[] mathQuestions = [
            "What is the square root of 144?",
            "Calculate the factorial of 5",
            "Is 17 a prime number?"
        ];

        foreach (var question in mathQuestions)
        {
            Console.WriteLine($"[User]: {question}");
            chatHistory.AddUserMessage(question);

            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                new AzureOpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
                kernel);

            Console.WriteLine($"[Assistant]: {response.Content}");
            chatHistory.AddAssistantMessage(response.Content ?? "");
            Console.WriteLine();
        }
    }

    private static async Task UsePromptFunctions(AzureAIConfig config)
    {
        Console.WriteLine("=== Using Custom Prompt Functions ===");
        
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());

        // Define custom prompt functions
        var summarizeFunction = KernelFunctionFactory.CreateFromPrompt(
            "Summarize the following text in 2-3 sentences:\n\n{{$input}}",
            description: "Summarizes text to 2-3 sentences");

        var translateFunction = KernelFunctionFactory.CreateFromPrompt(
            "Translate the following text to {{$language}}:\n\n{{$input}}",
            description: "Translates text to specified language");

        // Add functions as plugins
        builder.Plugins.Add(KernelPluginFactory.CreateFromFunctions("TextPlugin", [summarizeFunction, translateFunction]));
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a text processing assistant. Use the available text functions to help users with text tasks.");

        string longText = "Artificial Intelligence (AI) has become one of the most transformative technologies of the 21st century. It encompasses various techniques including machine learning, deep learning, and natural language processing. AI systems can now perform tasks that were once thought to be exclusively human, such as recognizing images, understanding speech, and making complex decisions. The applications of AI span across numerous industries including healthcare, finance, transportation, and entertainment.";

        Console.WriteLine("[User]: Summarize this text about AI");
        chatHistory.AddUserMessage($"Please summarize this text: {longText}");

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            new AzureOpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
            kernel);

        Console.WriteLine($"[Assistant]: {response.Content}");
        Console.WriteLine();
    }
}