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

namespace Issue2Markdown.Services;

public class CdnImageDownloader
{
    private readonly HttpClient _http;
    private readonly string _outputPath;

    public CdnImageDownloader(HttpClient http, string outputPath)
    {
        _http = http;
        _outputPath = outputPath;
    }

    /// <summary>
    /// Downloads a Bitbucket CDN image and stores it under attachments/{issueId}/.
    /// Returns the relative path (e.g. "attachments/59/image.png") on success, or null on failure.
    /// Mutates <paramref name="usedFilenames"/> by adding the chosen filename.
    /// </summary>
    public string? Download(string url, int issueId, HashSet<string> usedFilenames)
    {
        var filename = DeriveFilename(url);
        if (filename is null) return null;

        filename = ResolveCollision(filename, usedFilenames);

        var issueDir = Path.Combine(_outputPath, "attachments", issueId.ToString());
        Directory.CreateDirectory(issueDir);
        var target = Path.Combine(issueDir, filename);

        try
        {
            using var response = _http.GetAsync(url).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return null;
            var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            File.WriteAllBytes(target, bytes);
        }
        catch
        {
            return null;
        }

        usedFilenames.Add(filename);
        return $"attachments/{issueId}/{filename}";
    }

    private static string? DeriveFilename(string url)
    {
        string lastSegment;
        try
        {
            var uri = new Uri(url);
            lastSegment = Uri.UnescapeDataString(Path.GetFileName(uri.LocalPath));
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(lastSegment)) return null;

        // Strip Bitbucket's numeric prefix: "631718982-image.png" -> "image.png"
        var dashIdx = lastSegment.IndexOf('-');
        if (dashIdx > 0 && lastSegment[..dashIdx].All(char.IsDigit))
        {
            var tail = lastSegment[(dashIdx + 1)..];
            if (!string.IsNullOrWhiteSpace(tail)) lastSegment = tail;
        }

        return lastSegment;
    }

    private static string ResolveCollision(string filename, HashSet<string> used)
    {
        if (!used.Contains(filename)) return filename;

        var name = Path.GetFileNameWithoutExtension(filename);
        var ext = Path.GetExtension(filename);
        var counter = 2;
        string candidate;
        do
        {
            candidate = $"{counter}-{name}{ext}";
            counter++;
        } while (used.Contains(candidate));
        return candidate;
    }
}
