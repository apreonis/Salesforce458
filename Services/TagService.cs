using InventoryManagement.Data;
using InventoryManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services;

public class TagService : ITagService
{
    private readonly ApplicationDbContext _context;

    public TagService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> SearchTagsAsync(string term)
    {
        term = term?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(term))
            return new List<string>();

        return await _context.Tags
            .Where(t => EF.Functions.ILike(t.Name, $"{term}%"))
            .OrderBy(t => t.Name)
            .Select(t => t.Name)
            .Take(10)
            .ToListAsync();
    }

    public async Task<Tag> GetOrCreateTagAsync(string name)
    {
        name = name.Trim();

        var tag = await _context.Tags.FirstOrDefaultAsync(t => EF.Functions.ILike(t.Name, name));

        if (tag == null)
        {
            tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = name,
                Inventories = new List<Inventory>()
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }

        return tag;
    }
}