using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public interface IItemService
{
    Task<Item> GetItemAsync(Guid id);
    Task<List<Item>> GetItemsForInventoryAsync(Guid inventoryId, string? searchTerm = null);
    Task<Item> CreateItemAsync(Item item, string userId);
    Task UpdateItemAsync(Item item, byte[] rowVersion);
    Task DeleteItemAsync(Guid id);
    Task ToggleLikeAsync(Guid itemId, string userId);
    Task<bool> UserCanModifyItemAsync(Guid itemId, string userId);
}