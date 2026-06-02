import sys
import argparse
import json
import os
from pathlib import Path

def update_mcp_config(language: str):
    config_path = Path(__file__).parent.parent / "mcp_config.json"
    
    if not config_path.exists():
        print(f"Error: Could not find {config_path}")
        return False
        
    try:
        with open(config_path, "r", encoding="utf-8") as f:
            content = f.read()
            # strip inline comments which might break standard json parsing 
            # (or we just use basic json with ignore)
            import re
            content = re.sub(r'//.*', '', content)
            config = json.loads(content)
    except Exception as e:
        print(f"Could not parse mcp_config.json: {e}")
        return False

    # Language mapping for MCP Servers
    mcp_servers = {
        "typescript": {
            "command": "npx",
            "args": ["-y", "@modelcontextprotocol/server-typescript", "--api-key", "local"]
        },
        "python": {
            "command": "uvx",
            "args": ["mcp-server-python"]
        },
        "go": {
            "command": "npx",
            "args": ["-y", "@modelcontextprotocol/server-go"] # Conceptual mapping
        }
    }
    
    # Generic mapping layer -> map to 'lsp-dev-server'
    if language.lower() in mcp_servers:
        if "mcpServers" not in config:
            config["mcpServers"] = {}
            
        config["mcpServers"]["lsp-dev-server"] = mcp_servers[language.lower()]
        
        with open(config_path, "w", encoding="utf-8") as f:
            json.dump(config, f, indent=4)
        print(f"Successfully mapped [lsp-dev-server] to Language Protocol: {language.upper()}")
        return True
    else:
        print(f"Language {language} not currently templated. Fallback to default grep.")
        return False

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Switch Target Language for Agent MCP/LSP")
    parser.add_argument("--lang", type=str, required=True, help="Target language (e.g. typescript, python, go)")
    args = parser.parse_args()
    
    update_mcp_config(args.lang)
