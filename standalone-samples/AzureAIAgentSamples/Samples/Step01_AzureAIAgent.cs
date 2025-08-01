// Copyright (c) Microsoft. All rights reserved.
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using AzureAIAgentSamples.Utilities;

namespace AzureAIAgentSamples.Samples;

/// <summary>
/// Standalone example demonstrating basic Azure OpenAI chat completion with Semantic Kernel.
/// This sample shows how to create a simple story generation function using prompt templates.
/// </summary>
public class Step01_AzureAIAgent
{
    /// <summary>
    /// Configuration for Azure AI services.
    /// </summary>
    private class AzureAIConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Run the sample demonstrating basic Azure OpenAI chat completion functionality.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Step 01: Azure OpenAI Chat Completion with Story Generation ===");
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

        // Create Semantic Kernel with Azure OpenAI
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                config.ChatModelId,
                config.Endpoint,
                new AzureCliCredential());

        var kernel = builder.Build();

        // Load the prompt template from the YAML file
        string generateStoryYaml = ResourceHelper.Read("GenerateStory.yaml");
        
        // Parse the YAML to extract the template
        var lines = generateStoryYaml.Split('\n');
        var templateStart = false;
        var template = "";
        
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("template:"))
            {
                templateStart = true;
                // Check if template is on the same line
                var parts = line.Split(':', 2);
                if (parts.Length > 1 && parts[1].Trim().StartsWith("|"))
                {
                    continue; // Multi-line template starting
                }
                else if (parts.Length > 1)
                {
                    template = parts[1].Trim();
                    break;
                }
                continue;
            }
            if (templateStart)
            {
                if (line.StartsWith("  "))
                {
                    // Remove the indentation (2 spaces)
                    template += line.Substring(2) + "\n";
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    // End of template block
                    break;
                }
            }
        }

        template = template.Trim();
        Console.WriteLine($"Using template: {template}");
        Console.WriteLine();

        // Create a kernel function from the template
        var storyFunction = kernel.CreateFunctionFromPrompt(template);

        try
        {
            // Invoke the function with default arguments
            Console.WriteLine("Generating story with default arguments (topic: Dog, length: 3):");
            await InvokeStoryFunction("Dog", "3");

            Console.WriteLine();
            Console.WriteLine("Generating story with override arguments (topic: Cat, length: 5):");
            // Invoke the function with override arguments
            await InvokeStoryFunction("Cat", "5");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Local function to invoke the story generation function
        async Task InvokeStoryFunction(string topic, string length)
        {
            var arguments = new KernelArguments
            {
                ["topic"] = topic,
                ["length"] = length
            };

            var result = await kernel.InvokeAsync(storyFunction, arguments);
            Console.WriteLine($"Generated Story: {result}");
        }
    }

    /// <summary>
    /// Utility method for formatting agent chat messages.
    /// </summary>
    private static void WriteAgentChatMessage(ChatMessageContent message)
    {
        Console.WriteLine($"[{message.Role}]: {message.Content}");
        if (message.Metadata is { Count: > 0 })
        {
            Console.WriteLine($"[metadata]: {string.Join(", ", message.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        }
    }
}