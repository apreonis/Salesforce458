namespace InventoryManagement.Services;

public sealed class SalesforceOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v61.0";
}