#!/bin/bash
# Markdown Linting Pre-commit Hook
# ================================
#
# Purpose:
# This script automatically lints and fixes markdown files that are staged for commit.
# It ensures consistent markdown formatting across the project by applying rules defined
# in .markdownlint.json before code is committed.
#
# Usage:
# 1. Manual run:
#    ./scripts/lint-markdown.sh
#
# 2. As Git pre-commit hook:
#    Installed automatically by install-markdown-lint-hook.sh
#
# Requirements:
# - Node.js and npm installed
# - Git Bash
# - markdownlint-cli2

set -e  # Stop on any error

# Function to convert Windows path to Unix path
win_to_unix_path() {
    echo "$1" | sed 's/\\/\//g' | sed 's/^\([A-Za-z]\):/\/\1/'
}

# Function to find Node.js executable
find_node() {
    # Try standard locations
    if command -v node &> /dev/null; then
        command -v node
        return
    fi

    # Try Windows npm location
    local npm_node="/c/Users/$USER/AppData/Roaming/npm/node.exe"
    if [ -f "$npm_node" ]; then
        echo "$npm_node"
        return
    fi

    # Try program files locations
    local prog_files_node="/c/Program Files/nodejs/node.exe"
    if [ -f "$prog_files_node" ]; then
        echo "$prog_files_node"
        return
    fi

    echo "Error: Could not find Node.js installation" >&2
    exit 1
}

echo "🔍 Checking for Node.js..."
NODE_EXE=$(find_node)
echo "Using Node.js at: $NODE_EXE"

echo "🔍 Checking for npm..."
if ! command -v npm &> /dev/null; then
    echo "Error: npm is not installed or not in PATH" >&2
    exit 1
fi

echo "🔍 Checking for markdownlint-cli2..."
if ! npm list -g markdownlint-cli2 &> /dev/null; then
    echo "📦 Installing markdownlint-cli2 globally..."
    npm install -g markdownlint-cli2
    
    if [ $? -ne 0 ]; then
        echo "Error: Failed to install markdownlint-cli2. Please install it manually: npm install -g markdownlint-cli2" >&2
        exit 1
    fi
fi

echo "🔎 Finding staged markdown files..."

# Get all staged markdown files
files=$(git diff --cached --name-only --diff-filter=ACM | grep '\.md$' || true)

if [ -z "$files" ]; then
    echo "No markdown files to check."
    exit 0
fi

# Run markdownlint on staged files
echo "$files" | while read -r file; do
    if [ -f "$file" ]; then
        echo "📝 Checking $file..."
        npx markdownlint-cli2 --fix "$file"
        git add "$file"  # Re-stage the file if it was modified
    fi
done 