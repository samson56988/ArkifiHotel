using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Admin.Infrastructure.Helpers;

public static class BusinessLogoFileHelper
{
    public const long MaxUploadBytes = 8 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
    };

    public static string? Validate(IFormFile? file)
    {
        if (file is null || file.Length <= 0)
        {
            return "No logo file uploaded.";
        }

        if (file.Length > MaxUploadBytes)
        {
            return "Logo must be 8MB or smaller.";
        }

        if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            return "Only JPEG or PNG images are allowed.";
        }

        return null;
    }

    public static async Task<(string? RelativePath, string? Error)> SaveAsync(
        IWebHostEnvironment env,
        Guid businessId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(file);
        if (validationError is not null)
        {
            return (null, validationError);
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || ext.Length > 10)
        {
            ext = file.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                _ => ".bin",
            };
        }

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var relativeFolder = $"uploads/{businessId:N}/logo";
        var physicalDir = Path.Combine(webRoot, "uploads", businessId.ToString("N"), "logo");
        Directory.CreateDirectory(physicalDir);

        foreach (var existing in Directory.EnumerateFiles(physicalDir))
        {
            try
            {
                File.Delete(existing);
            }
            catch
            {
                // ignore stale file cleanup errors
            }
        }

        var fileName = $"logo{ext}";
        var relativePath = $"{relativeFolder}/{fileName}";
        var physicalPath = Path.Combine(physicalDir, fileName);

        try
        {
            await using var stream = File.Create(physicalPath);
            await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return (null, "Could not save logo.");
        }

        return (relativePath, null);
    }

    public static void TryDeleteStoredFile(IWebHostEnvironment env, string? relativeOrAbsolutePath)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
        {
            return;
        }

        var path = relativeOrAbsolutePath;
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(path);
            path = uri.AbsolutePath;
        }

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var combined = Path.Combine(webRoot, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        try
        {
            var full = Path.GetFullPath(combined);
            var root = Path.GetFullPath(webRoot);
            if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(full))
            {
                File.Delete(full);
            }
        }
        catch
        {
            // ignore IO errors
        }
    }
}
