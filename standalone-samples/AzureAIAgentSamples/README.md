# Azure OpenAI Samples - Standalone Console Application

This is a standalone console application that demonstrates how to use Azure OpenAI with the Semantic Kernel. These samples are self-contained and can be run independently without requiring the full Semantic Kernel repository.

## Prerequisites

1. **.NET 8.0 SDK** - Download from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

2. **Azure OpenAI Services** - You'll need access to Azure OpenAI with a deployed chat model (e.g., GPT-4).

3. **Azure CLI** (recommended) - For authentication. Install from [https://docs.microsoft.com/cli/azure/install-azure-cli](https://docs.microsoft.com/cli/azure/install-azure-cli)

## Setup

### 1. Configure Azure OpenAI Services

Edit the `appsettings.json` file and provide your Azure OpenAI configuration:

```json
{
  "AzureAI": {
    "Endpoint": "https://your-azure-ai-endpoint.openai.azure.com/",
    "ChatModelId": "gpt-4o"
  }
}
```

### 2. Authentication

This project uses Azure CLI authentication by default. Make sure you're logged in:

```bash
az login
```

Alternatively, you can modify the samples to use other authentication methods like managed identity, service principal, or API keys.

## Running the Samples

### Option 1: Interactive Menu

Run the application without arguments to see an interactive menu:

```bash
dotnet run
```

### Option 2: Direct Sample Execution

Run a specific sample by passing the step number as an argument:

```bash
dotnet run 1  # Runs Step 01 - Azure AI Agent Basic Usage
```

## Available Samples

All samples are now implemented and available:

### 1. Basic Chat Completion with Story Generation
Demonstrates basic Azure OpenAI chat completion using prompt templates from YAML files. Shows how to generate creative stories with customizable parameters.

### 2. Plugins and Function Calling
Shows how to create and use plugins with Semantic Kernel. Includes examples of:
- Menu plugin for restaurant scenarios
- Widget factory with enum parameters
- Custom prompt functions for text analysis

### 3. Vision Capabilities
Demonstrates Azure OpenAI's vision capabilities including:
- Analyzing images from URLs
- Processing local image files
- Describing visual content

### 4. Code Analysis and Generation
Shows code-related AI capabilities:
- Code analysis and bug detection
- Performance optimization suggestions
- Code generation from requirements

### 5. File Search and Document Analysis
Demonstrates document processing capabilities:
- Text document analysis
- Key theme extraction
- Structured data search

### 6. OpenAPI Integration
Shows API-related functionality:
- OpenAPI specification generation
- API endpoint analysis
- Documentation generation

### 7. Custom Functions
Demonstrates creating and using custom kernel functions:
- Mathematical operations (square root, factorial, prime checking)
- Custom prompt functions
- Function chaining

### 8. Declarative AI Patterns
Shows declarative approaches to AI:
- Template-based workflows
- Multi-step processing
- Reusable prompt patterns

### 9. Search Grounding Concepts
Demonstrates information grounding and fact-checking:
- Source verification recommendations
- Time-sensitive information handling
- Bias detection

### 10. JSON Response Formatting
Shows structured output generation:
- JSON object responses
- Schema validation
- Structured data parsing

## Sample Details

### Step 01 - Azure OpenAI Chat Completion with Story Generation

This sample demonstrates:
- How to configure Azure OpenAI with Semantic Kernel
- Loading and parsing YAML prompt templates
- Using prompt templates with variable substitution
- Basic chat completion functionality with different arguments
- Proper error handling

The sample uses a story generation template that accepts a topic and length parameter, showing how to pass arguments to functions and handle responses.

## Project Structure

```
AzureAIAgentSamples/
├── Program.cs                     # Main console application entry point
├── appsettings.json              # Configuration file
├── AzureAIAgentSamples.csproj    # Project file with dependencies
├── Samples/                      # All sample implementations
│   ├── Step01_AzureAIAgent.cs
│   ├── Step02_AzureAIAgent_Plugins.cs
│   ├── Step03_AzureAIAgent_Vision.cs
│   ├── Step04_AzureAIAgent_CodeInterpreter.cs
│   ├── Step05_AzureAIAgent_FileSearch.cs
│   ├── Step06_AzureAIAgent_OpenAPI.cs
│   ├── Step07_AzureAIAgent_Functions.cs
│   ├── Step08_AzureAIAgent_Declarative.cs
│   ├── Step09_AzureAIAgent_BingGrounding.cs
│   └── Step10_JsonResponse.cs
├── Plugins/                      # Reusable plugin implementations
│   ├── MenuPlugin.cs
│   └── WidgetFactory.cs
├── Utilities/                    # Helper utilities
│   └── ResourceHelper.cs         # Resource file reader
└── Resources/                    # Template and resource files
    ├── AutoInvokeTools.yaml
    ├── GenerateStory.yaml
    ├── Hamlet_full_play_summary.txt
    ├── cat.jpg
    ├── countries.json
    ├── employees.pdf
    └── weather.json
```

## Dependencies

This project uses the following NuGet packages:
- `Microsoft.SemanticKernel` - Core Semantic Kernel functionality including Azure OpenAI support
- `Azure.Identity` - Azure authentication
- `Microsoft.Extensions.Configuration.*` - Configuration management

## Troubleshooting

### Authentication Issues
- Ensure you're logged in with `az login`
- Check that your account has access to the Azure OpenAI resource
- Verify the endpoint URL in your configuration

### Configuration Issues
- Double-check your `appsettings.json` configuration
- Ensure the model ID matches a deployed model in your Azure OpenAI resource
- Verify the endpoint URL format (should end with `.openai.azure.com/`)

### Dependency Issues
- Make sure you have .NET 8.0 SDK installed
- Run `dotnet restore` to ensure all packages are installed

## Extending the Samples

To add your own samples:

1. Create a new class in the `Samples/` directory
2. Implement a static `RunAsync()` method
3. Add the sample to the menu in `Program.cs`
4. Add any required resources to the `Resources/` directory

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues and questions:
- Check the [Semantic Kernel documentation](https://learn.microsoft.com/semantic-kernel/)
- Visit the [Semantic Kernel GitHub repository](https://github.com/microsoft/semantic-kernel)
- Review Azure OpenAI Services documentation for authentication and setup guidance