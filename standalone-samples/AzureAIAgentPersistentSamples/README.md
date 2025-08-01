# Azure AI Agent Samples using Azure.AI.Agents.Persistent

This standalone console application demonstrates comprehensive Azure AI Agent functionality using **Azure.AI.Agents.Persistent**, providing practical examples that showcase the power of persistent AI agents with Semantic Kernel.

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 or later
- Azure AI resource with OpenAI models
- Azure CLI installed and authenticated (`az login`)

### Configuration

1. Update `appsettings.json` with your Azure AI configuration:
```json
{
  "AzureAI": {
    "Endpoint": "https://your-azure-ai-endpoint.openai.azure.com/",
    "ChatModelId": "gpt-4o"
  }
}
```

2. Ensure you're authenticated with Azure CLI:
```bash
az login
```

### Running the Samples

**Interactive menu:**
```bash
dotnet run
```

**Run specific sample:**
```bash
dotnet run 1    # Run Step 01 - Basic Agent Story Generation
```

## 🎯 Key Features

- **Azure.AI.Agents.Persistent Integration**: Uses the persistent agents API for robust agent management
- **Standalone Project**: No dependencies on the full Semantic Kernel repository
- **Production Ready**: Proper error handling, configuration management, and resource cleanup
- **Educational**: Each sample demonstrates specific concepts and patterns
- **Extensible**: Easy to add new samples and modify existing ones

## 📋 Available Samples

### Step 01 - Azure AI Agent with Story Generation ✅
**Status: Implemented**

Demonstrates basic Azure AI Agent functionality with YAML prompt templates:
- Creating persistent agents with template-based instructions
- Using KernelArguments for dynamic template variables
- Proper agent and thread lifecycle management
- Template-based story generation with variable substitution

**Key concepts:**
- `PersistentAgentsClient` for agent management
- `AzureAIAgent` creation from agent definitions
- `AzureAIAgentThread` for conversation management
- YAML prompt template configuration
- Dynamic parameter passing with `KernelArguments`

### Additional Samples (Planned)

- **Step 02**: Azure AI Agent with Plugins and Function Calling
- **Step 03**: Azure AI Agent with Vision Capabilities
- **Step 04**: Azure AI Agent for Code Analysis and Generation
- **Step 05**: Azure AI Agent with File Search
- **Step 06**: Azure AI Agent with OpenAPI Integration
- **Step 07**: Azure AI Agent with Custom Functions
- **Step 08**: Azure AI Agent with Declarative Patterns
- **Step 09**: Azure AI Agent with Information Grounding
- **Step 10**: Azure AI Agent with Structured Output

## 🏗️ Architecture

### Project Structure
```
AzureAIAgentPersistentSamples/
├── Program.cs                  # Main console application with interactive menu
├── appsettings.json           # Configuration file
├── Utilities/
│   ├── AzureAIConfiguration.cs       # Configuration model
│   ├── ConfigurationManager.cs       # Configuration loading utilities
│   └── BaseAzureAIAgentSample.cs    # Base class for all samples
├── Samples/
│   ├── Step01_AzureAIAgent.cs       # Basic agent story generation
│   └── [Additional samples...]
├── Plugins/
│   └── [Reusable plugin implementations]
└── Resources/
    └── [Template files and sample data]
```

### Key Components

1. **BaseAzureAIAgentSample**: Provides common functionality for all samples
   - Configuration management
   - PersistentAgentsClient initialization
   - Agent creation helpers
   - Response formatting utilities

2. **ConfigurationManager**: Handles loading settings from appsettings.json
   - Validates required configuration
   - Provides strongly-typed configuration objects

3. **Sample Classes**: Individual examples demonstrating specific functionality
   - Inherit from BaseAzureAIAgentSample
   - Implement RunAsync() method
   - Include comprehensive documentation and error handling

## 🔧 Implementation Details

### Azure.AI.Agents.Persistent Usage

This project specifically uses the `Azure.AI.Agents.Persistent` package which provides:
- **PersistentAgentsClient**: Main client for agent operations
- **AzureAIAgent**: Persistent agent instances that maintain state
- **AzureAIAgentThread**: Conversation threads for agent interactions
- **Agent Lifecycle Management**: Proper creation, usage, and cleanup of agents

### Differences from Basic Semantic Kernel

Unlike basic chat completion with Semantic Kernel, this approach:
- Creates persistent agent definitions stored in Azure AI
- Maintains conversation state across interactions
- Supports complex agent orchestration scenarios
- Provides built-in agent lifecycle management
- Enables advanced features like tool calling and file handling

### Authentication

The samples use Azure CLI authentication (`AzureCliCredential`) which:
- Simplifies local development setup
- Uses your existing Azure login
- Supports role-based access control
- Works with Azure AI resource permissions

## 🛠️ Package Dependencies

- **Microsoft.SemanticKernel** (1.61.0): Core Semantic Kernel functionality
- **Microsoft.SemanticKernel.Yaml** (1.61.0): YAML template support
- **Microsoft.SemanticKernel.Agents.Core** (1.61.0): Core agent abstractions
- **Microsoft.SemanticKernel.Agents.AzureAI** (1.61.0-preview): Azure AI agent implementation
- **Azure.AI.Agents.Persistent** (1.0.0): Persistent agents API
- **Azure.Identity** (1.14.2): Azure authentication
- **Microsoft.Extensions.Configuration*** (9.0.1): Configuration management

## 🔍 Troubleshooting

### Common Issues

1. **Authentication Errors**
   - Ensure `az login` is completed
   - Verify you have access to the Azure AI resource
   - Check that your account has Cognitive Services Contributor role

2. **Configuration Issues**
   - Verify the endpoint URL in appsettings.json
   - Ensure the model ID exists in your Azure AI resource
   - Check that the configuration file is copied to output directory

3. **Build Errors**
   - Ensure .NET 8.0 SDK is installed
   - Run `dotnet restore` to ensure all packages are restored
   - Check for any package version conflicts

### Getting Help

If you encounter issues:
1. Check the console output for detailed error messages
2. Verify your Azure AI resource configuration
3. Ensure all prerequisites are installed and configured
4. Review the troubleshooting section above

## 📚 Learning Resources

- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Azure AI Services Documentation](https://learn.microsoft.com/en-us/azure/ai-services/)
- [Azure OpenAI Service Documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

## 🤝 Contributing

This is a standalone sample project. To extend it:
1. Add new sample classes in the `Samples/` directory
2. Inherit from `BaseAzureAIAgentSample`
3. Implement the `RunAsync()` method
4. Add the sample to the menu in `Program.cs`
5. Update this README with the new sample description

## 📄 License

This project follows the same license as the main Semantic Kernel repository.