using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsBlocked { get; set; }
    public ICollection<Inventory> OwnedInventories { get; set; } = new List<Inventory>();
    public ICollection<InventoryAccess> InventoryAccesses { get; set; } = new List<InventoryAccess>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<ItemLike> ItemLikes { get; set; } = new List<ItemLike>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}