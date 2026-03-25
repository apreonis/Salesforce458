using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public interface ITagService
{
    Task<List<string>> SearchTagsAsync(string term);
    Task<Tag> GetOrCreateTagAsync(string name);
}