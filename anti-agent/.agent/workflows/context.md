---
description: Manage the Context Memory Layer (status, search, compact, reconcile)
---

# /context — Context Memory Management

This workflow is used to interact with and manage the Antigravity Kit's persistent Context Memory Layer (`.agent/.context/`).

## /context status

Displays the health and current state of the context memory system, and flags any configuration mismatches (simulating Pattern #16: Reconciliation).

```bash
# 1. Read index
Read .agent/.context/_index.json

# 2. Check filesystem
Scan .agent/.context/ directory

# 3. Reconcile
Check for:
- ORPHANED INDEX ENTRY: File in index but NOT on disk
- UNREGISTERED FILE: File on disk but NOT in index
- COUNT MISMATCH: entry_count in index ≠ actual entries in file

# 4. Check violations
Read violation_log from _index.json
```

## 🔍 /context search <keywords>

Runs an escalating search across the context memory store.

```bash
# Supervisor extracts tags from <keywords>
# Runs 5-Level Escalating Search (context-protocol SKILL §4)

Level 1: Exact tag match
Level 2: Partial tag match
Level 3: Category match
Level 4: Cross-category search
Level 5: Complete miss

# Supervisor returns matching entries with confidence level
```

## 🗜️ /context compact

Manually forces compaction on all eligible context files.

```bash
# 1. Scan all context files in .agent/.context/*.json
# 2. Identify files with ≥10 entries
# 3. For each eligible file:
   - Sort entries by reuse_count (desc) + recency (desc)
   - Keep top 7 in active file
   - Move bottom 3 to .agent/.context/_archive/{filename}.archive.json
# 4. Update _index.json entry_count for compacted files
```

## /context reconcile

Runs the reconciliation checks from `status` and automatically fixes the index to match reality.

```bash
# 1. Run reconciliation checks
# 2. Auto-fix:
   - Remove orphaned entries from _index.json
   - Add unregistered files to _index.json (requires user input for category)
   - Update incorrect entry counts in _index.json
# 3. Clear violation_log (optional)
```
