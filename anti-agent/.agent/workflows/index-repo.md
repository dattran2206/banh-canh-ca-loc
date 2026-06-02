---
description: Trigger the PageIndex generator to build a semantic "Table of Contents" JSON tree for the current repository or a specific document. This enables Vectorless RAG reasoning for the supervisor.
---

# /index-repo — Architect Semantic Tree (PageIndex)

You are now executing the **PageIndex Vectorless RAG Workflow**. 
Unlike brute-force vector embedding, this command physically generates a structured `page_index.json` semantic tree. This hierarchy acts as the system's map.

## ZERO-HALLUCINATION MAPPING

> By generating an index, you guarantee that future agent operations (`explorer-agent`, `orchestrator`) will know exactly where files and logical boundaries exist. 
> **Short-Map Optimization:** Run `/graphify` before or alongside indexing to generate an architectural knowledge graph (`graphify-out/graph.json`). This provides high-level context that further reduces token usage during index traversal.

## GATE 1: IDENTIFY TARGET
*Operated by Orchestrator*

**Actions:**
1. Determine if the user is targeting:
   - A **Codebase** (e.g., `./` or `src/`)
   - An **External Document** (e.g., `docs/API_Reference.md`)
2. Verify the path exists using native tools.

## GATE 2: EXECUTE PAGE_INDEXER
*Operated by Orchestrator*

**Actions:**
1. Run the native python indexer script:
   ```bash
   python .agent/scripts/page_indexer.py <TARGET_PATH>
   ```
2. Monitor the output. The script will output either success or an error regarding AST/Markdown parsing.

## GATE 3: REGISTER KNOWLEDGE TREE
*Operated by Orchestrator*

**Actions:**
1. Confirm the creation of `page_index.json` in the root directory.
2. Output a brief summary to the user indicating how many primary nodes (folders/headers) were mapped.
3. Remind the user that all subsequent agent calls (via `/orchestrate` or `/edit`) will now drastically benefit in speed and accuracy by traversing this index first.

---

**Output Format:**
```markdown
# Vectorless PageIndex Generated Successfully
- **Target:** `<target>`
- **Output:** `page_index.json`
- **Engine:** AST/Markdown Semantic Mapper

> The AI Supervisor (`orchestrator`) is now locked onto this semantic map. Future codebase queries will bypass blind searching.
```
