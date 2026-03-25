namespace InventoryManagement.Data.Models;

public class CustomFieldDefinition
{
    public Guid Id { get; set; }
    public Guid InventoryId { get; set; }
    public Inventory Inventory { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public CustomFieldType FieldType { get; set; }
    public int FieldIndex { get; set; }
    public bool DisplayInTable { get; set; }
    public int DisplayOrder { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MaxLength { get; set; }
    public string RegexPattern { get; set; }
}

public enum CustomFieldType
{
    SingleLineText,
    MultiLineText,
    Number,
    Document,
    Boolean
}