# MDC Files: Guidelines for Creation

## Table of Contents

- [Table of Contents](#table-of-contents)
- [What are MDC Files?](#what-are-mdc-files)
- [Core Purpose](#core-purpose)
- [Basic Structure](#basic-structure)
- [Rule Types](#rule-types)
- [How Rules Work](#how-rules-work)
  - [Stage 1: Injection](#stage-1-injection)
  - [Stage 2: Activation](#stage-2-activation)
  - [Rule Types and Properties](#rule-types-and-properties)
- [Best Practices](#best-practices)
- [When to Create MDC Files](#when-to-create-mdc-files)
- [References](#references)

## What are MDC Files?

MDC (Markdown Documentation Cursor) files are specialized documentation files used in Cursor AI to provide
context-aware coding guidelines and rules.
They help the AI understand project conventions, coding standards, and best practices when assisting you.

## Core Purpose

- Provide AI with structured knowledge about your codebase
- Enforce consistent coding standards and practices
- Create project-specific documentation that influences AI suggestions
- Improve code quality through consistent rule application

## Basic Structure

```mdc
---
description: Brief description of this rule set
globs: "*.js", "*.ts", "*.jsx"
alwaysApply: false
---

# Main Title

Content organized in sections...
```

## Rule Types

Cursor supports four rule types, each with a different activation behavior:

- **Manual**: Only applied when explicitly requested
- **Always**: Applied across all matching files automatically
- **Auto Attached**: Automatically attaches to relevant files based on content
- **Agent Requested**: Applied when the AI determines they're relevant

## How Rules Work

Rules in Cursor operate through a two-stage process:

### Stage 1: Injection

Rules are injected into the system prompt but aren't yet active. Injection depends on:

- **`alwaysApply`**: Controls unconditional injection into context
  - `true` - Always injected into every prompt
  - `false` - Only injected when relevant by other criteria

- **`globs`**: File pattern matching for context-based injection
  - Matches files based on patterns (filenames, extensions)
  - If a file matches, the rule is injected into context

### Stage 2: Activation

Whether an injected rule takes effect depends on:

- **`description`**: Determines the scenarios where the rule should be activated
  - The AI uses this to decide if the rule is relevant to the current task

### Rule Types and Properties

Here's how the frontmatter properties relate to rule types:

| Rule Type       | alwaysApply | Notes                                          |
| --------------- | ----------- | ---------------------------------------------- |
| Manual          | `false`     | Requires explicit request to activate          |
| Always          | `true`      | Automatically applied to all matching files    |
| Auto Attached   | `false`     | System decides based on file content relevance |
| Agent Requested | `false`     | AI decides based on context relevance          |

## Best Practices

1. **Be concise**: Keep rules clear and to the point
2. **Use proper formatting**: Utilize markdown headings, lists, and code blocks
3. **Target appropriate files**: Set precise glob patterns to apply rules only where needed
4. **Choose the right rule type**: Consider when you want your rules applied
5. **Organize logically**: Group related guidelines under clear headings
6. **Include examples**: Show correct and incorrect usage where helpful
7. **Keep updated**: Review and revise rules as your project evolves

## When to Create MDC Files

- For project-specific conventions
- When establishing team coding standards
- To document architectural decisions
- For special handling of framework-specific code
- To ensure consistency across your codebase

## References

- [A Deep Dive into Cursor Rules](https://forum.cursor.com/t/a-deep-dive-into-cursor-rules-0-45/60721) - Comprehensive
explanation of how Cursor rules work
