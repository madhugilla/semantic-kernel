// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Configuration;

namespace AzureAIAgentPersistentSamples.Utilities;

/// <summary>
/// Configuration manager for loading settings from appsettings.json.
/// </summary>
public static class ConfigurationManager
{
    private static readonly IConfiguration s_configuration;

    static ConfigurationManager()
    {
        s_configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Gets the Azure AI configuration.
    /// </summary>
    public static AzureAIConfiguration GetAzureAIConfiguration()
    {
        var config = new AzureAIConfiguration();
        s_configuration.GetSection("AzureAI").Bind(config);

        if (string.IsNullOrEmpty(config.Endpoint))
        {
            throw new InvalidOperationException("Azure AI endpoint is not configured. Please set the 'AzureAI:Endpoint' in appsettings.json");
        }

        if (string.IsNullOrEmpty(config.ChatModelId))
        {
            throw new InvalidOperationException("Azure AI model ID is not configured. Please set the 'AzureAI:ChatModelId' in appsettings.json");
        }

        return config;
    }
}