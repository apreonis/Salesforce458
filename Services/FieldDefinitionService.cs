using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public class FieldDefinitionService : IFieldDefinitionService
{
    private readonly ApplicationDbContext _context;

    public FieldDefinitionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomFieldDefinition>> GetFieldsForInventoryAsync(Guid inventoryId)
    {
        return await _context.CustomFieldDefinitions
            .Where(f => f.InventoryId == inventoryId)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
    }

    public async Task<CustomFieldDefinition> AddFieldAsync(Guid inventoryId, string name,
        string description, CustomFieldType type, bool displayInTable)
    {
        var existingCount = await _context.CustomFieldDefinitions
            .CountAsync(f => f.InventoryId == inventoryId && f.FieldType == type);

        if (existingCount >= 3)
            throw new InvalidOperationException("Maximum 3 fields of this type allowed");

        int fieldIndex = 1;
        var usedIndexes = await _context.CustomFieldDefinitions
            .Where(f => f.InventoryId == inventoryId && f.FieldType == type)
            .Select(f => f.FieldIndex)
            .ToListAsync();

        while (usedIndexes.Contains(fieldIndex) && fieldIndex <= 3)
            fieldIndex++;

        if (fieldIndex > 3)
            throw new InvalidOperationException("No available field index");

        var field = new CustomFieldDefinition
        {
            Id = Guid.NewGuid(),
            InventoryId = inventoryId,
            Name = name,
            Description = description,
            FieldType = type,
            FieldIndex = fieldIndex,
            DisplayInTable = displayInTable,
            DisplayOrder = await _context.CustomFieldDefinitions
                .Where(f => f.InventoryId == inventoryId)
                .MaxAsync(f => (int?)f.DisplayOrder) + 1 ?? 0
        };

        _context.CustomFieldDefinitions.Add(field);
        await _context.SaveChangesAsync();
        return field;
    }

    public async Task UpdateFieldAsync(CustomFieldDefinition field)
    {
        _context.CustomFieldDefinitions.Update(field);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFieldAsync(Guid fieldId)
    {
        var field = await _context.CustomFieldDefinitions.FindAsync(fieldId);
        if (field != null)
        {
            _context.CustomFieldDefinitions.Remove(field);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderFieldsAsync(Guid inventoryId, Dictionary<Guid, int> fieldOrders)
    {
        foreach (var order in fieldOrders)
        {
            var field = await _context.CustomFieldDefinitions
                .FirstOrDefaultAsync(f => f.Id == order.Key && f.InventoryId == inventoryId);
            if (field != null)
            {
                field.DisplayOrder = order.Value;
            }
        }
        await _context.SaveChangesAsync();
    }
}