using InventoryManagement.Data.Models.CustomId;

namespace InventoryManagement.Data.Models;

public class Inventory
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InventoryCategory Category { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public byte[] RowVersion { get; set; } = Guid.NewGuid().ToByteArray();
    public CustomIdFormat CustomIdFormat { get; set; } = new();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<CustomFieldDefinition> CustomFieldDefinitions { get; set; } = new List<CustomFieldDefinition>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<InventoryAccess> AccessList { get; set; } = new List<InventoryAccess>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

public enum InventoryCategory
{
    Equipment,
    Furniture,
    Book,
    Other
}