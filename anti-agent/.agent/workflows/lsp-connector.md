---
description: Automatically discovers target language and prepares the Language Server Protocol (LSP) for Compiler-level code reading.
---

# LSP Connector & Router

> **MANDATORY**: When first interacting with a project (or when the source code structure changes), the Agent must securely connect to the Language Server instead of purely relying on Text Grep.

## Step 1: Language Discovery
Scan the current Cwd directory to find core configuration files:
- If `package.json` and `tsconfig.json` exist → Project is `typescript` (or `javascript`)
- If `requirements.txt` or `pyproject.toml` exist → Project is `python`
- If `go.mod` exists → Project is `go`

## Step 2: Trigger Router Script
Depending on the report from Step 1, the Agent calls the following routing script via the Terminal tool (`run_command`).
```bash
python .agent/scripts/lsp_switcher.py --lang [typescript|python|go]
```
> *Note: This script will dynamically inject the `lsp-dev-server` alias attribute into the mcp_config.json configuration. It acts statically, only needing to be called once per project switch.*

## Step 3: Exploit LSP Power (Code Analysis)
Immediately after successfully connecting, instead of blindly searching for functions via Text, you **MUST USE the system's built-in MCP/LSP TOOLS** (if they are activated in your Tool tab):
- Use `get_diagnostics` to see where the Code currently flags red errors (e.g., TypeScript Type errors) in the IDE.
- Use `hover` to view the JSDoc/Docstring Comment of a hidden function.
- Use `find_references` to see specifically which files are calling an empty Interface.
*(Skip this step if MCP Tools do not appear in your tool list).*
