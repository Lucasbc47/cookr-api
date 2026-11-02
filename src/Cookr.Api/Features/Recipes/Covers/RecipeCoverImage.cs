using System.Buffers.Binary;
using Cookr.Api.Shared;
using SkiaSharp;

namespace Cookr.Api.Features.Recipes.Covers;

internal static class RecipeCoverImage
{
    public const long MaxUploadBytes = 10 * 1024 * 1024;
    private const long MaxPixels = 40_000_000;
    private const int MaxSide = 1600;
    private const int WebpQuality = 80;

    private static readonly SKEncodedImageFormat[] AcceptedFormats =
        [SKEncodedImageFormat.Jpeg, SKEncodedImageFormat.Png, SKEncodedImageFormat.Webp];

    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    public static Result<byte[]> ToWebp(Stream content)
    {
        if (content.Length > MaxUploadBytes) return Result<byte[]>.Invalid(RecipeCoverErrors.TooLarge);

        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        var original = buffer.ToArray();

        using var codec = SKCodec.Create(new MemoryStream(original));
        if (codec is null || !AcceptedFormats.Contains(codec.EncodedFormat))
        {
            return Result<byte[]>.Invalid(RecipeCoverErrors.InvalidImage);
        }

        if ((long)codec.Info.Width * codec.Info.Height > MaxPixels)
        {
            return Result<byte[]>.Invalid(RecipeCoverErrors.TooLarge);
        }

        if (IsReadyWebp(codec, original)) return Result<byte[]>.Success(original);

        using var source = SKBitmap.Decode(codec);
        if (source is null) return Result<byte[]>.Invalid(RecipeCoverErrors.InvalidImage);

        using var cover = Orient(source, codec.EncodedOrigin, MaxSide);
        using var image = SKImage.FromBitmap(cover);
        using var encoded = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);
        return Result<byte[]>.Success(encoded.ToArray());
    }

    private static bool IsReadyWebp(SKCodec codec, byte[] bytes) =>
        codec.EncodedFormat == SKEncodedImageFormat.Webp
        && codec.EncodedOrigin == SKEncodedOrigin.TopLeft
        && codec.FrameCount <= 1
        && Math.Max(codec.Info.Width, codec.Info.Height) <= MaxSide
        && !HasWebpMetadata(bytes);

    public static bool HasWebpMetadata(ReadOnlySpan<byte> bytes)
    {
        const byte ExifFlag = 0x08;
        const byte XmpFlag = 0x04;
        const byte AnimationFlag = 0x02;
        const int HeaderLength = 12;
        const int ChunkHeaderLength = 8;

        if (bytes.Length > 20 && bytes[12..16].SequenceEqual("VP8X"u8)
            && (bytes[20] & (ExifFlag | XmpFlag | AnimationFlag)) != 0)
        {
            return true;
        }

        var offset = HeaderLength;
        while (offset + ChunkHeaderLength <= bytes.Length)
        {
            var fourCc = bytes.Slice(offset, 4);
            if (fourCc.SequenceEqual("EXIF"u8) || fourCc.SequenceEqual("XMP "u8)) return true;
            var size = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset + 4, 4));
            var next = offset + ChunkHeaderLength + (long)size + (size % 2);
            if (next <= offset || next > int.MaxValue) return true;
            offset = (int)next;
        }

        return false;
    }

    public static SKBitmap Orient(SKBitmap source, SKEncodedOrigin origin, int maxSide)
    {
        float width = source.Width;
        float height = source.Height;
        var swapsSides = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var scale = Math.Min(1f, maxSide / Math.Max(width, height));
        var targetWidth = Math.Max(1, (int)Math.Round((swapsSides ? height : width) * scale));
        var targetHeight = Math.Max(1, (int)Math.Round((swapsSides ? width : height) * scale));

        var target = new SKBitmap(targetWidth, targetHeight);
        using var canvas = new SKCanvas(target);
        canvas.Scale(scale);
        ApplyOrigin(canvas, origin, width, height);
        canvas.DrawBitmap(source, new SKRect(0, 0, width, height), Sampling);
        return target;
    }

    private static void ApplyOrigin(SKCanvas canvas, SKEncodedOrigin origin, float width, float height)
    {
        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Translate(width, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.BottomRight:
                canvas.Translate(width, height);
                canvas.Scale(-1, -1);
                break;
            case SKEncodedOrigin.BottomLeft:
                canvas.Translate(0, height);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftTop:
                canvas.RotateDegrees(90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.RightTop:
                canvas.Translate(height, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom:
                canvas.Translate(height, width);
                canvas.RotateDegrees(-90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, width);
                canvas.RotateDegrees(-90);
                break;
            default:
                break;
        }
    }
}
