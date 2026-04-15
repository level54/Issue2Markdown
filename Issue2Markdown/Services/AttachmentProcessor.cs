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

using Issue2Markdown.Models;

namespace Issue2Markdown.Services;

public class AttachmentProcessor
{
    public List<AttachmentMapping> ProcessIssue(
        Issue issue,
        IEnumerable<Attachment> allAttachments,
        string inputPath,
        string outputPath)
    {
        var issueAttachments = allAttachments.Where(a => a.Issue == issue.Id).ToList();
        if (issueAttachments.Count == 0)
            return [];

        var issueAttachDir = Path.Combine(outputPath, "attachments", issue.Id.ToString());
        Directory.CreateDirectory(issueAttachDir);

        var mappings = new List<AttachmentMapping>();
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var attachment in issueAttachments)
        {
            var guidName = Path.GetFileName(attachment.Path);
            var sourcePath = Path.Combine(inputPath, "attachments", guidName);

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"⚠ Attachment file not found: {sourcePath} (Issue #{issue.Id})");
                continue;
            }

            var filename = ResolveFilename(attachment.Filename, sourcePath);
            filename = ResolveCollision(filename, usedNames);
            usedNames.Add(filename);

            File.Copy(sourcePath, Path.Combine(issueAttachDir, filename), overwrite: true);

            var relativePath = $"attachments/{issue.Id}/{filename}";
            var isImage = FileTypeDetector.IsImage(Path.GetExtension(filename));
            mappings.Add(new AttachmentMapping(attachment.Filename, relativePath, isImage));
        }

        return mappings;
    }

    private static string ResolveFilename(string filename, string sourcePath)
    {
        if (!string.IsNullOrWhiteSpace(Path.GetExtension(filename)))
            return filename;

        var ext = FileTypeDetector.DetectExtension(sourcePath);
        return Path.GetFileNameWithoutExtension(filename) + ext;
    }

    private static string ResolveCollision(string filename, HashSet<string> used)
    {
        if (!used.Contains(filename))
            return filename;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
        var ext = Path.GetExtension(filename);
        var counter = 2;
        string candidate;
        do
        {
            candidate = $"{counter}-{nameWithoutExt}{ext}";
            counter++;
        } while (used.Contains(candidate));

        return candidate;
    }
}
