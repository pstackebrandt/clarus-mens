#!/bin/bash
# Install Markdown Lint Hook
# =========================
#
# Purpose:
# Installs a markdown linting pre-commit hook for Git.
# This is a simpler bash version that replaces a PowerShell implementation.
#
# Usage:
# ./scripts/install-markdown-lint-hook.sh

set -e  # Stop on any error

HOOK_DIR=".git/hooks"
SCRIPTS_DIR="$(dirname "$0")"
HOOK_SCRIPT="$SCRIPTS_DIR/lint-markdown.sh"
HOOK_TARGET="$HOOK_DIR/pre-commit"

# Ensure hooks directory exists
mkdir -p "$HOOK_DIR"

# Remove existing hook if present
if [ -f "$HOOK_TARGET" ]; then
    echo "🗑️ Removing existing pre-commit hook..."
    rm "$HOOK_TARGET"
fi

# Create the pre-commit hook
echo "#!/bin/bash
exec '$HOOK_SCRIPT' \"\$@\"" > "$HOOK_TARGET"

# Make both scripts executable
chmod +x "$HOOK_TARGET"
chmod +x "$HOOK_SCRIPT"

# Verify installation
if [ -x "$HOOK_TARGET" ] && [ -x "$HOOK_SCRIPT" ]; then
    echo "✅ Markdown lint hook installed successfully!"
    echo "   Pre-commit hook location: $HOOK_TARGET"
    echo "   Hook script location: $HOOK_SCRIPT"
else
    echo "Error: Failed to install markdown lint hook." >&2
    exit 1
fi

echo -e "\n📘 Usage:"
echo "The markdown lint hook will automatically run when you commit changes."
echo "To run the markdown linter manually:"
echo "  ./scripts/lint-markdown.sh" 