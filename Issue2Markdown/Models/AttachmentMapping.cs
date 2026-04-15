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

// Issue2Markdown/Models/AttachmentMapping.cs
namespace Issue2Markdown.Models;

/// <summary>
/// The result of processing one attachment file:
/// the original filename from the JSON, the relative path used in markdown links,
/// and whether the file is an image (to decide inline vs link rendering).
/// </summary>
public record AttachmentMapping(string OriginalFilename, string RelativePath, bool IsImage);
