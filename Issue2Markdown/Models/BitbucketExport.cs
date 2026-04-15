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

// Issue2Markdown/Models/BitbucketExport.cs
using System.Text.Json.Serialization;

namespace Issue2Markdown.Models;

public class BitbucketExport
{
    [JsonPropertyName("issues")]
    public List<Issue> Issues { get; set; } = new();

    [JsonPropertyName("comments")]
    public List<Comment> Comments { get; set; } = new();

    [JsonPropertyName("attachments")]
    public List<Attachment> Attachments { get; set; } = new();

    [JsonPropertyName("logs")]
    public List<Log> Logs { get; set; } = new();
}
