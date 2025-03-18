# Markdown Guidelines and Quality Control

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Tools and Setup](#tools-and-setup)
  - [Required Extensions](#required-extensions)
  - [Automated Linting](#automated-linting)
- [Automated Processing](#automated-processing)
  - [Bulk Fix Command](#bulk-fix-command)
  - [GitHub Actions Integration](#github-actions-integration)
- [VS Code Configuration](#vs-code-configuration)
  - [Settings](#settings)
  - [Extension Recommendations](#extension-recommendations)
- [Measurements and Status](#measurements-and-status)
  - [Status Legend](#status-legend)
- [Common Pitfalls and Solutions](#common-pitfalls-and-solutions)
  - [Line Length (MD013)](#line-length-md013)

## Tools and Setup

### Required Extensions

- `DavidAnson.vscode-markdownlint`: Core linting functionality
- `yzhang.markdown-all-in-one`: Enhanced markdown support
- `streetsidesoftware.code-spell-checker`: Spell checking

### Automated Linting

The project includes automated markdown linting that:

- Checks and fixes common markdown formatting issues
- Runs automatically as a pre-commit hook
- Can be run manually for immediate feedback

To set up the pre-commit hook:

```bash
./scripts/install-markdown-lint-hook.sh
```

For manual linting:

```bash
./scripts/lint-markdown.sh
```

## Automated Processing

### Bulk Fix Command

```bash
# Fix all markdown files in the repository
find . -name "*.md" -type f -exec markdownlint-cli2 --fix {} \;
```

### GitHub Actions Integration

Create `.github/workflows/markdown-lint.yml`:

```yaml
name: Markdown Lint

on:
  pull_request:
    paths:
      - '**.md'
  push:
    paths:
      - '**.md'

jobs:
  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: DavidAnson/markdownlint-cli2-action@v15
        with:
          globs: "**/*.md"
          config: '.markdownlint.json'
```

## VS Code Configuration

### Settings

The `.vscode/settings.json` includes:

```json
{
    "markdownlint.config": {
        "extends": "${workspaceFolder}/.markdownlint.json"
    },
    "[markdown]": {
        "editor.defaultFormatter": "DavidAnson.vscode-markdownlint",
        "editor.formatOnSave": true,
        "editor.rulers": [120],
        "files.trimTrailingWhitespace": false
    }
}
```

### Extension Recommendations

Add to `.vscode/extensions.json`:

```json
{
    "recommendations": [
        "DavidAnson.vscode-markdownlint",
        "yzhang.markdown-all-in-one",
        "streetsidesoftware.code-spell-checker"
    ]
}
```

## Measurements and Status

| Measurement         | Status     | Description                             |
| ------------------- | ---------- | --------------------------------------- |
| Line Length         | ✅ Active   | Max 120 characters (except code/tables) |
| HTML Elements       | ✅ Active   | Only `<br>`, `<img>`, `<a>` allowed     |
| Header Duplicates   | ✅ Active   | Allowed within different sections       |
| Header Punctuation  | ✅ Active   | No punctuation except .,;:!             |
| Top Level Header    | ✅ Active   | Not required                            |
| Trailing Spaces     | ⚠️ Mixed    | Preserved in MD, trimmed elsewhere      |
| Table Formatting    | 🔄 Planned  | Consistent column alignment             |
| Code Block Language | 🔄 Planned  | Require language specification          |
| Link Validation     | 📝 Proposed | Check for broken internal links         |
| Image References    | 📝 Proposed | Verify image file existence             |
| Spell Checking      | 🔄 Planned  | US English dictionary                   |
| Frontmatter         | 📝 Proposed | Validate required metadata              |

### Status Legend

- ✅ Active: Currently enforced
- ⚠️ Mixed: Partially enforced
- 🔄 Planned: Implementation in progress
- 📝 Proposed: Under consideration

## Common Pitfalls and Solutions

### Line Length (MD013)

- Keep lines under 120 characters
- Break long lines at natural points:
  - After punctuation
  - Before conjunctions (and, or, but)
  - Between list items
- For links, use reference style:

  ```markdown
  Check the [pre-publishing checklist][pre-pub] first.
  
  [pre-pub]: PREPUBLISHING_CHECKLIST.md
  ```

- For long code blocks, use line breaks and proper indentation
- Tables can be formatted across multiple lines:

  ```markdown
  | Column A 
    | Column with a very long header 
    | Column C                         |
    | -------------------------------- |
    | -------------------------------- |
    | -------------------------------- |
    | Data                             |
    | This is a very long cell content |
    | Short                            |
  ```
