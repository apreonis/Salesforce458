using InventoryManagement.Data;
using InventoryManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;

    public CommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Comment> AddCommentAsync(Guid inventoryId, string userId, string content)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            InventoryId = inventoryId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<List<Comment>> GetCommentsAsync(Guid inventoryId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.InventoryId == inventoryId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}