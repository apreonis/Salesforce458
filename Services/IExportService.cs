namespace InventoryManagement.Services;

public interface IExportService
{
    Task<byte[]> ExportInventoryToCsvAsync(Guid inventoryId);
    Task<byte[]> ExportInventoryToExcelAsync(Guid inventoryId);
}