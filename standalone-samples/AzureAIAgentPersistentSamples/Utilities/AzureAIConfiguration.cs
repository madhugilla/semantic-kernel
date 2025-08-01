// Copyright (c) Microsoft. All rights reserved.

namespace AzureAIAgentPersistentSamples.Utilities;

/// <summary>
/// Configuration for Azure AI Agent samples.
/// </summary>
public class AzureAIConfiguration
{
    /// <summary>
    /// Azure AI endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure AI chat model ID.
    /// </summary>
    public string ChatModelId { get; set; } = string.Empty;
}