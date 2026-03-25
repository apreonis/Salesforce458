using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public interface IFieldDefinitionService
{
    Task<List<CustomFieldDefinition>> GetFieldsForInventoryAsync(Guid inventoryId);
    Task<CustomFieldDefinition> AddFieldAsync(Guid inventoryId, string name, string description,
        CustomFieldType type, bool displayInTable);
    Task UpdateFieldAsync(CustomFieldDefinition field);
    Task DeleteFieldAsync(Guid fieldId);
    Task ReorderFieldsAsync(Guid inventoryId, Dictionary<Guid, int> fieldOrders);
}