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

using System.Text.Json;
using Issue2Markdown.Models;

namespace Issue2Markdown.Services;

public static class JsonLoader
{
    public static BitbucketExport Load(string inputPath)
    {
        var jsonPath = Path.Combine(inputPath, "db-2.0.json");

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"db-2.0.json not found in: {inputPath}", jsonPath);

        var json = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<BitbucketExport>(json, options)
               ?? throw new InvalidOperationException("Failed to deserialise db-2.0.json — result was null.");
    }
}
