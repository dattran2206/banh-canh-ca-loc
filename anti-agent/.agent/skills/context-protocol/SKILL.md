---
name: context-protocol
description: Persistent context management layer. Lazy-loads past fixes, architecture decisions, and patterns from JSON files to eliminate repeated work and save tokens. Implements supervisor-worker authority model.
trigger: always_on
---

# Context Memory Skill

Persistent JSON-based context management for the Antigravity Kit agent system.
Implements 12 of 18 Agentic OS architectural patterns via instruction-based simulation.

---

## §1 — Supervisor Protocol: Index Scanning & Delegation

### When to Scan

The supervisor (orchestrator) MUST scan `_index.json` **before every task delegation**.

### Scanning Algorithm

```
1. Extract 3-5 keywords from user request
2. Use `mcp codebase-searcher` to find semantically related context files if index search is ambiguous.
3. Map keywords to likely categories in _index.json
4. Scan matching category → read entry titles
5. If title match found → load the context file
6. Attach relevant entries to worker delegation message
```

### File Creation Authority

**ONLY the supervisor** may:
- Create new context files
- Assign CTX-IDs (sequential: CTX-001, CTX-002, ...)
- Name files (format: `{category}-{specific-type}.json`)
- Assign files to categories in `_index.json`
- Register new categories

### Naming Convention

```
Format: {category}-{specific-type}.json

Prefixes for non-error types:
  arch-     → architecture decisions (e.g., arch-state-management.json)
  pattern-  → code patterns (e.g., pattern-compound-component.json)
  (none)    → error fixes (default, e.g., react-hydration.json)

Category examples: react, api, css, db, docker, next, node, auth, testing
```

### ID Assignment

```
File ID:  CTX-{NNN}           (e.g., CTX-001, CTX-042)
Entry ID: CTX-{NNN}-E{NN}     (e.g., CTX-001-E01, CTX-001-E07)
```

---

## §2 — Iteration Protocol (Simulates Agentic OS Pattern #1: Query Loop)

Every worker agent MUST follow this structured loop for each task:

```
┌─────────────────────────────────────────────┐
│  1. CONTEXT PHASE                           │
│     Check if supervisor provided context    │
│     entries. Read and understand them.      │
├─────────────────────────────────────────────┤
│  2. EXECUTE PHASE                           │
│     Solve the problem:                      │
│     - If context HIT: apply known solution  │
│     - If context MISS: solve from scratch   │
├─────────────────────────────────────────────┤
│  3. OBSERVE PHASE                           │
│     Validate the solution works:            │
│     - Run tests if applicable               │
│     - Check for side effects                │
│     - Verify the fix addresses root cause   │
├─────────────────────────────────────────────┤
│  4. DECIDE PHASE                            │
│     - Done? → Proceed to Completion Checklist│
│     - Not done? → Return to step 1 with     │
│       refined keywords                      │
│     - Error? → Escalate to supervisor        │
└─────────────────────────────────────────────┘
```

### Rules
- Each phase MUST complete before moving to the next
- If EXECUTE fails, do NOT skip OBSERVE — observe the failure
- If returning to step 1, use different/broader keywords
- Maximum 3 iterations before escalating to supervisor

---

## §3 — Completion Checklist (Simulates Pattern #2: State Machine)

Worker MUST NOT declare "done" until ALL checks pass:

```
PRE-COMPLETION CHECKLIST
========================
□ Context was searched? (HIT or MISS — must be logged)
□ Solution was validated? (tests pass, no regressions)
□ Context entry was proposed to supervisor?
□ If HIT: reuse_count was requested to increment?
□ If MISS: new entry has ALL required fields?
  - entry_id, type, timestamp, tags (2-5 items)
  - problem, root_cause, solution
  - files_affected, confidence (high|medium|low)
□ Entry proposal includes resolved_by field (own agent name)?

RESULT:
  ALL checks = YES → Task COMPLETE. Report to supervisor.
  ANY check = NO  → Task NOT COMPLETE. Continue working.
```

---

## §4 — Escalating Context Search (Simulates Pattern #3: Escalating Recovery)

5-level search protocol. Each level runs ONCE. If miss → escalate to next level.
Never retry the same level.

### Level 1: EXACT TAG MATCH
```
- Extract 3+ tags from problem description
- Search _index.json entries where title contains ALL extracted tags
- Confidence: HIGH
- Token cost: ~50 tokens (index scan only)
```

### Level 2: PARTIAL TAG MATCH
```
- Match entries containing ≥2 of extracted tags
- Confidence: MEDIUM
- Token cost: ~50 tokens
```

### Level 3: CATEGORY MATCH
```
- Identify problem category (react, api, css, db, docker...)
- Scan ALL entry titles within that category
- Look for semantic similarity (not exact keyword match)
- Confidence: LOW
- Token cost: ~100 tokens (read category entries)
```

