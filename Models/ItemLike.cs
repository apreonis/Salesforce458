namespace InventoryManagement.Data.Models;

public class ItemLike
{
    public Guid ItemId { get; set; }
    public Item Item { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}