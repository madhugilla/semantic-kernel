// Copyright (c) Microsoft. All rights reserved.
using AzureAIAgentSamples.Samples;

namespace AzureAIAgentSamples;

/// <summary>
/// Console application for running Azure OpenAI samples with Semantic Kernel.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Azure OpenAI Samples with Semantic Kernel");
        Console.WriteLine("==========================================");
        Console.WriteLine();

        if (args.Length > 0 && int.TryParse(args[0], out int stepNumber))
        {
            await RunSpecificSample(stepNumber);
        }
        else
        {
            await ShowMenuAndRunSample();
        }
    }

    private static async Task ShowMenuAndRunSample()
    {
        Console.WriteLine("Available samples:");
        Console.WriteLine("1. Step 01 - Azure OpenAI Chat Completion with Story Generation");
        Console.WriteLine("2. Step 02 - Azure OpenAI with Plugins");
        Console.WriteLine("3. Step 03 - Azure OpenAI with Vision");
        Console.WriteLine("4. Step 04 - Azure OpenAI Code Analysis and Generation");
        Console.WriteLine("5. Step 05 - Azure OpenAI File Search and Document Analysis");
        Console.WriteLine("6. Step 06 - Azure OpenAI OpenAPI Integration");
        Console.WriteLine("7. Step 07 - Azure OpenAI Custom Functions");
        Console.WriteLine("8. Step 08 - Azure OpenAI Declarative Patterns");
        Console.WriteLine("9. Step 09 - Azure OpenAI Search Grounding Concepts");
        Console.WriteLine("10. Step 10 - Azure OpenAI JSON Response Formatting");
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
                    await Step01_AzureAIAgent.RunAsync();
                    break;
                case 2:
                    await Step02_AzureAIAgent_Plugins.RunAsync();
                    break;
                case 3:
                    await Step03_AzureAIAgent_Vision.RunAsync();
                    break;
                case 4:
                    await Step04_AzureAIAgent_CodeInterpreter.RunAsync();
                    break;
                case 5:
                    await Step05_AzureAIAgent_FileSearch.RunAsync();
                    break;
                case 6:
                    await Step06_AzureAIAgent_OpenAPI.RunAsync();
                    break;
                case 7:
                    await Step07_AzureAIAgent_Functions.RunAsync();
                    break;
                case 8:
                    await Step08_AzureAIAgent_Declarative.RunAsync();
                    break;
                case 9:
                    await Step09_AzureAIAgent_BingGrounding.RunAsync();
                    break;
                case 10:
                    await Step10_JsonResponse.RunAsync();
                    break;
                default:
                    Console.WriteLine("Invalid sample number. Please choose 1-10.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running sample: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}