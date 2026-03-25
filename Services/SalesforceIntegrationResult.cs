using System.Text.Json.Serialization;

namespace InventoryManagement.Services;

public sealed record SalesforceIntegrationResult(string AccountId, string ContactId);

internal sealed class SalesforceTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("instance_url")]
    public string InstanceUrl { get; set; } = string.Empty;
}

internal sealed class SalesforceCreateRecordResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}