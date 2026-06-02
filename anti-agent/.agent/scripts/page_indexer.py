import os
import json
import argparse
import fnmatch
from typing import Dict, Any

# Exclude patterns to keep the index clean
EXCLUDE_PATTERNS = [
    "node_modules", ".git", ".next", "dist", "build", 
    ".agent", "page_index.json", "*.pyc", "__pycache__"
]

def should_exclude(path: str) -> bool:
    for pattern in EXCLUDE_PATTERNS:
        if fnmatch.fnmatch(path, f"*{pattern}*") or pattern in path.split(os.sep):
            return True
    return False

def build_directory_tree(root_dir: str) -> Dict[str, Any]:
    """Builds a hierarchical semantic tree of a codebase."""
    tree = {"title": os.path.basename(os.path.abspath(root_dir)), "nodes": []}
    
    for item in sorted(os.listdir(root_dir)):
        item_path = os.path.join(root_dir, item)
        if should_exclude(item_path):
            continue
            
        if os.path.isdir(item_path):
            child_tree = build_directory_tree(item_path)
            if child_tree["nodes"]:  # Only add non-empty directories
                tree["nodes"].append(child_tree)
        else:
            # For files, we add them as leaf nodes.
            # In a full AST implementation, we would extract classes/functions here.
            tree["nodes"].append({
                "title": item,
                "path": item_path,
                "type": "file"
            })
    return tree

def build_markdown_tree(file_path: str) -> Dict[str, Any]:
    """Parse a Markdown file and build a PageIndex from its headers."""
    tree = {"title": os.path.basename(file_path), "nodes": []}
    stack = [(0, tree)]  # (level, node_dict)
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
            
        for i, line in enumerate(lines):
            line = line.strip()
            if line.startswith("#"):
                header_level = len(line) - len(line.lstrip("#"))
                node_title = line.lstrip("#").strip()
                
                new_node = {
                    "title": node_title,
                    "start_line": i + 1,
                    "nodes": []
                }
                
                # Pop stack until we find the proper parent level
                while stack and stack[-1][0] >= header_level:
                    stack.pop()
                    
                # Append to current parent
                parent_node = stack[-1][1]
                parent_node["nodes"].append(new_node)
                
                # Push this node to stack
                stack.append((header_level, new_node))
                
    except Exception as e:
        print(f"Error reading {file_path}: {e}")
        
    return tree

def main():
    parser = argparse.ArgumentParser(description="PageIndex Generator for Vectorless RAG")
    parser.add_argument("target", help="Path to directory (codebase) or markdown file")
    parser.add_argument("--out", default="page_index.json", help="Output JSON path")
    
    args = parser.parse_args()
    target_path = os.path.abspath(args.target)
    
    if not os.path.exists(target_path):
        print(f"Error: Target path {target_path} does not exist.")
        return

    print(f"Generating PageIndex Semantic Tree for: {target_path}")
    
    if os.path.isfile(target_path) and target_path.endswith('.md'):
        index_tree = build_markdown_tree(target_path)
    elif os.path.isdir(target_path):
        index_tree = build_directory_tree(target_path)
    else:
        print("Error: Target must be a directory or a .md file.")
        return
        
    with open(args.out, 'w', encoding='utf-8') as f:
        json.dump(index_tree, f, indent=2)
        
    print(f"PageIndex successfully generated and saved to {args.out}")
    print("Agentic AI can now reason over this index instead of brute-forcing RAG!")

if __name__ == "__main__":
    main()
