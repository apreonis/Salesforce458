using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace InventoryManagement.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadAsync(IBrowserFile file, CancellationToken cancellationToken = default);
        Task<string> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
        Task DeleteAsync(string url);
    }
}