### Level 4: CROSS-CATEGORY SEARCH
```
- Scan ALL categories in _index.json
- Match any entry with ≥1 overlapping tag
- Confidence: VERY LOW
- Token cost: ~200 tokens (full index scan)
```

### Level 5: COMPLETE MISS
```
- No matching context found anywhere
- Proceed to solve fresh (no prior context)
- After solving: propose new entry to supervisor
- Supervisor decides: create new file OR append to existing file
- Token cost: 0 (no context loaded)
```

### Search Result Reporting
```
When reporting search results to supervisor or in context entry:

SEARCH_RESULT: {
  level: 1-5,
  confidence: "high" | "medium" | "low" | "very_low" | "miss",
  matched_file: "react-hydration" | null,
  matched_entries: ["CTX-001-E03", "CTX-001-E07"] | [],
  tags_used: ["hydration", "localStorage", "SSR"]
}
```

---

## §5 — Compact & Archive Protocol (Simulates Pattern #9: Context Defense)

### 3-Layer Defense (Escalating Cost)

```
Layer 1: LAZY LOADING (cost: ~0)
  - Only load context files that match the current task
  - Never load all files at once
  - Supervisor scans index (~200 tokens) then loads 1 file (~500 tokens)

Layer 2: COMPACT (cost: low)
  - Trigger: file.entries.length >= 10
  - Supervisor ranks entries by reuse_count (desc) + recency (desc)
  - Keep TOP 7 entries in active file
  - Move BOTTOM 3 entries to _archive/{filename}.archive.json
  - Update _index.json entry_count

Layer 3: ARCHIVE DEEP SEARCH (cost: medium)
  - Only when Layer 1 search misses AND problem seems familiar
  - Search _archive/ directory for matching entries
  - If found: promote entry back to active file + increment reuse_count
  - Rarely needed — last resort before solving fresh
```

### Compact Execution Steps
```
1. Read file → count entries
2. If entries < 10 → skip (no action needed)
3. Sort entries: reuse_count DESC, then timestamp DESC
4. Split: top 7 = KEEP, bottom 3 = ARCHIVE
5. Read existing archive file (or create new)
6. Append archived entries to archive file
7. Rewrite active file with only top 7 entries
8. Update _index.json: entry_count = 7, last_touched = now
```

---

## §6 — Permission Matrix & Violation Logging (Simulates Pattern #10)

### Permission Matrix

| Role | Scan Index | Read Files | Write Entries | Create Files | Name Files | Compact | Canonical |
|------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Supervisor** | | ALL | ALL | SOLE | SOLE | SOLE | SOLE |
| **Workers** | via supervisor | assigned only | own entries | propose only | | | propose only |
| **Support** | | read-only | | | | | |

### Worker Write Flow
```
1. Worker solves issue
2. Worker creates entry proposal (JSON object)
3. Worker sends proposal to supervisor
4. Supervisor runs Validation Gate (§8)
5. If VALID:
   a. Does matching file exist? → Append to file
   b. No matching file? → Supervisor creates new file + registers in _index.json
   c. Conflict with existing entry? → Supervisor resolves (§7)
6. Entry written with validated_by: "supervisor"
7. If INVALID: Supervisor returns rejection with reason
```

### Violation Logging
```
When ANY agent attempts an action beyond its permission:

1. Action is BLOCKED
2. Violation logged to _index.json → violation_log[]
3. Entry format:
   {
     "agent": "frontend-specialist",
     "attempted_action": "create_new_file",
     "denied_reason": "Only supervisor creates files",
     "timestamp": "2026-04-03T20:00:00+07:00"
   }
4. Supervisor reviews violation_log during /context status
5. Repeated violations from same agent → supervisor adjusts delegation strategy
```

---

## §7 — Conflict Resolution Protocol (Simulates Pattern #8: Fork Isolation)

### When 2+ Workers Propose Entries for the SAME Context File

```
CASE A: Same problem + Same solution
  → MERGE into 1 entry
  → Credit both workers in resolved_by
  → reuse_count = sum of both

CASE B: Same problem + Different solution
  → Supervisor evaluates both solutions
  → Select CANONICAL solution (mark canonical: true)
  → Non-canonical solution stored in entry's "alternatives" array
  → Rationale for choice documented in entry

CASE C: Different problems
  → Append both entries normally
  → No conflict to resolve
```

### Simulated File Locking
```
When supervisor is performing compact or archive on a file:
  - Workers MUST NOT write to that file during the operation
  - Workers queue their proposals
  - Supervisor processes queued proposals after compact completes
  - Queue is implicit: supervisor handles proposals in order received
```

