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

using System.IO.Compression;

namespace Issue2Markdown.Services;

public static class FileTypeDetector
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".svg" };

    public static string DetectExtension(string filePath)
    {
        var bytes = new byte[8];
        using var fs = File.OpenRead(filePath);
        var read = fs.Read(bytes, 0, 8);

        if (read >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return ".png";

        if (read >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return ".jpg";

        if (read >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
            return ".pdf";

        if (read >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B && bytes[2] == 0x03 && bytes[3] == 0x04)
            return DetectZipVariant(filePath);

        if (read >= 4 && bytes[0] == 0xD0 && bytes[1] == 0xCF && bytes[2] == 0x11 && bytes[3] == 0xE0)
            return ".msg";

        if (read >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return ".txt";

        if (IsPrintableText(bytes, read))
            return ".txt";

        return ".bin";
    }

    private static string DetectZipVariant(string filePath)
    {
        try
        {
            using var zip = ZipFile.OpenRead(filePath);
            if (zip.Entries.Any(e => e.FullName.StartsWith("xl/", StringComparison.OrdinalIgnoreCase)))
                return ".xlsx";
            if (zip.Entries.Any(e => e.FullName.StartsWith("word/", StringComparison.OrdinalIgnoreCase)))
                return ".docx";
            return ".zip";
        }
        catch
        {
            return ".zip";
        }
    }

    private static bool IsPrintableText(byte[] bytes, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var b = bytes[i];
            // Allow tab (0x09), LF (0x0A), CR (0x0D), and printable range (0x20+)
            if (b != 0x09 && b != 0x0A && b != 0x0D && b < 0x20)
                return false;
        }
        return count > 0;
    }

    public static bool IsImage(string extension)
        => ImageExtensions.Contains(extension);
}
