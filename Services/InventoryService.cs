using InventoryManagement.Data;
using InventoryManagement.Data.Models;
using InventoryManagement.Data.Models.CustomId;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InventoryService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Inventory> GetInventoryAsync(Guid id)
    {
        return await _context.Inventories
            .Include(i => i.Owner)
            .Include(i => i.Tags)
            .Include(i => i.AccessList)
                .ThenInclude(a => a.User)
            .Include(i => i.CustomFieldDefinitions.OrderBy(f => f.DisplayOrder))
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Inventory>> GetUserInventoriesAsync(string userId, bool writableOnly = false)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

        var query = _context.Inventories
            .Include(i => i.Owner)
            .Include(i => i.Items)
            .AsQueryable();

        if (!writableOnly)
        {
            query = query.Where(i => i.OwnerId == userId);
        }
        else if (!isAdmin)
        {
            query = query.Where(i =>
                i.OwnerId == userId ||
                i.IsPublic ||
                i.AccessList.Any(a => a.UserId == userId));
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Inventory> CreateInventoryAsync(Inventory inventory, string ownerId)
    {
        inventory.Id = Guid.NewGuid();
        inventory.OwnerId = ownerId;
        inventory.CreatedAt = DateTime.UtcNow;
        inventory.CustomIdFormat ??= new CustomIdFormat();
        inventory.RowVersion = Guid.NewGuid().ToByteArray();

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task UpdateInventoryAsync(Inventory inventory, byte[] rowVersion)
    {
        _context.Entry(inventory).Property(nameof(Inventory.RowVersion)).OriginalValue = rowVersion;
        inventory.RowVersion = Guid.NewGuid().ToByteArray();
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInventoryAsync(Guid id)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id);
        if (inventory != null)
        {
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> UserCanWriteAsync(Guid inventoryId, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return true;
        }

        var inventory = await _context.Inventories
            .Include(i => i.AccessList)
            .FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return false;
        }

        if (inventory.OwnerId == userId)
        {
            return true;
        }

        if (inventory.IsPublic)
        {
            return true;
        }

        return inventory.AccessList.Any(a => a.UserId == userId);
    }

    public async Task GrantWriteAccessAsync(Guid inventoryId, string userId)
    {
        var exists = await _context.InventoryAccesses.AnyAsync(a => a.InventoryId == inventoryId && a.UserId == userId);
        if (!exists)
        {
            _context.InventoryAccesses.Add(new InventoryAccess
            {
                InventoryId = inventoryId,
                UserId = userId
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeWriteAccessAsync(Guid inventoryId, string userId)
    {
        var access = await _context.InventoryAccesses
            .FirstOrDefaultAsync(a => a.InventoryId == inventoryId && a.UserId == userId);

        if (access != null)
        {
            _context.InventoryAccesses.Remove(access);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetPublicAsync(Guid inventoryId, bool isPublic)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);
        if (inventory != null)
        {
            inventory.IsPublic = isPublic;
            await _context.SaveChangesAsync();
        }
    }
}