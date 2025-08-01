// Copyright (c) Microsoft. All rights reserved.

using AzureAIAgentPersistentSamples.Samples;

namespace AzureAIAgentPersistentSamples;

/// <summary>
/// Console application demonstrating Azure AI Agents using Azure.AI.Agents.Persistent.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Azure AI Agent Samples using Azure.AI.Agents.Persistent");
        Console.WriteLine("=========================================================");
        Console.WriteLine();

        try
        {
            if (args.Length > 0 && int.TryParse(args[0], out int stepNumber))
            {
                await RunSpecificSample(stepNumber);
            }
            else
            {
                await ShowMenuAndRunSample();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            Console.WriteLine("\n📝 Please check your configuration in appsettings.json and ensure:");
            Console.WriteLine("   - Azure AI endpoint is correctly set");
            Console.WriteLine("   - Azure CLI is authenticated (run 'az login')");
            Console.WriteLine("   - You have proper access to the Azure AI resource");
        }
    }

    private static async Task ShowMenuAndRunSample()
    {
        Console.WriteLine("Available samples using Azure.AI.Agents.Persistent:");
        Console.WriteLine("1. Step 01 - Azure AI Agent with Story Generation");
        Console.WriteLine("2. Step 02 - Azure AI Agent with Plugins");
        Console.WriteLine("3. Step 03 - Azure AI Agent with Vision");
        Console.WriteLine("4. Step 04 - Azure AI Agent Code Analysis");
        Console.WriteLine("5. Step 05 - Azure AI Agent File Search");
        Console.WriteLine("6. Step 06 - Azure AI Agent OpenAPI Integration");
        Console.WriteLine("7. Step 07 - Azure AI Agent Custom Functions");
        Console.WriteLine("8. Step 08 - Azure AI Agent Declarative Patterns");
        Console.WriteLine("9. Step 09 - Azure AI Agent Information Grounding");
        Console.WriteLine("10. Step 10 - Azure AI Agent Structured Output");
        Console.WriteLine();
        Console.Write("Select a sample (1-10) or press Enter to exit: ");

        var input = Console.ReadLine();
        if (int.TryParse(input, out int choice))
        {
            await RunSpecificSample(choice);
        }
    }

    private static async Task RunSpecificSample(int stepNumber)
    {
        try
        {
            switch (stepNumber)
            {
                case 1:
                    await new Step01_AzureAIAgent().RunAsync();
                    break;
                case 2:
                    await new Step02_AzureAIAgent_Plugins().RunAsync();
                    break;
                // TODO: Implement other samples
                // case 3:
                //     await new Step03_AzureAIAgent_Vision().RunAsync();
                //     break;
                default:
                    Console.WriteLine($"❌ Sample {stepNumber} is not yet implemented.");
                    Console.WriteLine("Currently available: Samples 1-2");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error running sample {stepNumber}: {ex.Message}");
            throw;
        }
    }
}