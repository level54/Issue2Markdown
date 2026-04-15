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

// Issue2Markdown/Models/Comment.cs
using System.Text.Json.Serialization;

namespace Issue2Markdown.Models;

public class Comment
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("issue")]
    public int Issue { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("created_on")]
    public DateTimeOffset? CreatedOn { get; set; }

    [JsonPropertyName("updated_on")]
    public DateTimeOffset? UpdatedOn { get; set; }
}
