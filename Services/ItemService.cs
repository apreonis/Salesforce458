using InventoryManagement.Data;
using InventoryManagement.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services;

public class ItemService : IItemService
{
    private readonly ApplicationDbContext _context;
    private readonly ICustomIdGenerator _idGenerator;
    private readonly UserManager<ApplicationUser> _userManager;

    public ItemService(ApplicationDbContext context, ICustomIdGenerator idGenerator, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _idGenerator = idGenerator;
        _userManager = userManager;
    }

    public async Task<Item> GetItemAsync(Guid id)
    {
        return await _context.Items
            .Include(i => i.Inventory)
                .ThenInclude(inv => inv.Owner)
            .Include(i => i.Inventory)
                .ThenInclude(inv => inv.AccessList)
                    .ThenInclude(a => a.User)
            .Include(i => i.CreatedBy)
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Item>> GetItemsForInventoryAsync(Guid inventoryId, string? searchTerm = null)
    {
        var query = _context.Items
            .Include(i => i.CreatedBy)
            .Include(i => i.Likes)
            .Where(i => i.InventoryId == inventoryId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(i =>
                EF.Functions.ILike(i.CustomId, $"%{term}%") ||
                EF.Functions.ILike(i.Text1, $"%{term}%") ||
                EF.Functions.ILike(i.Text2, $"%{term}%") ||
                EF.Functions.ILike(i.Text3, $"%{term}%") ||
                EF.Functions.ILike(i.MultiText1, $"%{term}%") ||
                EF.Functions.ILike(i.MultiText2, $"%{term}%") ||
                EF.Functions.ILike(i.MultiText3, $"%{term}%") ||
                EF.Functions.ILike(i.Document1, $"%{term}%") ||
                EF.Functions.ILike(i.Document2, $"%{term}%") ||
                EF.Functions.ILike(i.Document3, $"%{term}%"));
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Item> CreateItemAsync(Item item, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Cannot determine current user.");
        }

        item.Id = Guid.NewGuid();
        item.CreatedById = userId;
        item.CreatedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(item.CustomId))
        {
            item.CustomId = await _idGenerator.GenerateIdAsync(item.InventoryId);
        }
        else
        {
            item.CustomId = item.CustomId.Trim();
        }

        item.RowVersion = Guid.NewGuid().ToByteArray();

        var duplicate = await _context.Items.AnyAsync(i => i.InventoryId == item.InventoryId && i.CustomId == item.CustomId);
        if (duplicate)
        {
            throw new InvalidOperationException("An item with this Custom ID already exists in this inventory.");
        }

        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateItemAsync(Item item, byte[] rowVersion)
    {
        _context.Entry(item).Property(nameof(Item.RowVersion)).OriginalValue = rowVersion;
        item.RowVersion = Guid.NewGuid().ToByteArray();
        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(Guid id)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleLikeAsync(Guid itemId, string userId)
    {
        var like = await _context.ItemLikes.FirstOrDefaultAsync(l => l.ItemId == itemId && l.UserId == userId);
        if (like == null)
        {
            _context.ItemLikes.Add(new ItemLike
            {
                ItemId = itemId,
                UserId = userId
            });
        }
        else
        {
            _context.ItemLikes.Remove(like);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> UserCanModifyItemAsync(Guid itemId, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return true;
        }

        var item = await _context.Items
            .Include(i => i.Inventory)
                .ThenInclude(inv => inv.AccessList)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
        {
            return false;
        }

        if (item.Inventory.OwnerId == userId)
        {
            return true;
        }

        if (item.Inventory.IsPublic)
        {
            return true;
        }

        return item.Inventory.AccessList.Any(a => a.UserId == userId);
    }
}