using Issue2Markdown.Models;
using Issue2Markdown.Services;

namespace Issue2Markdown.Tests;

public class IssueLinkResolverTests
{
    private static List<Issue> SampleIssues() =>
    [
        new() { Id = 14, Title = "Clockings left behind" },
        new() { Id = 59, Title = "How To Use auto management of devices" }
    ];

    [Fact]
    public void Resolve_SelfReferenceWithComment_ReturnsFragmentOnly()
    {
        var resolver = new IssueLinkResolver(SampleIssues());
        var url = "https://bitbucket.org/level54solutions/chronos/issues/59/how-to-use-auto-management-of-devices?iframe=true&spa=0#comment-69210913";
        Assert.Equal("#comment-69210913", resolver.Resolve(url, currentIssueId: 59));
    }

    [Fact]
    public void Resolve_SelfReferenceWithoutComment_ReturnsLocalFilename()
    {
        var resolver = new IssueLinkResolver(SampleIssues());
        var url = "https://bitbucket.org/level54solutions/chronos/issues/59/how-to-use-auto-management-of-devices";
        Assert.Equal("./59 - How To Use auto management of devices.md", resolver.Resolve(url, currentIssueId: 59));
    }

    [Fact]
    public void Resolve_CrossReference_BuildsLocalFilenamePath()
    {
        var resolver = new IssueLinkResolver(SampleIssues());
        var url = "https://bitbucket.org/level54solutions/chronos/issues/14/clockings-left-behind";
        Assert.Equal("./14 - Clockings left behind.md", resolver.Resolve(url, currentIssueId: 59));
    }

    [Fact]
    public void Resolve_CrossReferenceWithComment_IncludesAnchor()
    {
        var resolver = new IssueLinkResolver(SampleIssues());
        var url = "https://bitbucket.org/level54solutions/chronos/issues/14/clockings-left-behind#comment-999";
        Assert.Equal("./14 - Clockings left behind.md#comment-999", resolver.Resolve(url, currentIssueId: 59));
    }

    [Fact]
    public void Resolve_UnknownTargetIssue_ReturnsNull()
    {
        var resolver = new IssueLinkResolver(SampleIssues());
        var url = "https://bitbucket.org/level54solutions/chronos/issues/9999/whatever";
        Assert.Null(resolver.Resolve(url, currentIssueId: 59));
    }

    [Fact]
    public void Resolve_NonIssueUrl_ReturnsNull()
    {
        var resolver = new IssueLinkResolver(SampleIssues());
        Assert.Null(resolver.Resolve("https://example.com/foo", currentIssueId: 59));
    }
}
