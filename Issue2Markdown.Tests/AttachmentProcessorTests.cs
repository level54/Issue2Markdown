using Issue2Markdown.Models;
using Issue2Markdown.Services;

namespace Issue2Markdown.Tests;

public class AttachmentProcessorTests : IDisposable
{
    private readonly string _inputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly string _outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly AttachmentProcessor _processor = new();

    public AttachmentProcessorTests()
    {
        Directory.CreateDirectory(Path.Combine(_inputDir, "attachments"));
        Directory.CreateDirectory(_outputDir);
    }

    public void Dispose()
    {
        Directory.Delete(_inputDir, recursive: true);
        Directory.Delete(_outputDir, recursive: true);
    }

    private void CreateFakeAttachmentFile(string guid, byte[]? content = null)
    {
        var path = Path.Combine(_inputDir, "attachments", guid);
        File.WriteAllBytes(path, content ?? [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]); // PNG magic bytes
    }

    [Fact]
    public void ProcessIssue_CopiesFileWithCorrectName()
    {
        CreateFakeAttachmentFile("abc123");
        var issue = new Issue { Id = 7 };
        var attachments = new List<Attachment>
        {
            new() { Issue = 7, Filename = "screenshot.png", Path = "attachments/abc123" }
        };

        var mappings = _processor.ProcessIssue(issue, attachments, _inputDir, _outputDir);

        Assert.Single(mappings);
        Assert.True(File.Exists(Path.Combine(_outputDir, "attachments", "7", "screenshot.png")));
        Assert.Equal("attachments/7/screenshot.png", mappings[0].RelativePath);
        Assert.True(mappings[0].IsImage);
    }

    [Fact]
    public void ProcessIssue_HandlesCollisionWithCounterPrefix()
    {
        CreateFakeAttachmentFile("guid1");
        CreateFakeAttachmentFile("guid2");
        var issue = new Issue { Id = 7 };
        var attachments = new List<Attachment>
        {
            new() { Issue = 7, Filename = "report.pdf", Path = "attachments/guid1" },
            new() { Issue = 7, Filename = "report.pdf", Path = "attachments/guid2" }
        };

        var mappings = _processor.ProcessIssue(issue, attachments, _inputDir, _outputDir);

        Assert.Equal(2, mappings.Count);
        Assert.True(File.Exists(Path.Combine(_outputDir, "attachments", "7", "report.pdf")));
        Assert.True(File.Exists(Path.Combine(_outputDir, "attachments", "7", "2-report.pdf")));
    }

    [Fact]
    public void ProcessIssue_SkipsOtherIssuesAttachments()
    {
        CreateFakeAttachmentFile("xyz");
        var issue = new Issue { Id = 7 };
        var attachments = new List<Attachment>
        {
            new() { Issue = 99, Filename = "other.png", Path = "attachments/xyz" }
        };

        var mappings = _processor.ProcessIssue(issue, attachments, _inputDir, _outputDir);

        Assert.Empty(mappings);
    }

    [Fact]
    public void ProcessIssue_NonImageAttachment_IsImageFalse()
    {
        CreateFakeAttachmentFile("docguid");
        var issue = new Issue { Id = 3 };
        var attachments = new List<Attachment>
        {
            new() { Issue = 3, Filename = "Book1.xlsx", Path = "attachments/docguid" }
        };

        var mappings = _processor.ProcessIssue(issue, attachments, _inputDir, _outputDir);

        Assert.Single(mappings);
        Assert.False(mappings[0].IsImage);
    }

    [Fact]
    public void ProcessIssue_ReturnsEmptyList_WhenNoAttachments()
    {
        var issue = new Issue { Id = 5 };
        var mappings = _processor.ProcessIssue(issue, [], _inputDir, _outputDir);
        Assert.Empty(mappings);
    }
}
