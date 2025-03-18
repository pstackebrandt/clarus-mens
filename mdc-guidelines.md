# MDC Files: Guidelines for Creation

## Table of Contents

- [Table of Contents](#table-of-contents)
- [What are MDC Files?](#what-are-mdc-files)
- [Core Purpose](#core-purpose)
- [Basic Structure](#basic-structure)
- [Rule Types](#rule-types)
- [Best Practices](#best-practices)
- [When to Create MDC Files](#when-to-create-mdc-files)

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

- **Manual**: Only applied when explicitly requested
- **Always**: Applied across all matching files automatically
- **Auto Attached**: Automatically attaches to relevant files based on content
- **Agent Requested**: Applied when the AI determines they're relevant

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
