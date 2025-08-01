// Copyright (c) Microsoft. All rights reserved.

using AzureAIAgentPersistentSamples.Utilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace AzureAIAgentPersistentSamples.Samples;

/// <summary>
/// Demonstrates basic Azure AI Agent functionality with story generation using Azure.AI.Agents.Persistent.
/// </summary>
public class Step01_AzureAIAgent : BaseAzureAIAgentSample
{
    /// <summary>
    /// Shows how to create an Azure AI Agent and generate a story using prompt templates.
    /// </summary>
    public override async Task RunAsync()
    {
        Console.WriteLine("=== Step 01 - Azure AI Agent with Story Generation ===");
        Console.WriteLine("This sample demonstrates creating an Azure AI Agent using Azure.AI.Agents.Persistent");
        Console.WriteLine("and generating a story with prompt templates and variable substitution.");
        Console.WriteLine();

        // Read the story generation template
        string templatePath = Path.Combine("Resources", "GenerateStory.yaml");
        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"Template file not found: {templatePath}");
            Console.WriteLine("Creating a basic story generation template...");
            await CreateStoryTemplateAsync(templatePath);
        }

        string generateStoryYaml = await File.ReadAllTextAsync(templatePath);
        PromptTemplateConfig templateConfig = KernelFunctionYaml.ToPromptTemplateConfig(generateStoryYaml);

        // Create the Azure AI Agent with the template
        AzureAIAgent agent = await CreateAzureAIAgentAsync(
            instructions: templateConfig.Template,
            name: templateConfig.Name ?? "StoryGenerator",
            description: templateConfig.Description ?? "An agent that generates creative stories");

        // Create a thread for the conversation
        AgentThread thread = new AzureAIAgentThread(Client, metadata: SampleMetadata);

        try
        {
            Console.WriteLine("🐕 Generating a story about a Dog (3 paragraphs)...");
            
            var dogArguments = new KernelArguments()
            {
                { "topic", "Dog" },
                { "length", "3" }
            };
            
            await InvokeAgentAsync(agent, thread, dogArguments);

            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine("🐱 Now generating a story about a Cat (2 paragraphs)...");
            
            var catArguments = new KernelArguments()
            {
                { "topic", "Cat" },
                { "length", "2" }
            };

            await InvokeAgentAsync(agent, thread, catArguments);
        }
        finally
        {
            // Clean up resources
            await thread.DeleteAsync();
            await Client.Administration.DeleteAgentAsync(agent.Id);
            Console.WriteLine("\n✅ Sample completed successfully. Agent and thread have been cleaned up.");
        }
    }

    private static async Task CreateStoryTemplateAsync(string templatePath)
    {
        string template = """
        name: GenerateStory
        template_format: semantic-kernel
        description: Generate a creative story on any topic
        template: |
          You are a creative storyteller. Generate an engaging {{$length}} paragraph story about {{$topic}}.
          
          Requirements:
          - Make the story interesting and engaging
          - Include vivid descriptions and character development
          - Ensure the story has a clear beginning, middle, and end
          - Keep each paragraph substantial and meaningful
          - Make it suitable for all audiences
          
          Topic: {{$topic}}
          Length: {{$length}} paragraphs
          
          Please create the story now:
        """;

        await File.WriteAllTextAsync(templatePath, template);
        Console.WriteLine($"✅ Created story template at: {templatePath}");
    }
}