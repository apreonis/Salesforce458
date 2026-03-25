using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _context;

    public StatisticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryStatistics> GetStatisticsAsync(Guid inventoryId)
    {
        var items = await _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .ToListAsync();
        var fields = await _context.CustomFieldDefinitions
            .Where(f => f.InventoryId == inventoryId)
            .ToListAsync();

        var stats = new InventoryStatistics
        {
            TotalItems = items.Count,
            FieldStats = new Dictionary<string, object>()
        };

        foreach (var field in fields.Where(f => f.FieldType == CustomFieldType.Number))
        {
            var values = items.Select(i => GetNumberValue(i, field.FieldIndex)).Where(v => v.HasValue).Select(v => v.Value).ToList();
            if (values.Any())
            {
                stats.FieldStats[$"{field.Name}_min"] = values.Min();
                stats.FieldStats[$"{field.Name}_max"] = values.Max();
                stats.FieldStats[$"{field.Name}_avg"] = values.Average();
                stats.FieldStats[$"{field.Name}_count"] = values.Count;
            }
        }

        foreach (var field in fields.Where(f => f.FieldType == CustomFieldType.SingleLineText))
        {
            var values = items.Select(i => GetTextValue(i, field.FieldIndex)).Where(v => !string.IsNullOrEmpty(v)).ToList();
            if (values.Any())
            {
                var mostFrequent = values.GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault();
                stats.FieldStats[$"{field.Name}_most_common"] = mostFrequent;
            }
        }

        return stats;
    }

    private decimal? GetNumberValue(Item item, int index)
    {
        return index switch
        {
            1 => item.Number1,
            2 => item.Number2,
            3 => item.Number3,
            _ => null
        };
    }

    private string GetTextValue(Item item, int index)
    {
        return index switch
        {
            1 => item.Text1,
            2 => item.Text2,
            3 => item.Text3,
            _ => null
        };
    }
}