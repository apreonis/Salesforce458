namespace InventoryManagement.Data.Models.CustomId;

public class CustomIdFormat
{
    public List<CustomIdPart> Parts { get; set; } = new();
}

public class CustomIdPart
{
    public CustomIdPartType Type { get; set; }
    public string FixedText { get; set; }
    public int? Length { get; set; }
    public string Format { get; set; }
}

public enum CustomIdPartType
{
    FixedText,
    Random20Bit,
    Random32Bit,
    Random6Digit,
    Random9Digit,
    Guid,
    DateTime,
    Sequence
}