using InventoryManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _context;

    public SearchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResult>> SearchAsync(string query)
    {
        query = query?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<SearchResult>();
        }

        var inventoryResults = await _context.Inventories
            .Where(i => EF.Functions.ILike(i.Title, $"%{query}%") || EF.Functions.ILike(i.Description, $"%{query}%"))
            .Select(i => new SearchResult
            {
                Id = i.Id,
                Title = i.Title,
                Type = "Inventory"
            })
            .ToListAsync();

        var itemResults = await _context.Items
            .Where(i =>
                EF.Functions.ILike(i.CustomId, $"%{query}%") ||
                EF.Functions.ILike(i.Text1, $"%{query}%") ||
                EF.Functions.ILike(i.Text2, $"%{query}%") ||
                EF.Functions.ILike(i.Text3, $"%{query}%") ||
                EF.Functions.ILike(i.MultiText1, $"%{query}%") ||
                EF.Functions.ILike(i.MultiText2, $"%{query}%") ||
                EF.Functions.ILike(i.MultiText3, $"%{query}%") ||
                EF.Functions.ILike(i.Document1, $"%{query}%") ||
                EF.Functions.ILike(i.Document2, $"%{query}%") ||
                EF.Functions.ILike(i.Document3, $"%{query}%"))
            .Select(i => new SearchResult
            {
                Id = i.Id,
                Title = i.CustomId,
                Type = "Item"
            })
            .ToListAsync();

        return inventoryResults
            .Concat(itemResults)
            .GroupBy(x => new { x.Id, x.Type })
            .Select(g => g.First())
            .ToList();
    }
}