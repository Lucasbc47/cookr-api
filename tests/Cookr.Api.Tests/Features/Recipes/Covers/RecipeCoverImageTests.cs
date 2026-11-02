using Cookr.Api.Features.Recipes.Covers;
using SkiaSharp;

namespace Cookr.Api.Tests.Features.Recipes.Covers;

public class RecipeCoverImageTests
{
    [Theory]
    [InlineData(SKEncodedOrigin.TopLeft, 3, 2, 0, 0)]
    [InlineData(SKEncodedOrigin.TopRight, 3, 2, 2, 0)]
    [InlineData(SKEncodedOrigin.BottomRight, 3, 2, 2, 1)]
    [InlineData(SKEncodedOrigin.BottomLeft, 3, 2, 0, 1)]
    [InlineData(SKEncodedOrigin.LeftTop, 2, 3, 0, 0)]
    [InlineData(SKEncodedOrigin.RightTop, 2, 3, 1, 0)]
    [InlineData(SKEncodedOrigin.RightBottom, 2, 3, 1, 2)]
    [InlineData(SKEncodedOrigin.LeftBottom, 2, 3, 0, 2)]
    public void Orient_ExifOrigin_MovesTopLeftPixelToUprightPosition(
        SKEncodedOrigin origin, int width, int height, int redX, int redY)
    {
        using var source = new SKBitmap(3, 2);
        source.Erase(SKColors.White);
        source.SetPixel(0, 0, SKColors.Red);

        using var oriented = RecipeCoverImage.Orient(source, origin, maxSide: 1600);

        Assert.Equal(width, oriented.Width);
        Assert.Equal(height, oriented.Height);
        Assert.Equal(SKColors.Red, oriented.GetPixel(redX, redY));
    }

    [Fact]
    public void ToWebp_LargePhoto_ResizesLongestSideTo1600()
    {
        using var content = new MemoryStream(TestImages.Png(4000, 2000));

        var result = RecipeCoverImage.ToWebp(content);

        Assert.True(result.IsSuccess);
        using var codec = SKCodec.Create(new MemoryStream(result.Value));
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
        Assert.Equal(1600, codec.Info.Width);
        Assert.Equal(800, codec.Info.Height);
    }

    [Fact]
    public void ToWebp_SmallPhoto_KeepsSize()
    {
        using var content = new MemoryStream(TestImages.Png(800, 600));

        var result = RecipeCoverImage.ToWebp(content);

        Assert.True(result.IsSuccess);
        using var codec = SKCodec.Create(new MemoryStream(result.Value));
        Assert.Equal(800, codec.Info.Width);
        Assert.Equal(600, codec.Info.Height);
    }

    [Fact]
    public void ToWebp_SmallWebpWithoutMetadata_KeepsOriginalBytes()
    {
        var webp = TestImages.Webp(800, 600);
        using var content = new MemoryStream(webp);

        var result = RecipeCoverImage.ToWebp(content);

        Assert.True(result.IsSuccess);
        Assert.Equal(webp, result.Value);
    }

    [Fact]
    public void ToWebp_WebpWithExif_ReencodesWithoutMetadata()
    {
        var webp = TestImages.WebpWithExif(800, 600);
        Assert.True(RecipeCoverImage.HasWebpMetadata(webp));
        using var content = new MemoryStream(webp);

        var result = RecipeCoverImage.ToWebp(content);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(webp, result.Value);
        Assert.False(RecipeCoverImage.HasWebpMetadata(result.Value));
        using var codec = SKCodec.Create(new MemoryStream(result.Value));
        Assert.Equal(800, codec.Info.Width);
    }

    [Fact]
    public void ToWebp_LargeWebp_ResizesLongestSideTo1600()
    {
        using var content = new MemoryStream(TestImages.Webp(3200, 1600));

        var result = RecipeCoverImage.ToWebp(content);

        Assert.True(result.IsSuccess);
        using var codec = SKCodec.Create(new MemoryStream(result.Value));
        Assert.Equal(1600, codec.Info.Width);
        Assert.Equal(800, codec.Info.Height);
    }

    [Fact]
    public void ToWebp_TextFile_ReturnsInvalidImage()
    {
        using var content = new MemoryStream("not an image"u8.ToArray());

        var result = RecipeCoverImage.ToWebp(content);

        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeCoverErrors.InvalidImage, result.Error);
    }

    [Fact]
    public void ToWebp_GifImage_ReturnsInvalidImage()
    {
        using var content = new MemoryStream(TestImages.Gif());

        var result = RecipeCoverImage.ToWebp(content);

        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeCoverErrors.InvalidImage, result.Error);
    }
}
