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

namespace Issue2Markdown.Helpers;

public static class FilenameHelper
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

    public static string Sanitise(string title)
    {
        var chars = title.Select(c => InvalidChars.Contains(c) ? ' ' : c).ToArray();
        var replaced = new string(chars);
        var collapsed = Regex.Replace(replaced, @"\s+", " ").Trim(' ', '.');

        return collapsed.Length > 200
            ? collapsed[..200].TrimEnd(' ', '.')
            : collapsed;
    }

    public static string BuildIssueFilename(int id, string title)
        => $"{id} - {Sanitise(title)}.md";
}
