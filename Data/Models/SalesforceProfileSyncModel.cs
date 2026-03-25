using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Data.Models;

public sealed class SalesforceProfileSyncModel
{
    [Required]
    [StringLength(255)]
    public string AccountName { get; set; } = string.Empty;

    [StringLength(40)]
    public string? AccountPhone { get; set; }

    [StringLength(255)]
    public string? AccountWebsite { get; set; }

    [Required]
    [StringLength(80)]
    public string ContactFirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string ContactLastName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(255)]
    public string? ContactEmail { get; set; }

    [StringLength(40)]
    public string? ContactPhone { get; set; }

    [StringLength(80)]
    public string? ContactTitle { get; set; }

    [StringLength(100)]
    public string? MailingCity { get; set; }

    [StringLength(100)]
    public string? MailingCountry { get; set; }

    [StringLength(3200)]
    public string? Description { get; set; }

    public static SalesforceProfileSyncModel FromUser(ApplicationUser user)
    {
        var displayName =
            !string.IsNullOrWhiteSpace(user.DisplayName)
                ? user.DisplayName.Trim()
                : !string.IsNullOrWhiteSpace(user.UserName)
                    ? user.UserName.Trim()
                    : (user.Email ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = "Inventory User";
        }

        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = parts.Length > 0 ? parts[0] : displayName;
        var lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "User";

        return new SalesforceProfileSyncModel
        {
            AccountName = displayName,
            ContactFirstName = firstName,
            ContactLastName = lastName,
            ContactEmail = user.Email ?? string.Empty,
            ContactPhone = user.PhoneNumber,
            Description = $"Synced from Inventory Management for {displayName}"
        };
    }
}