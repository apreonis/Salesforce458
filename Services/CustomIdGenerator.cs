using InventoryManagement.Data;
using InventoryManagement.Data.Models.CustomId;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace InventoryManagement.Services;

public class CustomIdGenerator : ICustomIdGenerator
{
    private readonly ApplicationDbContext _context;

    public CustomIdGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateIdAsync(Guid inventoryId)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            throw new InvalidOperationException("Inventory not found.");
        }

        var parts = new List<string>();

        foreach (var part in inventory.CustomIdFormat.Parts)
        {
            parts.Add(part.Type switch
            {
                CustomIdPartType.FixedText => part.FixedText ?? string.Empty,
                CustomIdPartType.Random20Bit => RandomNumberGenerator.GetInt32(1 << 20).ToString(),
                CustomIdPartType.Random32Bit => RandomNumberGenerator.GetInt32(int.MaxValue).ToString(),
                CustomIdPartType.Random6Digit => RandomNumberGenerator.GetInt32(100000, 1000000).ToString(part.Format ?? "D6"),
                CustomIdPartType.Random9Digit => RandomNumberGenerator.GetInt32(100000000, 1000000000).ToString(part.Format ?? "D9"),
                CustomIdPartType.Guid => Guid.NewGuid().ToString("N"),
                CustomIdPartType.DateTime => DateTime.UtcNow.ToString(part.Format ?? "yyyyMMddHHmmss"),
                CustomIdPartType.Sequence => await GetNextSequenceAsync(inventoryId, part.Format),
                _ => string.Empty
            });
        }

        var candidate = string.Concat(parts);

        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = Guid.NewGuid().ToString("N");
        }

        var suffix = 0;
        while (await _context.Items.AnyAsync(i => i.InventoryId == inventoryId && i.CustomId == candidate))
        {
            suffix++;
            candidate = $"{candidate}-{suffix}";
        }

        return candidate;
    }

    public bool ValidateFormat(string customId, CustomIdFormat format)
    {
        return !string.IsNullOrWhiteSpace(customId);
    }

    private async Task<string> GetNextSequenceAsync(Guid inventoryId, string? format)
    {
        var next = await _context.Items.CountAsync(i => i.InventoryId == inventoryId) + 1;
        return next.ToString(string.IsNullOrWhiteSpace(format) ? "D6" : format);
    }
}