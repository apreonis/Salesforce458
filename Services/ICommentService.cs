using InventoryManagement.Data.Models;

namespace InventoryManagement.Services;

public interface ICommentService
{
    Task<Comment> AddCommentAsync(Guid inventoryId, string userId, string content);
    Task<List<Comment>> GetCommentsAsync(Guid inventoryId);
}