using Issue2Markdown.Models;
using Issue2Markdown.Services;

namespace Issue2Markdown.Tests;

public class MarkdownRendererCrossRefTests
{
    private static Issue MakeIssue(int id, string title = "Test", string? content = null) => new()
    {
        Id = id,
        Title = title,
        Content = content,
        CreatedOn = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        UpdatedOn = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
    };

    [Fact]
    public void Render_RewritesSelfReferenceLink_ToFragment()
    {
        var issues = new List<Issue> { MakeIssue(59, "Auto management") };
        var resolver = new IssueLinkResolver(issues);
        var renderer = new MarkdownRenderer(linkResolver: resolver);

        var issue = MakeIssue(59, "Auto management",
            "See [this post](https://bitbucket.org/level54solutions/chronos/issues/59/auto?iframe=true&spa=0#comment-69210913) for detail.");

        var md = renderer.Render(issue, [], [], [], []);

        Assert.Contains("[this post](#comment-69210913)", md);
        Assert.DoesNotContain("bitbucket.org/level54solutions", md);
    }

    [Fact]
    public void Render_RewritesCrossIssueLink_ToLocalFile()
    {
        var issues = new List<Issue>
        {
            MakeIssue(14, "Clockings"),
            MakeIssue(59, "Auto management")
        };
        var resolver = new IssueLinkResolver(issues);
        var renderer = new MarkdownRenderer(linkResolver: resolver);

        var issue = MakeIssue(59, "Auto management",
            "Related: [#14](https://bitbucket.org/level54solutions/chronos/issues/14/clockings)");

        var md = renderer.Render(issue, [], [], [], []);

        Assert.Contains("[#14](./14 - Clockings.md)", md);
    }

    [Fact]
    public void Render_EmitsCommentAnchor()
    {
        var renderer = new MarkdownRenderer();
        var issue = MakeIssue(1);
        var comments = new List<Comment>
        {
            new() { Id = 69210913, Issue = 1, Content = "hi",
                    User = new User { DisplayName = "Alice" },
                    CreatedOn = new DateTimeOffset(2020, 1, 2, 0, 0, 0, TimeSpan.Zero) }
        };

        var md = renderer.Render(issue, comments, [], [], []);

        Assert.Contains("<a id=\"comment-69210913\"></a>", md);
    }

    [Fact]
    public void Render_RewritesAttachmentDownloadLink_ToLocalPath()
    {
        var renderer = new MarkdownRenderer();
        var issue = MakeIssue(59, "Auto management",
            "See [log file attached](https://bitbucket.org/level54solutions/chronos/issues/attachments/59/level54solutions/chronos/1762299080.1332572/59/Chronos-20251104.log) for detail.");
        var mappings = new List<AttachmentMapping>
        {
            new("Chronos-20251104.log", "attachments/59/Chronos-20251104.log", IsImage: false)
        };

        var md = renderer.Render(issue, [], [], mappings, []);

        Assert.Contains("[log file attached](attachments/59/Chronos-20251104.log)", md);
        Assert.DoesNotContain("bitbucket.org/level54solutions/chronos/issues/attachments", md);
    }

    [Fact]
    public void Render_AttachmentLink_WithUrlEncodedSpaces_Resolves()
    {
        var renderer = new MarkdownRenderer();
        var issue = MakeIssue(14, "Clockings",
            "See [book](https://bitbucket.org/level54solutions/chronos/issues/attachments/14/foo/bar/Book%201.xlsx)");
        var mappings = new List<AttachmentMapping>
        {
            new("Book 1.xlsx", "attachments/14/Book 1.xlsx", IsImage: false)
        };

        var md = renderer.Render(issue, [], [], mappings, []);

        Assert.Contains("[book](attachments/14/Book 1.xlsx)", md);
    }

    [Fact]
    public void Render_RewritesNonImageLinkToCdnImage_WhenMappingExists()
    {
        var renderer = new MarkdownRenderer();
        var issue = MakeIssue(14, "Clockings",
            "[This is a quirk](https://bitbucket.org/repo/z8nqp5x/images/image.png)");
        var mappings = new List<AttachmentMapping>
        {
            new("image.png", "attachments/14/image.png", IsImage: true)
        };
        var attachments = new List<Attachment>
        {
            new() { Issue = 14, Filename = "image.png", Path = "attachments/abc" }
        };

        var md = renderer.Render(issue, [], [], mappings, attachments);

        Assert.Contains("[This is a quirk](attachments/14/image.png)", md);
    }

    [Fact]
    public void Render_WithoutResolver_LeavesIssueLinksAlone()
    {
        var renderer = new MarkdownRenderer();
        var issue = MakeIssue(59, "Auto management",
            "See [foo](https://bitbucket.org/level54solutions/chronos/issues/14/x)");

        var md = renderer.Render(issue, [], [], [], []);

        Assert.Contains("bitbucket.org/level54solutions", md);
    }
}
