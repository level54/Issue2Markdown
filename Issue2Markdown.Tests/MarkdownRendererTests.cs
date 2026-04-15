using Issue2Markdown.Models;
using Issue2Markdown.Services;

namespace Issue2Markdown.Tests;

public class MarkdownRendererTests
{
    private readonly MarkdownRenderer _renderer = new();

    private static Issue MakeIssue(int id = 14, string title = "Test Issue") => new()
    {
        Id = id,
        Title = title,
        Status = "resolved",
        Priority = "major",
        Kind = "bug",
        Component = "General",
        Version = "2021.10.04.1013",
        Reporter = new User { DisplayName = "Ben Coetzee" },
        Assignee = new User { DisplayName = "Level54 Solutions" },
        CreatedOn = new DateTimeOffset(2019, 8, 6, 7, 24, 15, TimeSpan.Zero),
        UpdatedOn = new DateTimeOffset(2025, 8, 18, 21, 25, 51, TimeSpan.Zero),
        Content = "This is the issue description."
    };

    [Fact]
    public void Render_ContainsIssueHeader()
    {
        var md = _renderer.Render(MakeIssue(), [], [], [], []);
        Assert.Contains("# Issue #14: Test Issue", md);
    }

    [Fact]
    public void Render_ContainsMetadataTable()
    {
        var md = _renderer.Render(MakeIssue(), [], [], [], []);
        Assert.Contains("| Status | resolved |", md);
        Assert.Contains("| Priority | major |", md);
        Assert.Contains("| Reporter | Ben Coetzee |", md);
        Assert.Contains("| Assignee | Level54 Solutions |", md);
        Assert.Contains("| Created | 2019-08-06 07:24 |", md);
    }

    [Fact]
    public void Render_ContainsDescription()
    {
        var md = _renderer.Render(MakeIssue(), [], [], [], []);
        Assert.Contains("## Description", md);
        Assert.Contains("This is the issue description.", md);
    }

    [Fact]
    public void Render_OmitsDescriptionSection_WhenContentNull()
    {
        var issue = MakeIssue();
        issue.Content = null;
        var md = _renderer.Render(issue, [], [], [], []);
        Assert.DoesNotContain("## Description", md);
    }

    [Fact]
    public void Render_InlineImage_ForImageAttachment()
    {
        var mappings = new List<AttachmentMapping>
        {
            new("screenshot.png", "attachments/14/screenshot.png", IsImage: true)
        };
        var md = _renderer.Render(MakeIssue(), [], [], mappings, []);
        Assert.Contains("![screenshot.png](attachments/14/screenshot.png)", md);
    }

    [Fact]
    public void Render_LinkOnly_ForNonImageAttachment()
    {
        var mappings = new List<AttachmentMapping>
        {
            new("Book1.xlsx", "attachments/14/Book1.xlsx", IsImage: false)
        };
        var md = _renderer.Render(MakeIssue(), [], [], mappings, []);
        Assert.Contains("[Book1.xlsx](attachments/14/Book1.xlsx)", md);
        Assert.DoesNotContain("![Book1.xlsx]", md);
    }

    [Fact]
    public void Render_OmitsAttachmentsSection_WhenNone()
    {
        var md = _renderer.Render(MakeIssue(), [], [], [], []);
        Assert.DoesNotContain("## Attachments", md);
    }

    [Fact]
    public void Render_IncludesComments_SortedByDate()
    {
        var comments = new List<Comment>
        {
            new()
            {
                Issue = 14, Content = "Second comment",
                User = new User { DisplayName = "Alice" },
                CreatedOn = new DateTimeOffset(2019, 9, 1, 10, 0, 0, TimeSpan.Zero)
            },
            new()
            {
                Issue = 14, Content = "First comment",
                User = new User { DisplayName = "Bob" },
                CreatedOn = new DateTimeOffset(2019, 8, 10, 9, 0, 0, TimeSpan.Zero)
            }
        };
        var md = _renderer.Render(MakeIssue(), comments, [], [], []);
        Assert.Contains("### Bob — 2019-08-10 09:00", md);
        Assert.Contains("### Alice — 2019-09-01 10:00", md);
        var bobIdx = md.IndexOf("Bob", StringComparison.Ordinal);
        var aliceIdx = md.IndexOf("Alice", StringComparison.Ordinal);
        Assert.True(bobIdx < aliceIdx, "Bob's comment should appear before Alice's");
    }

    [Fact]
    public void Render_OmitsCommentsSection_WhenNone()
    {
        var md = _renderer.Render(MakeIssue(), [], [], [], []);
        Assert.DoesNotContain("## Comments", md);
    }

    [Fact]
    public void Render_IncludesChangeHistory()
    {
        var logs = new List<Log>
        {
            new()
            {
                Issue = 14, Field = "status", ChangedFrom = "new", ChangedTo = "closed",
                User = new User { DisplayName = "Level54 Solutions" },
                CreatedOn = new DateTimeOffset(2019, 3, 19, 9, 10, 39, TimeSpan.Zero)
            }
        };
        var md = _renderer.Render(MakeIssue(), [], logs, [], []);
        Assert.Contains("## Change History", md);
        Assert.Contains("| 2019-03-19 09:10 | Level54 Solutions | status | new | closed |", md);
    }

    [Fact]
    public void Render_OmitsChangeHistory_WhenNone()
    {
        var md = _renderer.Render(MakeIssue(), [], [], [], []);
        Assert.DoesNotContain("## Change History", md);
    }

    [Fact]
    public void Render_RewritesBitbucketImageUrl_ToLocalPath()
    {
        var issue = MakeIssue();
        issue.Content = "See ![](https://bytebucket.org/level54solutions/chronos/issues/attachments/screenshot.png)";
        var attachments = new List<Attachment>
        {
            new() { Issue = 14, Filename = "screenshot.png", Path = "attachments/abc123" }
        };
        var mappings = new List<AttachmentMapping>
        {
            new("screenshot.png", "attachments/14/screenshot.png", IsImage: true)
        };
        var md = _renderer.Render(issue, [], [], mappings, attachments);
        Assert.Contains("![](attachments/14/screenshot.png)", md);
        Assert.DoesNotContain("bytebucket.org", md);
    }

    [Fact]
    public void Render_LeavesUnmatchedBitbucketUrl_AndAddsWarning()
    {
        var issue = MakeIssue();
        issue.Content = "See ![](https://bytebucket.org/something/unknown.png)";
        var md = _renderer.Render(issue, [], [], [], []);
        Assert.Contains("bytebucket.org", md); // left unchanged
        Assert.Single(_renderer.Warnings);
        Assert.Contains("Could not localise", _renderer.Warnings[0]);
    }
}
