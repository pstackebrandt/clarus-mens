# Documentation Guide

> This document defines the file naming and formatting standards for project documentation to ensure
> consistency and maintainability across all documentation files.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Document Type Definitions](#document-type-definitions)
- [File Naming Conventions](#file-naming-conventions)
  - [Markdown Files](#markdown-files)
  - [Strategy Documents](#strategy-documents)
  - [Guide Documents](#guide-documents)
  - [Specification Documents](#specification-documents)
  - [Checklists](#checklists)
  - [File Extensions](#file-extensions)
- [Checklist Definition and Purpose](#checklist-definition-and-purpose)
- [Handling Checklist Content](#handling-checklist-content)
  - [Dedicated Checklist Files](#dedicated-checklist-files)
  - [Embedded Checklists](#embedded-checklists)
- [Benefits of Consistent Naming](#benefits-of-consistent-naming)

## Document Type Definitions

- **Strategy Document**: Outlines an approach or plan for implementing a feature or achieving a goal.
  Includes rationale, steps, considerations, and timeline.

- **Guide Document**: Provides instructional content with step-by-step procedures or best practices
  for performing specific tasks.

- **Specification Document**: Defines requirements, behaviors, and technical details for a feature
  or system. Describes what should be built rather than how.

- **Checklist Document**: Tracks task completion with status indicators for verifying processes.
  Focuses on verification rather than detailed explanation.

## File Naming Conventions

### Markdown Files

- Use **kebab-case** for all markdown files (lowercase with hyphens between words)
  - Example: `shell-usage-guidelines.md`
  - Example: `troubleshooting.md`

- **Exception**: Keep `README.md` in uppercase
  - This follows the universal convention that makes entry point documentation instantly recognizable

### Strategy Documents

- Name strategy documents with the pattern **"feature-strategy.md"**
  - Example: `api-versioning-strategy.md`
  - Example: `deployment-strategy.md`
- This makes strategy documents easily identifiable and consistent
- Always use feature-first naming (feature comes before "strategy" in the name)

### Guide Documents

- Name guide documents with the pattern **"topic-guide.md"**
  - Example: `deployment-guide.md`
  - Example: `testing-guide.md`
- Use when providing instructional content or best practices
- Always use topic-first naming (topic comes before "guide" in the name)

### Specification Documents

- Name specification documents with the pattern **"feature-spec.md"**
  - Example: `api-versioning-spec.md`
  - Example: `authentication-spec.md`
- Use for defining requirements and technical details
- Always use feature-first naming (feature comes before "spec" in the name)

### Checklists

- Name checklist files with **kebab-case** and the suffix **"-checklist.md"**
  - Example: `publishing-checklist.md`
  - Example: `deployment-checklist.md`
- This naming convention ensures proper application of checklist maintenance rules

### File Extensions

- Use `.md` for all Markdown files

## Checklist Definition and Purpose

A checklist is a document that tracks task completion using status indicators. Its primary features:

- Status markers (✅, - [ ], 🔄, ⚠️) tracking completion state
- Discrete, actionable tasks organized by category
- Focus on verification rather than detailed instructions

Checklists help ensure consistent, complete execution of complex processes while reducing errors.

## Handling Checklist Content

### Dedicated Checklist Files

- Create standalone checklist files for significant operational procedures
- Follow the naming convention with "-checklist.md" suffix
- Apply full checklist formatting rules

### Embedded Checklists

For documents that contain checklist sections but serve a broader purpose:

- Do not rename the main document to include "checklist"
- Add the following comment above the checklist section:

  ```markdown
  <!-- This section follows checklist maintenance rules -->
  ```

- Apply checklist formatting standards only to that section
- Consider extracting to a dedicated checklist file if the section grows large

## Benefits of Consistent Naming

- Improved readability and organization
- URL-friendly (no spaces or special characters)
- Consistent with modern web development practices
- Works reliably across different operating systems and file systems