### Canonical Decision Protocol
```
When selecting canonical solution:

1. Check reuse_count (higher = more proven)
2. Check confidence level (high > medium > low)
3. Check recency (newer may account for codebase changes)
4. If still tied: supervisor uses domain expertise to decide
5. Document rationale in the entry

Canonical entries are prioritized in future context loading.
Non-canonical alternatives are preserved for edge cases.
```

---

## §8 — Validation Gate (Simulates Pattern #15: Security Sandbox)

Before writing ANY entry, supervisor MUST validate through 4 checks:

### Check 1: FORMAT CHECK
```
Required fields — ALL must be present:
  ✓ entry_id    (format: CTX-{NNN}-E{NN})
  ✓ type        (one of: error_fix | architecture_decision | pattern)
  ✓ timestamp   (ISO 8601 format)
  ✓ tags        (array, 2-5 items, no empty strings)
  ✓ problem     (non-empty string, ≥10 characters)
  ✓ solution    (non-empty string, ≥10 characters)
  ✓ confidence  (one of: high | medium | low)
  ✓ resolved_by (agent name that solved it)

Optional but recommended:
  - root_cause
  - files_affected
  - alternatives
```

### Check 2: DOMAIN CHECK
```
  - Does the proposing agent belong to the relevant domain group?
  - Frontend worker proposing backend entry → REJECT
  - Rejection action: reroute to correct domain worker
  
  Exception: cross-domain issues (e.g., API response format affecting UI)
  → Supervisor may accept with domain_hint noting both domains
```

### Check 3: CONTENT CHECK
```
  - Tags are specific enough?
    ✗ REJECT generic tags: "code", "error", "bug", "fix", "issue"  
    ✓ ACCEPT specific tags: "hydration", "useEffect", "JWT", "flexbox"
  
  - Solution is actionable?
    ✗ REJECT vague: "fix the bug", "update the code"
    ✓ ACCEPT specific: "Wrap in useEffect + useState(null) to defer render"
  
  - Confidence justification:
    "high"   → requires ≥2 successful applications OR explicit test verification
    "medium" → single successful application with reasonable confidence
    "low"    → theoretical solution, not yet fully verified
```

### Check 4: DUPLICATE CHECK
```
  - Compare new entry against existing entries in target file
  - Match criteria: ≥3 overlapping tags AND similar problem description
  - If DUPLICATE found:
    → Do NOT create new entry
    → Instead: MERGE (update existing entry's reuse_count + 1)
    → If solution differs: add to alternatives array
```

### Rejection Response Format
```json
{
  "status": "rejected",
  "check_failed": "FORMAT_CHECK | DOMAIN_CHECK | CONTENT_CHECK | DUPLICATE_CHECK",
  "details": "Missing required field: root_cause",
  "action": "Re-submit with complete fields"
}
```

---

## §9 — Context Entry Schema Reference

### Full Entry Schema
```json
{
  "entry_id": "CTX-001-E01",
  "type": "error_fix | architecture_decision | pattern",
  "timestamp": "2026-04-03T20:00:00+07:00",
  "tags": ["hydration", "localStorage", "next.js", "SSR"],
  "problem": "Hydration mismatch when using localStorage in initial render",
  "root_cause": "Server has no localStorage → HTML differs from client",
  "solution": "Wrap in useEffect + useState(null) → render after mount",
  "files_affected": ["components/ThemeProvider.tsx"],
  "resolved_by": "frontend-specialist",
  "validated_by": "supervisor",
  "canonical": true,
  "confidence": "high",
  "reuse_count": 3,
  "alternatives": [
    {
      "solution": "Use dynamic import with ssr: false",
      "resolved_by": "performance-optimizer",
      "reason_not_canonical": "Prevents SSR entirely, bad for SEO"
    }
  ]
}
```

### File Schema
```json
{
  "id": "CTX-001",
  "file": "react-hydration",
  "domain_hint": "frontend",
  "version": "1.0",
  "entries": []
}
```

### Index Entry Schema (in _index.json)
```json
{
  "id": "CTX-001",
  "file": "react-hydration",
  "title": "Hydration mismatch & SSR/CSR sync errors",
  "entry_count": 7,
  "last_touched": "2026-04-03T20:00:00+07:00"
}
```

---

## §10 — Session Protocol

### Session Start
```
1. Supervisor scans _index.json categories (lightweight, ~200 tokens)
2. Note which categories have been recently touched (last_touched)
3. Prime awareness: "These context areas are available for reference"
4. Do NOT load any context files yet (lazy loading)
```

### Session End
```
1. Review any new entries written during session
2. Verify _index.json is up-to-date (entry counts match reality)
3. Check if any files need compacting (≥10 entries)
4. If compaction needed: run compact protocol (§5)
```

### Mid-Session Context Refresh
```
When task domain shifts significantly mid-session:
1. Re-scan _index.json for new relevant categories
2. Load context for new domain
3. Previous domain context can be released (lazy unloading)
```
