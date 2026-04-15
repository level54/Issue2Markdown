using Issue2Markdown.Services;

namespace Issue2Markdown.Tests;

public class FileTypeDetectorTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public FileTypeDetectorTests() => Directory.CreateDirectory(_tempDir);
    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private string WriteTempFile(byte[] bytes)
    {
        var path = Path.Combine(_tempDir, Guid.NewGuid().ToString());
        File.WriteAllBytes(path, bytes);
        return path;
    }

    [Fact]
    public void DetectExtension_Png()
    {
        var path = WriteTempFile([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        Assert.Equal(".png", FileTypeDetector.DetectExtension(path));
    }

    [Fact]
    public void DetectExtension_Jpg()
    {
        var path = WriteTempFile([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46]);
        Assert.Equal(".jpg", FileTypeDetector.DetectExtension(path));
    }

    [Fact]
    public void DetectExtension_Pdf()
    {
        var path = WriteTempFile([0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34]);
        Assert.Equal(".pdf", FileTypeDetector.DetectExtension(path));
    }

    [Fact]
    public void DetectExtension_Msg()
    {
        var path = WriteTempFile([0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]);
        Assert.Equal(".msg", FileTypeDetector.DetectExtension(path));
    }

    [Fact]
    public void DetectExtension_Txt_Ascii()
    {
        var path = WriteTempFile("Hello, world!\r\n"u8.ToArray());
        Assert.Equal(".txt", FileTypeDetector.DetectExtension(path));
    }

    [Fact]
    public void DetectExtension_Txt_Utf16Le()
    {
        var path = WriteTempFile([0xFF, 0xFE, 0x48, 0x00, 0x69, 0x00]);
        Assert.Equal(".txt", FileTypeDetector.DetectExtension(path));
    }

    [Fact]
    public void DetectExtension_Unknown_ReturnsBin()
    {
        var path = WriteTempFile([0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07]);
        Assert.Equal(".bin", FileTypeDetector.DetectExtension(path));
    }

    [Theory]
    [InlineData(".png", true)]
    [InlineData(".jpg", true)]
    [InlineData(".jpeg", true)]
    [InlineData(".gif", true)]
    [InlineData(".webp", true)]
    [InlineData(".bmp", true)]
    [InlineData(".svg", true)]
    [InlineData(".PNG", true)]
    [InlineData(".pdf", false)]
    [InlineData(".xlsx", false)]
    [InlineData(".zip", false)]
    [InlineData(".msg", false)]
    public void IsImage_CorrectlyClassifies(string ext, bool expected)
    {
        Assert.Equal(expected, FileTypeDetector.IsImage(ext));
    }
}
