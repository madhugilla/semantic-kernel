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
        Console.WriteLine("2. Step 02 - Azure OpenAI with Plugins (Coming Soon)");
        Console.WriteLine("3. Step 03 - Azure OpenAI with Vision (Coming Soon)");
        Console.WriteLine("4. Step 04 - Azure OpenAI with Code Interpreter (Coming Soon)");
        Console.WriteLine("5. Step 05 - Azure OpenAI with File Search (Coming Soon)");
        Console.WriteLine("6. Step 06 - Azure OpenAI with OpenAPI (Coming Soon)");
        Console.WriteLine("7. Step 07 - Azure OpenAI with Functions (Coming Soon)");
        Console.WriteLine("8. Step 08 - Azure OpenAI Declarative (Coming Soon)");
        Console.WriteLine("9. Step 09 - Azure OpenAI with Bing Grounding (Coming Soon)");
        Console.WriteLine("10. Step 10 - JSON Response (Coming Soon)");
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
                    Console.WriteLine("Step 02 sample will be available soon.");
                    break;
                case 3:
                    Console.WriteLine("Step 03 sample will be available soon.");
                    break;
                case 4:
                    Console.WriteLine("Step 04 sample will be available soon.");
                    break;
                case 5:
                    Console.WriteLine("Step 05 sample will be available soon.");
                    break;
                case 6:
                    Console.WriteLine("Step 06 sample will be available soon.");
                    break;
                case 7:
                    Console.WriteLine("Step 07 sample will be available soon.");
                    break;
                case 8:
                    Console.WriteLine("Step 08 sample will be available soon.");
                    break;
                case 9:
                    Console.WriteLine("Step 09 sample will be available soon.");
                    break;
                case 10:
                    Console.WriteLine("Step 10 sample will be available soon.");
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