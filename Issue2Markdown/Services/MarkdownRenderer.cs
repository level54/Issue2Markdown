// Issue2Markdown/Models/Attachment.cs
/*
 * This file is part of Issue2Markdown.
 *
 * Issue2Markdown is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Issue2Markdown is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Issue2Markdown.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Text;
using System.Text.RegularExpressions;
using Issue2Markdown.Models;

namespace Issue2Markdown.Services;

public class MarkdownRenderer
{
    private static readonly Regex BitbucketImageRegex = new(
        @"!\[([^\]]*)\]\((https?://[^)]*(?:bitbucket\.org|bytebucket\.org)[^)]*)\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HtmlImgRegex = new(
        @"<img[^>]+src=""(https?://[^""]*(?:bitbucket\.org|bytebucket\.org)[^""]*)""\s*/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Markdown [text](bitbucket-image-url) — a NON-image link pointing at a CDN image (no leading `!`).
    private static readonly Regex BitbucketImageLinkRegex = new(
        @"(?<!\!)\[([^\]]*)\]\((https?://[^)]*(?:bitbucket\.org|bytebucket\.org)[^)]*\.(?:png|jpe?g|gif|webp|bmp|svg)[^)]*)\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Markdown [text](url) — but NOT image links (![...](..))
    private static readonly Regex IssueMarkdownLinkRegex = new(
        @"(?<!\!)\[([^\]]*)\]\((https?://(?:www\.)?bitbucket\.org/[^/]+/[^/]+/issues/\d+[^)]*)\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // HTML <a href="...">text</a>
    private static readonly Regex IssueHtmlLinkRegex = new(
        @"<a\b([^>]*?)\bhref=""(https?://(?:www\.)?bitbucket\.org/[^/]+/[^/]+/issues/\d+[^""]*)""([^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Bitbucket attachment download URLs — `.../issues/attachments/{issueId}/.../{filename}`
    private static readonly Regex AttachmentMarkdownLinkRegex = new(
        @"(?<!\!)\[([^\]]*)\]\(https?://(?:www\.)?bitbucket\.org/[^/]+/[^/]+/issues/attachments/(\d+)/[^)]*?/([^/)]+)\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex AttachmentHtmlLinkRegex = new(
        @"<a\b([^>]*?)\bhref=""https?://(?:www\.)?bitbucket\.org/[^/]+/[^/]+/issues/attachments/(\d+)/[^""]*?/([^/""]+)""([^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly CdnImageDownloader? _downloader;
    private readonly IssueLinkResolver? _linkResolver;
    private readonly List<string> _warnings = new();

    public IReadOnlyList<string> Warnings => _warnings;

    public MarkdownRenderer(CdnImageDownloader? downloader = null, IssueLinkResolver? linkResolver = null)
    {
        _downloader = downloader;
        _linkResolver = linkResolver;
    }

    public string Render(
        Issue issue,
        IEnumerable<Comment> comments,
        IEnumerable<Log> logs,
        IEnumerable<AttachmentMapping> attachmentMappings,
        IEnumerable<Attachment> allAttachments)
    {
        _warnings.Clear();
        var sb = new StringBuilder();
        var sortedComments = comments.OrderBy(c => c.CreatedOn).ToList();
        var sortedLogs = logs.OrderBy(l => l.CreatedOn).ToList();
        var mappingsList = attachmentMappings.ToList();
        var attachmentList = allAttachments.ToList();

        // Names already taken in attachments/{issueId}/ — used to avoid collisions when downloading CDN images.
        var usedFilenames = new HashSet<string>(
            mappingsList.Select(m => Path.GetFileName(m.RelativePath)),
            StringComparer.OrdinalIgnoreCase);

        // Header
        sb.AppendLine($"# Issue #{issue.Id}: {issue.Title}");
        sb.AppendLine();

        // Metadata table
        sb.AppendLine("| Field | Value |");
        sb.AppendLine("|---|---|");
        AppendRow(sb, "Status", issue.Status);
        AppendRow(sb, "Priority", issue.Priority);
        AppendRow(sb, "Kind", issue.Kind);
        AppendRow(sb, "Component", issue.Component);
        AppendRow(sb, "Version", issue.Version);
        AppendRow(sb, "Milestone", issue.Milestone);
        AppendRow(sb, "Reporter", issue.Reporter?.DisplayName);
        AppendRow(sb, "Assignee", issue.Assignee?.DisplayName);
        AppendRow(sb, "Created", FormatDate(issue.CreatedOn));
        AppendRow(sb, "Updated", FormatDate(issue.UpdatedOn));
        sb.AppendLine();

        // Description
        if (!string.IsNullOrWhiteSpace(issue.Content))
        {
            sb.AppendLine("## Description");
            sb.AppendLine();
            sb.AppendLine(RewriteUrls(issue.Content, issue.Id, attachmentList, mappingsList, usedFilenames));
            sb.AppendLine();
        }

        // Attachments
        if (mappingsList.Count > 0)
        {
            sb.AppendLine("## Attachments");
            sb.AppendLine();
            foreach (var mapping in mappingsList)
            {
                sb.AppendLine(mapping.IsImage
                    ? $"![{mapping.OriginalFilename}]({mapping.RelativePath})"
                    : $"[{mapping.OriginalFilename}]({mapping.RelativePath})");
            }
            sb.AppendLine();
        }

        // Comments
        if (sortedComments.Count > 0)
        {
            sb.AppendLine("## Comments");
            sb.AppendLine();
            foreach (var comment in sortedComments)
            {
                // Anchor so cross-issue URLs with #comment-{id} can resolve.
                sb.AppendLine($"<a id=\"comment-{comment.Id}\"></a>");
                sb.AppendLine($"### {comment.User?.DisplayName ?? "Unknown"} — {FormatDate(comment.CreatedOn)}");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(comment.Content))
                {
                    sb.AppendLine(RewriteUrls(comment.Content, issue.Id, attachmentList, mappingsList, usedFilenames));
                }
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }
        }

        // Change History
        if (sortedLogs.Count > 0)
        {
            sb.AppendLine("## Change History");
            sb.AppendLine();
            sb.AppendLine("| Date | User | Field | From | To |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var log in sortedLogs)
            {
                sb.AppendLine(
                    $"| {FormatDate(log.CreatedOn)} | {log.User?.DisplayName ?? ""} | {log.Field ?? ""} | {log.ChangedFrom ?? ""} | {log.ChangedTo ?? ""} |");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string RewriteUrls(
        string content, int issueId,
        List<Attachment> allAttachments,
        List<AttachmentMapping> mappings,
        HashSet<string> usedFilenames)
    {
        // Images (markdown)
        content = BitbucketImageRegex.Replace(content, match =>
        {
            var alt = match.Groups[1].Value;
            var url = match.Groups[2].Value;
            var local = ResolveImageUrl(url, issueId, allAttachments, mappings, usedFilenames);
            if (local != null) return $"![{alt}]({local})";
            _warnings.Add($"⚠ Could not localise image: {url} (Issue #{issueId})");
            return match.Value;
        });

        // Images (HTML)
        content = HtmlImgRegex.Replace(content, match =>
        {
            var url = match.Groups[1].Value;
            var local = ResolveImageUrl(url, issueId, allAttachments, mappings, usedFilenames);
            if (local != null) return $"<img src=\"{local}\" />";
            _warnings.Add($"⚠ Could not localise image: {url} (Issue #{issueId})");
            return match.Value;
        });

        // Non-image markdown links pointing at a CDN image — download and rewrite target.
        content = BitbucketImageLinkRegex.Replace(content, match =>
        {
            var text = match.Groups[1].Value;
            var url = match.Groups[2].Value;
            var local = ResolveImageUrl(url, issueId, allAttachments, mappings, usedFilenames);
            if (local != null) return $"[{text}]({local})";
            _warnings.Add($"⚠ Could not localise image: {url} (Issue #{issueId})");
            return match.Value;
        });

        // Bitbucket attachment-download links (markdown)
        content = AttachmentMarkdownLinkRegex.Replace(content, match =>
        {
            var text = match.Groups[1].Value;
            var linkIssueId = int.Parse(match.Groups[2].Value);
            var filename = Uri.UnescapeDataString(match.Groups[3].Value);
            var local = ResolveAttachmentLink(linkIssueId, filename, issueId, mappings);
            return local != null ? $"[{text}]({local})" : match.Value;
        });

        // Bitbucket attachment-download links (HTML)
        content = AttachmentHtmlLinkRegex.Replace(content, match =>
        {
            var before = match.Groups[1].Value;
            var linkIssueId = int.Parse(match.Groups[2].Value);
            var filename = Uri.UnescapeDataString(match.Groups[3].Value);
            var after = match.Groups[4].Value;
            var local = ResolveAttachmentLink(linkIssueId, filename, issueId, mappings);
            return local != null ? $"<a{before}href=\"{local}\"{after}>" : match.Value;
        });

        // Issue cross-references (markdown links)
        if (_linkResolver != null)
        {
            content = IssueMarkdownLinkRegex.Replace(content, match =>
            {
                var text = match.Groups[1].Value;
                var url = match.Groups[2].Value;
                var local = _linkResolver.Resolve(url, issueId);
                return local != null ? $"[{text}]({local})" : match.Value;
            });

            // Issue cross-references (HTML anchors) — keep attributes, swap href.
            content = IssueHtmlLinkRegex.Replace(content, match =>
            {
                var before = match.Groups[1].Value;
                var url = match.Groups[2].Value;
                var after = match.Groups[3].Value;
                var local = _linkResolver.Resolve(url, issueId);
                return local != null ? $"<a{before}href=\"{local}\"{after}>" : match.Value;
            });
        }

        return content;
    }

    private string? ResolveImageUrl(
        string url, int issueId,
        List<Attachment> allAttachments,
        List<AttachmentMapping> mappings,
        HashSet<string> usedFilenames)
    {
        // 1. Try to match an existing attachment by filename.
        var matched = TryMatchAttachment(url, issueId, allAttachments, mappings);
        if (matched != null) return matched;

        // 2. Fall back to downloading the CDN image, if a downloader is configured.
        if (_downloader != null)
        {
            var downloaded = _downloader.Download(url, issueId, usedFilenames);
            if (downloaded != null) return downloaded;
        }

        return null;
    }

    private static string? ResolveAttachmentLink(
        int linkIssueId, string filename,
        int currentIssueId,
        List<AttachmentMapping> mappings)
    {
        // We only have mappings for the current issue. Cross-issue attachment links aren't resolved here.
        if (linkIssueId != currentIssueId) return null;

        var mapping = mappings.FirstOrDefault(m =>
            string.Equals(m.OriginalFilename, filename, StringComparison.OrdinalIgnoreCase));

        return mapping?.RelativePath;
    }

    private static string? TryMatchAttachment(
        string url, int issueId,
        List<Attachment> allAttachments,
        List<AttachmentMapping> mappings)
    {
        string urlFilename;
        try { urlFilename = Path.GetFileName(new Uri(url).LocalPath); }
        catch { return null; }

        var attachment = allAttachments.FirstOrDefault(a =>
            a.Issue == issueId &&
            string.Equals(a.Filename, urlFilename, StringComparison.OrdinalIgnoreCase));

        if (attachment == null) return null;

        var mapping = mappings.FirstOrDefault(m =>
            string.Equals(m.OriginalFilename, attachment.Filename, StringComparison.OrdinalIgnoreCase));

        return mapping?.RelativePath;
    }

    private static void AppendRow(StringBuilder sb, string field, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            sb.AppendLine($"| {field} | {value} |");
    }

    private static string FormatDate(DateTimeOffset? dt)
        => dt?.ToString("yyyy-MM-dd HH:mm") ?? "";
}
