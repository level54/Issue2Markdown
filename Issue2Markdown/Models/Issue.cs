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

// Issue2Markdown/Models/Issue.cs
using System.Text.Json.Serialization;

namespace Issue2Markdown.Models;

public class Issue
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("component")]
    public string? Component { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("milestone")]
    public string? Milestone { get; set; }

    [JsonPropertyName("reporter")]
    public User? Reporter { get; set; }

    [JsonPropertyName("assignee")]
    public User? Assignee { get; set; }

    [JsonPropertyName("created_on")]
    public DateTimeOffset? CreatedOn { get; set; }

    [JsonPropertyName("updated_on")]
    public DateTimeOffset? UpdatedOn { get; set; }

    [JsonPropertyName("edited_on")]
    public DateTimeOffset? EditedOn { get; set; }

    [JsonPropertyName("content_updated_on")]
    public DateTimeOffset? ContentUpdatedOn { get; set; }

    [JsonPropertyName("watchers")]
    public List<User> Watchers { get; set; } = new();

    [JsonPropertyName("voters")]
    public List<User> Voters { get; set; } = new();
}
