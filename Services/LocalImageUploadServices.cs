using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace InventoryManagement.Services
{
    public class LocalImageUploadService : IImageUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _uploadsFolder;

        public LocalImageUploadService(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;

            var configured = config["UPLOADS_FOLDER"];

            _uploadsFolder = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads")
                : configured;

            if (!Directory.Exists(_uploadsFolder))
                Directory.CreateDirectory(_uploadsFolder);
        }

        public async Task<string> UploadAsync(IBrowserFile file, CancellationToken cancellationToken = default)
        {
            var maxSize = 10 * 1024 * 1024;

            await using var stream = file.OpenReadStream(maxSize);

            return await UploadAsync(stream, file.Name, cancellationToken);
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            var ext = Path.GetExtension(fileName);

            var safeName = Guid.NewGuid().ToString("N") + ext;

            var path = Path.Combine(_uploadsFolder, safeName);

            await using var fs = new FileStream(path, FileMode.Create);

            await stream.CopyToAsync(fs, cancellationToken);

            var relative = Path.GetRelativePath(_env.WebRootPath ?? "wwwroot", path)
                .Replace('\\', '/')
                .TrimStart('/');

            return "/" + relative;
        }

        public Task DeleteAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Task.CompletedTask;

            var relative = url.TrimStart('/');

            var path = Path.Combine(_env.WebRootPath ?? "wwwroot",
                relative.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(path))
                File.Delete(path);

            return Task.CompletedTask;
        }
    }
}