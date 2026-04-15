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

using System.Text.RegularExpressions;
using Issue2Markdown.Helpers;
using Issue2Markdown.Models;

namespace Issue2Markdown.Services;

public class IssueLinkResolver
{
    private static readonly Regex IssueUrlRegex = new(
        @"^https?://(?:www\.)?bitbucket\.org/[^/]+/[^/]+/issues/(\d+)(?:/[^?#]*)?(?:\?[^#]*)?(?:#comment-(\d+))?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Dictionary<int, Issue> _issuesById;

    public IssueLinkResolver(IEnumerable<Issue> allIssues)
    {
        _issuesById = allIssues.ToDictionary(i => i.Id);
    }

    /// <summary>
    /// If <paramref name="url"/> is a Bitbucket issue URL, returns a local replacement:
    ///   - Self-reference with #comment-{id}:     "#comment-{id}"
    ///   - Self-reference without anchor:         null (leave as-is; we can't produce a useful link)
    ///   - Cross-reference:                       "./{filename}.md" or "./{filename}.md#comment-{id}"
    /// Returns null if the URL doesn't match the issue-URL pattern or the target issue is unknown.
    /// </summary>
    public string? Resolve(string url, int currentIssueId)
    {
        var match = IssueUrlRegex.Match(url);
        if (!match.Success) return null;

        if (!int.TryParse(match.Groups[1].Value, out var targetIssueId)) return null;
        if (!_issuesById.TryGetValue(targetIssueId, out var target)) return null;

        var commentAnchor = match.Groups[2].Success ? $"#comment-{match.Groups[2].Value}" : null;

        var filename = FilenameHelper.BuildIssueFilename(target.Id, target.Title);

        if (targetIssueId == currentIssueId)
        {
            // Self-reference. Fragment-only is preferable when we have one;
            // otherwise point to the file itself so the link is still usable.
            return commentAnchor ?? $"./{filename}";
        }

        return $"./{filename}{commentAnchor}";
    }
}
