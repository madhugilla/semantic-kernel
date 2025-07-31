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

Currently available:
- **Step 01** - Azure OpenAI Chat Completion with Story Generation

Coming soon:
- **Step 02** - Azure OpenAI with Plugins  
- **Step 03** - Azure OpenAI with Vision
- **Step 04** - Azure OpenAI with Code Interpreter
- **Step 05** - Azure OpenAI with File Search
- **Step 06** - Azure OpenAI with OpenAPI
- **Step 07** - Azure OpenAI with Functions
- **Step 08** - Azure OpenAI Declarative
- **Step 09** - Azure OpenAI with Bing Grounding
- **Step 10** - JSON Response

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
├── Samples/                      # Sample implementations
│   └── Step01_AzureAIAgent.cs    # Basic Azure OpenAI chat completion sample
├── Utilities/                    # Helper utilities
│   └── ResourceHelper.cs         # Resource file reader
└── Resources/                    # Template and resource files
    ├── GenerateStory.yaml        # Story generation prompt template
    ├── cat.jpg                   # Sample image for vision samples
    ├── countries.json            # Sample data file
    ├── employees.pdf             # Sample PDF for file search
    └── weather.json              # Sample weather data
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