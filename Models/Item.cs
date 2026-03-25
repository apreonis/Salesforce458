namespace InventoryManagement.Data.Models;

public class Item
{
    public Guid Id { get; set; }
    public Guid InventoryId { get; set; }
    public Inventory Inventory { get; set; } = null!;
    public string CustomId { get; set; } = string.Empty;
    public string CreatedById { get; set; } = string.Empty;
    public ApplicationUser CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Text1 { get; set; } = string.Empty;
    public string Text2 { get; set; } = string.Empty;
    public string Text3 { get; set; } = string.Empty;
    public string MultiText1 { get; set; } = string.Empty;
    public string MultiText2 { get; set; } = string.Empty;
    public string MultiText3 { get; set; } = string.Empty;
    public decimal? Number1 { get; set; }
    public decimal? Number2 { get; set; }
    public decimal? Number3 { get; set; }
    public string Document1 { get; set; } = string.Empty;
    public string Document2 { get; set; } = string.Empty;
    public string Document3 { get; set; } = string.Empty;
    public bool? Boolean1 { get; set; }
    public bool? Boolean2 { get; set; }
    public bool? Boolean3 { get; set; }
    public byte[] RowVersion { get; set; } = Guid.NewGuid().ToByteArray();
    public ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
}