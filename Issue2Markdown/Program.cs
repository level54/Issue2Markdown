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

using System.Net.Http;
using Issue2Markdown.Helpers;
using Issue2Markdown.Models;
using Issue2Markdown.Services;

var inputPath = GetArg(args, "--input");
var outputPath = GetArg(args, "--output");

if (inputPath is null || outputPath is null)
{
    Console.WriteLine("Usage: Issue2Markdown --input <path-to-chronos-issues> --output <path-to-output-folder>");
    return 1;
}

if (!Directory.Exists(inputPath))
{
    Console.WriteLine($"Error: input directory not found: {inputPath}");
    return 1;
}

if (!File.Exists(Path.Combine(inputPath, "db-2.0.json")))
{
    Console.WriteLine($"Error: db-2.0.json not found in: {inputPath}");
    return 1;
}

Directory.CreateDirectory(outputPath);

BitbucketExport export;
try
{
    export = JsonLoader.Load(inputPath);
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading db-2.0.json: {ex.Message}");
    return 1;
}

Console.WriteLine(
    $"Loaded {export.Issues.Count} issues, {export.Attachments.Count} attachments, " +
    $"{export.Comments.Count} comments, {export.Logs.Count} log entries.");

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("Issue2Markdown/1.0");

var processor = new AttachmentProcessor();
var downloader = new CdnImageDownloader(http, outputPath);
var linkResolver = new IssueLinkResolver(export.Issues);
var renderer = new MarkdownRenderer(downloader, linkResolver);

int written = 0;
int attachmentsCopied = 0;
var allWarnings = new List<string>();

foreach (var issue in export.Issues.OrderBy(i => i.Id))
{
    var mappings = processor.ProcessIssue(issue, export.Attachments, inputPath, outputPath);
    attachmentsCopied += mappings.Count;

    var comments = export.Comments.Where(c => c.Issue == issue.Id);
    var logs = export.Logs.Where(l => l.Issue == issue.Id);

    var markdown = renderer.Render(issue, comments, logs, mappings, export.Attachments);
    allWarnings.AddRange(renderer.Warnings);

    var filename = FilenameHelper.BuildIssueFilename(issue.Id, issue.Title ?? $"Issue {issue.Id}");
    File.WriteAllText(Path.Combine(outputPath, filename), markdown, System.Text.Encoding.UTF8);

    written++;
    Console.Write($"\r[{written}/{export.Issues.Count}] Writing issues...");
}

Console.WriteLine();

foreach (var warning in allWarnings)
    Console.WriteLine(warning);

Console.WriteLine($"Attachments copied: {attachmentsCopied}");
Console.WriteLine($"Warnings: {allWarnings.Count}");
Console.WriteLine($"Done. Markdown files written to: {outputPath}");

return 0;

static string? GetArg(string[] args, string key)
{
    var idx = Array.IndexOf(args, key);
    return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
}
