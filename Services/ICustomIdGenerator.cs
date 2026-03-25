using InventoryManagement.Data.Models.CustomId;

namespace InventoryManagement.Services;

public interface ICustomIdGenerator
{
    Task<string> GenerateIdAsync(Guid inventoryId);
    bool ValidateFormat(string customId, CustomIdFormat format);
}