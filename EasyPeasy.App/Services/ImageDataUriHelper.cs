namespace EasyPeasy.App.Services;

/// <summary>
/// Detects an image format from its magic-number header (no MIME type is stored alongside the bytes
/// anywhere in this app) and builds a `data:` URI for an <c>&lt;img&gt;</c> tag. Falls back to JPEG
/// when the format isn't recognized.
/// </summary>
public static class ImageDataUriHelper
{
    /// <summary>Builds a base64 <c>data:</c> URI from raw image bytes, or <c>null</c> if <paramref name="imageBytes"/> is empty/missing.</summary>
    public static string? BuildDataUri(byte[]? imageBytes)
    {
        if (imageBytes is not { Length: > 0 })
            return null;

        var (mimeType, _) = DetectFormat(imageBytes);
        return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
    }

    /// <summary>File extension (without the dot) for the given image bytes, e.g. "png".</summary>
    public static string DetectExtension(byte[] imageBytes) => DetectFormat(imageBytes).Extension;

    private static (string MimeType, string Extension) DetectFormat(byte[] bytes)
    {
        // PNG: 89 50 4E 47
        if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return ("image/png", "png");

        // GIF: "GIF8"
        if (bytes.Length >= 4 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
            return ("image/gif", "gif");

        // WEBP: "RIFF"...."WEBP"
        if (bytes.Length >= 12 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46
            && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            return ("image/webp", "webp");

        // JPEG: FF D8 FF — also the default fallback
        return ("image/jpeg", "jpg");
    }
}
