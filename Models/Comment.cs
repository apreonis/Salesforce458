namespace InventoryManagement.Data.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid InventoryId { get; set; }
    public Inventory Inventory { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}