using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public interface IStatisticsService
{
    Task<InventoryStatistics> GetStatisticsAsync(Guid inventoryId);
}

public class InventoryStatistics
{
    public int TotalItems { get; set; }
    public Dictionary<string, object> FieldStats { get; set; }
}