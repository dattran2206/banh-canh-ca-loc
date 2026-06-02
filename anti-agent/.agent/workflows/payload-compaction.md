---
description: Compresses payload tokens when passing Context between Agents
---

# Payload Compaction (Context Passing Optimization)

> **MANDATORY**: In Orchestration/Parallel mode, every Agent MUST adhere to this compaction protocol instead of passing the entire Chat Transcript to the next Agent.

## PRINCIPLE: NO RAW DATA TRANSFER
1. **DO NOT** copy and paste raw Terminal output.
2. **DO NOT** insert garbage lines like "I will now do X..." into the history.
3. **FORBIDDEN** to pass more than 1000 tokens (approx. 700 words) per transfer block.

## TRANSFER TEMPLATE (SYNTHESIS)

Whenever invoking another Agent via Antigravity, you MUST encapsulate your summary in the following structure:

```markdown
<SYNTHESIS>
**1. Current State:**
[Summarize what you have done in 2-3 sentences]

**2. Core Findings:**
- [Record only URLs / File Names / Function IDs that need attention]
- [Filtered real errors instead of tossing the raw Stacktrace]

**3. Action Request (For the next Agent):**
[Specific action command for the next Agent to perform]
</SYNTHESIS>
```

> **Immediate Action:** Read this tag carefully, distill your current information into a `<SYNTHESIS>` block, and then transfer it to the target Agent. Do not bloat the system's Token bandwidth!
