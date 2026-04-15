# Issue2Markdown

Converts a Bitbucket issue tracker export into a folder of self-contained Markdown files. Each issue becomes one `.md` file. Attachments are renamed from their GUID-based storage names back to their original filenames and organised per issue. All links that originally pointed at Bitbucket are rewritten to point at the corresponding local file or anchor — the result is a fully offline-readable archive.

## Requirements

- .NET 10 SDK

## Usage

```
dotnet run --project Issue2Markdown -- --input <path-to-export> --output <path-to-output-folder>
```

| Parameter  | Description |
|------------|-------------|
| `--input`  | Path to the Bitbucket export directory. Must contain `db-2.0.json` and an `attachments/` subfolder. |
| `--output` | Path to the folder where Markdown files and attachments will be written. Created if it does not exist. |

### Example

```
dotnet run --project Issue2Markdown -- \
  --input  "C:\exports\project-issues" \
  --output "C:\exports\project-markdown"
```

### Expected console output

```
Loaded 64 issues, 134 attachments, 281 comments, 107 log entries.
[64/64] Writing issues...
Attachments copied: 134
Warnings: 0
Done. Markdown files written to: C:\exports\project-markdown
```

Any image URLs that could not be resolved to a local file are printed as warnings:

```
⚠ Could not localise image: https://bytebucket.org/... (Issue #42)
```

## Output structure

```
<output>/
├── 1 - First issue title.md
├── 2 - Second issue title.md
│   ...
└── attachments/
    ├── 1/
    │   ├── screenshot.png
    │   └── report.pdf
    ├── 2/
    │   └── data.xlsx
    ...
```

Each Markdown file is named `{id} - {sanitised title}.md`. Characters that are invalid in Windows filenames are stripped from the title.

## What each Markdown file contains

### Metadata table

Issue fields rendered as a Markdown table: status, priority, kind, component, version, milestone, reporter, assignee, created date, and last-updated date. Fields with no value are omitted.

### Description

The issue's description, as written in Bitbucket (already Markdown).

### Attachments

Each attachment belonging to the issue is listed. Image attachments are embedded inline (`![filename](attachments/{id}/filename)`). Non-image attachments are rendered as download links (`[filename](attachments/{id}/filename)`).

### Comments

Comments are sorted by date and rendered under `## Comments`. Each comment gets an HTML anchor (`<a id="comment-{id}"></a>`) immediately before its heading so that deep links from other issues resolve correctly.

### Change History

Activity log entries are rendered as a table with columns: date, user, field changed, previous value, new value.

Sections with no content are omitted entirely.

## URL rewriting

A core goal of the tool is that no link in the generated files points back at Bitbucket. The following rewrites are applied to every issue description and comment body:

| Original URL type | Rewritten to |
|---|---|
| Inline image (`![alt](bitbucket-url)`) | `![alt](attachments/{id}/filename)` — matched by filename against the issue's attachment list; downloaded from CDN if not already present |
| HTML `<img src="bitbucket-url">` | Same as above |
| Non-image link pointing at a CDN image file | `[text](attachments/{id}/filename)` |
| Bitbucket attachment download link (`[text](.../issues/attachments/...)`) | `[text](attachments/{id}/filename)` |
| Cross-issue link (`[#14](.../issues/14/...)`) | `[#14](./14 - Issue title.md)` |
| Cross-issue link with comment anchor (`...#comment-999`) | `[text](./14 - Issue title.md#comment-999)` |
| Self-referencing link with comment anchor | `[text](#comment-999)` (fragment only) |
| Self-referencing link without anchor | `[text](./59 - Self title.md)` |

The only exception is when a Bitbucket URL appears **as the visible link text** with a local target as the href (e.g. `[https://bitbucket.org/...](./local.md#comment-999)`). In that case the display text is left as-is because replacing it would remove information rather than adding it.

## Attachment processing

Source files in the export's `attachments/` folder are stored as extension-less GUID-named blobs. The tool:

1. Looks up the original filename from `db-2.0.json`.
2. Determines the file extension from the filename; falls back to magic-byte detection (PNG, JPEG, PDF, ZIP/XLSX, DOC/MSG, TXT, or `.bin` with a warning) if the filename has no extension.
3. Copies (never moves) the source file to `attachments/{issueId}/originalFilename.ext` in the output folder.
4. Handles filename collisions within the same issue folder by prepending a counter: `2-filename.ext`, `3-filename.ext`, etc.

CDN images that were pasted inline (not uploaded as formal attachments) are downloaded via HTTP and saved into the same `attachments/{issueId}/` folder, using the filename from the CDN URL with any leading numeric prefix stripped.

## Building and testing

```bash
# Build
dotnet build Issue2Markdown.slnx

# Run tests
dotnet test Issue2Markdown.slnx
```
