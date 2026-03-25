using InventoryManagement.Data.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventoryManagement.Services;

public sealed class SalesforceService : ISalesforceService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly SalesforceOptions _options;

    public SalesforceService(HttpClient httpClient, IOptions<SalesforceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<SalesforceIntegrationResult> CreateAccountAndContactAsync(
        SalesforceProfileSyncModel model,
        CancellationToken cancellationToken = default)
    {
        ValidateOptions();

        var token = await GetAccessTokenAsync(cancellationToken);

        var accountId = await CreateRecordAsync(
            token.InstanceUrl,
            token.AccessToken,
            "Account",
            new
            {
                Name = model.AccountName.Trim(),
                Phone = Normalize(model.AccountPhone),
                Website = Normalize(model.AccountWebsite),
                Description = Normalize(model.Description),
                BillingCity = Normalize(model.MailingCity),
                BillingCountry = Normalize(model.MailingCountry)
            },
            cancellationToken);

        try
        {
            var contactId = await CreateRecordAsync(
                token.InstanceUrl,
                token.AccessToken,
                "Contact",
                new
                {
                    FirstName = Normalize(model.ContactFirstName),
                    LastName = Normalize(model.ContactLastName),
                    Email = Normalize(model.ContactEmail),
                    Phone = Normalize(model.ContactPhone),
                    Title = Normalize(model.ContactTitle),
                    MailingCity = Normalize(model.MailingCity),
                    MailingCountry = Normalize(model.MailingCountry),
                    Description = Normalize(model.Description),
                    AccountId = accountId
                },
                cancellationToken);

            return new SalesforceIntegrationResult(accountId, contactId);
        }
        catch
        {
            try
            {
                await DeleteRecordAsync(token.InstanceUrl, token.AccessToken, "Account", accountId, cancellationToken);
            }
            catch
            {
            }

            throw;
        }
    }

    private async Task<SalesforceTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var tokenEndpoint = $"{NormalizeBaseUrl(_options.BaseUrl)}/services/oauth2/token";

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId.Trim(),
            ["client_secret"] = _options.ClientSecret.Trim()
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Salesforce auth failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseText}");
        }

        var token = JsonSerializer.Deserialize<SalesforceTokenResponse>(responseText, JsonOptions)
            ?? throw new InvalidOperationException("Salesforce auth response is empty.");

        if (string.IsNullOrWhiteSpace(token.AccessToken) || string.IsNullOrWhiteSpace(token.InstanceUrl))
        {
            throw new InvalidOperationException("Salesforce auth response is invalid.");
        }

        return token;
    }

    private async Task<string> CreateRecordAsync(
        string instanceUrl,
        string accessToken,
        string objectName,
        object payload,
        CancellationToken cancellationToken)
    {
        var apiVersion = NormalizeApiVersion(_options.ApiVersion);
        var url = $"{NormalizeBaseUrl(instanceUrl)}/services/data/{apiVersion}/sobjects/{objectName}/";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Salesforce {objectName} create failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseText}");
        }

        var created = JsonSerializer.Deserialize<SalesforceCreateRecordResponse>(responseText, JsonOptions)
            ?? throw new InvalidOperationException($"Salesforce {objectName} create response is empty.");

        if (string.IsNullOrWhiteSpace(created.Id))
        {
            throw new InvalidOperationException($"Salesforce {objectName} create response is invalid.");
        }

        return created.Id;
    }

    private async Task DeleteRecordAsync(
        string instanceUrl,
        string accessToken,
        string objectName,
        string id,
        CancellationToken cancellationToken)
    {
        var apiVersion = NormalizeApiVersion(_options.ApiVersion);
        var url = $"{NormalizeBaseUrl(instanceUrl)}/services/data/{apiVersion}/sobjects/{objectName}/{id}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Salesforce {objectName} delete failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseText}");
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl) ||
            string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException("Salesforce integration is not configured.");
        }
    }

    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeBaseUrl(string value) => value.Trim().TrimEnd('/');

    private static string NormalizeApiVersion(string value)
    {
        var version = value.Trim();
        if (!version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            version = $"v{version}";
        }

        return version;
    }
}