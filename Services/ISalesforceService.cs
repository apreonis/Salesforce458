using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public interface ISalesforceService
{
    Task<SalesforceIntegrationResult> CreateAccountAndContactAsync(
        SalesforceProfileSyncModel model,
        CancellationToken cancellationToken = default);
}