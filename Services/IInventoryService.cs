using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public interface IInventoryService
{
    Task<Inventory> GetInventoryAsync(Guid id);
    Task<List<Inventory>> GetUserInventoriesAsync(string userId, bool writableOnly = false);
    Task<Inventory> CreateInventoryAsync(Inventory inventory, string ownerId);
    Task UpdateInventoryAsync(Inventory inventory, byte[] rowVersion);
    Task DeleteInventoryAsync(Guid id);
    Task<bool> UserCanWriteAsync(Guid inventoryId, string userId);
    Task GrantWriteAccessAsync(Guid inventoryId, string userId);
    Task RevokeWriteAccessAsync(Guid inventoryId, string userId);
    Task SetPublicAsync(Guid inventoryId, bool isPublic);
}