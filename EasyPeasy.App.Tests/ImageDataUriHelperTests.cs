using EasyPeasy.App.Services;

namespace EasyPeasy.App.Tests;

public class ImageDataUriHelperTests
{
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] GifHeader = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61];
    private static readonly byte[] WebpHeader =
        [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];
    private static readonly byte[] JpegHeader = [0xFF, 0xD8, 0xFF, 0xE0];

    [Fact]
    public void DetectExtension_Png_ReturnsPng()
    {
        Assert.Equal("png", ImageDataUriHelper.DetectExtension(PngHeader));
    }

    [Fact]
    public void DetectExtension_Gif_ReturnsGif()
    {
        Assert.Equal("gif", ImageDataUriHelper.DetectExtension(GifHeader));
    }

    [Fact]
    public void DetectExtension_Webp_ReturnsWebp()
    {
        Assert.Equal("webp", ImageDataUriHelper.DetectExtension(WebpHeader));
    }

    [Fact]
    public void DetectExtension_Jpeg_ReturnsJpg()
    {
        Assert.Equal("jpg", ImageDataUriHelper.DetectExtension(JpegHeader));
    }

    [Fact]
    public void DetectExtension_UnrecognizedBytes_FallsBackToJpg()
    {
        Assert.Equal("jpg", ImageDataUriHelper.DetectExtension([0x01, 0x02, 0x03, 0x04]));
    }

    [Fact]
    public void DetectExtension_TooShortForAnySignature_FallsBackToJpg()
    {
        Assert.Equal("jpg", ImageDataUriHelper.DetectExtension([0x89, 0x50]));
    }

    [Fact]
    public void BuildDataUri_Null_ReturnsNull()
    {
        Assert.Null(ImageDataUriHelper.BuildDataUri(null));
    }

    [Fact]
    public void BuildDataUri_EmptyArray_ReturnsNull()
    {
        Assert.Null(ImageDataUriHelper.BuildDataUri([]));
    }

    [Fact]
    public void BuildDataUri_PngBytes_BuildsCorrectDataUri()
    {
        var result = ImageDataUriHelper.BuildDataUri(PngHeader);

        var expectedBase64 = Convert.ToBase64String(PngHeader);
        Assert.Equal($"data:image/png;base64,{expectedBase64}", result);
    }

    [Fact]
    public void BuildDataUri_WebpBytes_UsesWebpMimeType()
    {
        var result = ImageDataUriHelper.BuildDataUri(WebpHeader);

        Assert.StartsWith("data:image/webp;base64,", result);
    }
}
