using SkiaSharp;

namespace Cookr.Api.Tests;

internal static class TestImages
{
    public static byte[] Png(int width, int height) => Encode(width, height, SKEncodedImageFormat.Png);

    public static byte[] Webp(int width, int height) => Encode(width, height, SKEncodedImageFormat.Webp);

    public static byte[] WebpWithExif(int width, int height)
    {
        var imageChunk = Webp(width, height).AsSpan(12);
        byte[] exif = [0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        using var chunks = new MemoryStream();
        using (var writer = new BinaryWriter(chunks, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            writer.Write("VP8X"u8);
            writer.Write(10u);
            writer.Write((byte)0x08);
            writer.Write(new byte[3]);
            WriteUInt24(writer, width - 1);
            WriteUInt24(writer, height - 1);
            writer.Write(imageChunk);
            writer.Write("EXIF"u8);
            writer.Write((uint)exif.Length);
            writer.Write(exif);
        }

        using var file = new MemoryStream();
        using (var writer = new BinaryWriter(file))
        {
            writer.Write("RIFF"u8);
            writer.Write((uint)(4 + chunks.Length));
            writer.Write("WEBP"u8);
            writer.Write(chunks.ToArray());
        }

        return file.ToArray();
    }

    private static void WriteUInt24(BinaryWriter writer, int value)
    {
        writer.Write((byte)(value & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
        writer.Write((byte)((value >> 16) & 0xFF));
    }

    private static byte[] Encode(int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Orange);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 100);
        return data.ToArray();
    }

    public static byte[] Gif() =>
        Convert.FromBase64String("R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==");

    public static MultipartFormDataContent Upload(byte[] bytes, string fileName = "cover.png", string contentType = "image/png")
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", fileName } };
    }
}
