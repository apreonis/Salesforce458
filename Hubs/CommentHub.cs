using Microsoft.AspNetCore.SignalR;
using InventoryManagement.Services;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagement.Hubs;

[Authorize]
public class CommentHub : Hub
{
    private readonly ICommentService _commentService;

    public CommentHub(ICommentService commentService)
    {
        _commentService = commentService;
    }

    public async Task SendComment(Guid inventoryId, string content)
    {
        var userId = Context.UserIdentifier;
        var comment = await _commentService.AddCommentAsync(inventoryId, userId, content);
        await Clients.Group(inventoryId.ToString()).SendAsync("ReceiveComment", comment);
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext.Request.Query.TryGetValue("inventoryId", out var inventoryId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, inventoryId);
        }
        await base.OnConnectedAsync();
    }
}