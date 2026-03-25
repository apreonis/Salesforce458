using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public interface ISearchService
{
    Task<List<SearchResult>> SearchAsync(string query);
}

public class SearchResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}