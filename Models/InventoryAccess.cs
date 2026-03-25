namespace InventoryManagement.Data.Models;

public class InventoryAccess
{
    public Guid InventoryId { get; set; }
    public Inventory Inventory { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}