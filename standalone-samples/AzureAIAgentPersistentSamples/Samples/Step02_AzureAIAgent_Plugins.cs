// Copyright (c) Microsoft. All rights reserved.

using AzureAIAgentPersistentSamples.Plugins;
using AzureAIAgentPersistentSamples.Utilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace AzureAIAgentPersistentSamples.Samples;

/// <summary>
/// Demonstrates Azure AI Agent with plugin functionality using Azure.AI.Agents.Persistent.
/// </summary>
public class Step02_AzureAIAgent_Plugins : BaseAzureAIAgentSample
{
    /// <summary>
    /// Shows how to create an Azure AI Agent with plugins for menu operations and widget creation.
    /// </summary>
    public override async Task RunAsync()
    {
        Console.WriteLine("=== Step 02 - Azure AI Agent with Plugins ===");
        Console.WriteLine("This sample demonstrates using Azure AI Agents with plugins for:");
        Console.WriteLine("- Restaurant menu operations (MenuPlugin)");
        Console.WriteLine("- Widget factory operations (WidgetFactory)");
        Console.WriteLine();

        await RunMenuPluginSampleAsync();
        
        Console.WriteLine("\n" + new string('=', 80));
        
        await RunWidgetFactorySampleAsync();
    }

    private async Task RunMenuPluginSampleAsync()
    {
        Console.WriteLine("🍽️ Menu Plugin Sample");
        Console.WriteLine("Creating an agent that can answer questions about restaurant menu...");

        // Create a kernel with the menu plugin
        var kernel = new Kernel();
        kernel.Plugins.Add(KernelPluginFactory.CreateFromType<MenuPlugin>());

        // Create the Azure AI Agent with plugin support
        AzureAIAgent agent = await CreateAzureAIAgentAsync(
            instructions: "You are a helpful restaurant host. Answer questions about the menu using the available menu functions. Be friendly and provide detailed information about menu items, prices, and recommendations.",
            name: "RestaurantHost",
            description: "A helpful restaurant host that can answer menu questions",
            kernel: kernel);

        // Create a thread for the conversation
        AgentThread thread = new AzureAIAgentThread(Client, metadata: SampleMetadata);

        try
        {
            Console.WriteLine("\n🤖 Agent: Welcome! I can help you with our menu. What would you like to know?");

            // Ask about the special soup
            await InvokeAgentAsync(agent, thread, "What is the special soup and its price?");

            // Ask about drinks
            await InvokeAgentAsync(agent, thread, "What drinks do you have available?");

            // Ask for recommendations
            await InvokeAgentAsync(agent, thread, "Can you recommend something for someone who likes seafood?");
        }
        finally
        {
            // Clean up resources
            await thread.DeleteAsync();
            await Client.Administration.DeleteAgentAsync(agent.Id);
            Console.WriteLine("\n✅ Menu plugin sample completed. Agent and thread cleaned up.");
        }
    }

    private async Task RunWidgetFactorySampleAsync()
    {
        Console.WriteLine("🏭 Widget Factory Sample");
        Console.WriteLine("Creating an agent that can create custom widgets...");

        // Create a kernel with the widget factory plugin
        var kernel = new Kernel();
        kernel.Plugins.Add(KernelPluginFactory.CreateFromType<WidgetFactory>());

        // Create the Azure AI Agent with widget factory support
        AzureAIAgent agent = await CreateAzureAIAgentAsync(
            instructions: "You are a helpful widget factory assistant. You can create custom widgets based on customer specifications using the available widget creation functions. Always confirm the widget specifications before creating them.",
            name: "WidgetFactoryAssistant",
            description: "An assistant that helps create custom widgets",
            kernel: kernel);

        // Create a thread for the conversation
        AgentThread thread = new AzureAIAgentThread(Client, metadata: SampleMetadata);

        try
        {
            Console.WriteLine("\n🤖 Agent: Welcome to the Widget Factory! I can help you create custom widgets.");

            // Create a red widget
            await InvokeAgentAsync(agent, thread, "I'd like to create a beautiful red widget, please.");

            // Create a blue widget
            await InvokeAgentAsync(agent, thread, "Now I need a blue widget that's suitable for outdoor use.");
        }
        finally
        {
            // Clean up resources
            await thread.DeleteAsync();
            await Client.Administration.DeleteAgentAsync(agent.Id);
            Console.WriteLine("\n✅ Widget factory sample completed. Agent and thread cleaned up.");
        }
    }
}