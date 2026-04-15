using Issue2Markdown.Helpers;

namespace Issue2Markdown.Tests;

public class FilenameHelperTests
{
    [Fact]
    public void Sanitise_RemovesWindowsInvalidChars()
    {
        var result = FilenameHelper.Sanitise("Fix: foo/bar\\baz");
        Assert.DoesNotContain(":", result);
        Assert.DoesNotContain("/", result);
        Assert.DoesNotContain("\\", result);
    }

    [Fact]
    public void Sanitise_CollapsesMultipleSpaces()
    {
        var result = FilenameHelper.Sanitise("foo   bar");
        Assert.Equal("foo bar", result);
    }

    [Fact]
    public void Sanitise_TrimsLeadingTrailingSpacesAndDots()
    {
        var result = FilenameHelper.Sanitise("  .foo bar.  ");
        Assert.Equal("foo bar", result);
    }

    [Fact]
    public void Sanitise_CapsAt200Chars()
    {
        var longTitle = new string('a', 250);
        var result = FilenameHelper.Sanitise(longTitle);
        Assert.Equal(200, result.Length);
    }

    [Fact]
    public void Sanitise_LeavesNormalTitleUnchanged()
    {
        var result = FilenameHelper.Sanitise("Clockings left behind in TimeLogEx");
        Assert.Equal("Clockings left behind in TimeLogEx", result);
    }

    [Fact]
    public void BuildIssueFilename_FormatsCorrectly()
    {
        var result = FilenameHelper.BuildIssueFilename(14, "Clockings left behind in TimeLogEx");
        Assert.Equal("14 - Clockings left behind in TimeLogEx.md", result);
    }

    [Fact]
    public void BuildIssueFilename_SanitisesTitleInFilename()
    {
        var result = FilenameHelper.BuildIssueFilename(5, "Fix: the bug");
        Assert.Equal("5 - Fix the bug.md", result);
    }
}
