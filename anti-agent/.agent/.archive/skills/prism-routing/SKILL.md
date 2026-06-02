---
name: prism-routing
description: PRISM-inspired intent-based persona routing. Classifies user requests into task types (DISCRIMINATIVE/ALIGNMENT/HYBRID) and determines the correct agent persona loading depth (MINIMUM/SHORT/FULL). Based on research: arXiv 2603.18507 — Hu, Rostami & Thomason, USC 2026.
version: 1.0.0
prism-paper: https://arxiv.org/abs/2603.18507
author: Antigravity Kit
---

# PRISM Routing Skill

> **Research foundation:** *"Expert Personas Improve LLM Alignment but Damage Accuracy"* — USC, 2026 [arXiv:2603.18507](https://arxiv.org/abs/2603.18507)

---

## Core Insight (Why This Skill Exists)

LLM expert personas have a **dual nature** — they are not universally helpful:

| Effect | On what tasks | Magnitude |
|---|---|---|
| **Improve** output | Alignment tasks (writing, style, format, safety, UX) | +0.65 MT-Bench score |
| **Damage** output | Discriminative tasks (logic, math, debugging, fact retrieval) | −3.6% accuracy, −0.65 coding |
| 📏 Longer persona | Amplifies BOTH effects | More damage → more gain depending on type |

**Conclusion:** There is NO single correct persona length. You must gate by task type.

---

## STAGE 1: TASK TYPE CLASSIFICATION

### The PRISM Gate (Run before EVERY agent activation)

```
Incoming User Request
        │
        ▼
  ┌─────────────────────────────────────────────┐
  │   PRISM DISCRIMINATOR                       │
  │                                             │
  │   Does the task require:                    │
  │   ├── Precise factual retrieval?   ──┐      │
  │   ├── Logical step-by-step trace?   ├─→ DISCRIMINATIVE
  │   ├── Root cause analysis?          │      │
  │   └── Calculation / exact answer? ──┘      │
  │                                             │
  │   Does the task require:                    │
  │   ├── Creating something new?      ──┐      │
  │   ├── Stylistic/format choices?     ├─→ ALIGNMENT
  │   ├── Safety-conscious writing?     │      │
  │   └── UX/tone/persona output?      ──┘      │
  │                                             │
  │   Is it BOTH above? ────────────────────→ HYBRID
  └─────────────────────────────────────────────┘
```

---

## TASK TYPE REFERENCE TABLE

### DISCRIMINATIVE — Persona: MINIMUM

> Load only `persona-minimum` from agent frontmatter (1 line).
> Reason: Long expert personas **damage** pretraining-acquired capabilities like logic and fact retrieval.

| Task Pattern | Example Requests | Correct Agent |
|---|---|---|
| Bug hunting | "Fix this error", "Why is X broken?" | `debugger` |
| Log/trace analysis | "What does this stack trace mean?" | `debugger` |
| Fact retrieval | "What does this API return?" | `backend-specialist` |
| Calculation | "Calculate the time complexity of..." | `backend-specialist` |
| Performance profiling | "Find the bottleneck in this code" | `performance-optimizer` |
| Security audit | "Scan this for vulnerabilities", "Find exploits" | `security-auditor` |
| Code archaeology | "Explain what this legacy function does" | `code-archaeologist` |
| Test analysis | "Why is this test failing?" | `test-engineer` |

**Signal keywords:** `fix`, `error`, `why`, `find`, `debug`, `trace`, `analyze`, `investigate`, `calculate`, `scan`, `audit`, `explain this`

---

### 🟢 ALIGNMENT — Persona: FULL

> Load the complete agent `.md` file + all relevant skills from frontmatter.
> Reason: Long expert personas significantly **improve** alignment-dependent output quality.

| Task Pattern | Example Requests | Correct Agent |
|---|---|---|
| UI/component creation | "Create a card component with dark mode" | `frontend-specialist` |
| Feature implementation | "Build a login page" | `frontend-specialist` |
| API design | "Design a REST API for user profiles" | `backend-specialist` |
| Writing/documentation | "Write a README for this project" | `documentation-writer` |
| Architecture design | "Design the database schema for..." | `database-architect` |
| Mobile UI creation | "Build a settings screen in React Native" | `mobile-developer` |
| Product definition | "Write user stories for the checkout flow" | `product-owner` |
| Security implementation | "Implement JWT auth with refresh tokens" | `backend-specialist` + `security-auditor` |

**Signal keywords:** `create`, `build`, `design`, `implement`, `write`, `make`, `generate`, `add a new`, `develop`

---

### 🟡 HYBRID — Persona: SHORT

> Load only the `## Rules` / `## Core Rules` section of agent `.md`.
> Load SKILL.md index only, not full skills.
> Reason: Needs alignment for quality output, needs accuracy for correct analysis.

| Task Pattern | Example Requests | Correct Agent |
|---|---|---|
| Code review | "Review this component for best practices" | `frontend-specialist` |
| Refactoring | "Refactor this to be more maintainable" | `frontend-specialist` |
| Optimization | "Optimize this database query" | `database-architect` |
| Architecture review | "Is this design pattern appropriate?" | `backend-specialist` |
| Test generation | "Write tests for this function" | `test-engineer` |
| Deployment review | "Review my CI/CD pipeline" | `devops-engineer` |
| Security review | "Review this auth flow for security issues" | `security-auditor` |

**Signal keywords:** `review`, `refactor`, `improve`, `optimize`, `suggest`, `check`, `is this correct`, `better way`, `best practice`

---

## STAGE 2: PERSONA DEPTH MAPPING

### Loading Protocol per Depth

```markdown
## MINIMUM Load (DISCRIMINATIVE)

1. Announce: "Applying knowledge of @[agent-name]..."
2. Load: persona-minimum value from agent frontmatter ONLY
3. Do NOT read the agent .md file body
4. Do NOT load any additional skills
5. Proceed directly with response

Estimated tokens: ~20–50 tokens

---

## 🟡 SHORT Load (HYBRID)

1. Announce: "Applying knowledge of @[agent-name]..."
2. Load: persona-short value from agent frontmatter
3. Read ONLY the `## Rules` or `## Core Rules` section of agent .md
4. Read SKILL.md index (first 50 lines only) for relevant skills
5. Do NOT load full skills unless a specific section is critical

Estimated tokens: ~300–600 tokens

---

## 🟢 FULL Load (ALIGNMENT)

1. Announce: "Applying knowledge of @[agent-name]..."
2. Read the ENTIRE agent .md file
3. Check `skills:` in frontmatter
4. Load ALL relevant skill SKILL.md files fully
5. Apply all rules, design principles, checklists
6. This is the standard "deep expert mode"

Estimated tokens: 1,000–8,000 tokens (scales with agent size)
```

---

## STAGE 3: EDGE CASES & OVERRIDE RULES

### Edge Case 1: Ambiguous Request

```
User: "Make this better"
→ Task Type: UNCLEAR
→ Action: DO NOT guess. Ask 1 clarifying question:
  "Do you want me to: (A) find bugs/issues [DISCRIMINATIVE], 
   (B) redesign/rewrite it [ALIGNMENT], or 
   (C) refactor while preserving logic [HYBRID]?"
```

### Edge Case 2: Mixed Request

```
User: "Fix the bug AND make the UI prettier"
→ Contains BOTH discriminative (fix bug) and alignment (UI prettier)
→ Split into 2 sub-tasks:
  Sub-task 1: DISCRIMINATIVE → debugger (MINIMUM persona)
  Sub-task 2: ALIGNMENT → frontend-specialist (FULL persona)
→ Route to orchestrator for sequential execution
```

### Edge Case 3: Reasoning Model (Claude Sonnet Thinking, etc.)

```
Per PRISM §3.3c: Reasoning-distilled models resist persona distillation.
  - The model's chain-of-thought DECREASES persona sensitivity
  - Expert persona provides only marginal benefit over no persona
→ For reasoning models: prefer MINIMUM or SHORT regardless of task type
→ Trust the model's built-in reasoning over persona steering
```

### Override Rule: Explicit User Agent Mention

```
User: "@backend-specialist review this"
→ OVERRIDE: User explicitly requested HYBRID via "review" keyword
→ Load backend-specialist at SHORT depth
→ User's explicit mention > PRISM gate classification
```

---

## STAGE 4: SELF-VERIFICATION (PRISM §3 Adapted)

After generating a response with expert persona active, run this internal check:

```markdown
### PRISM Self-Verify Checklist

Before submitting response:

[ ] Q1: Did the persona add concrete improvements in style/format/tone
        beyond what a base response would produce?
    → YES: Count as alignment gain ✅
    → NO:  The persona may have been unnecessary ⚠️

[ ] Q2: Did I maintain factual accuracy throughout?
    → Check: No hallucinated APIs, methods, or library names
    → If DISCRIMINATIVE task: did I stay evidence-based?

[ ] Q3: Is the response length appropriate for the task type?
    → DISCRIMINATIVE: Direct, concise, factual
    → ALIGNMENT: Rich, detailed, stylistically informed
    → HYBRID: Balanced — analytical but quality-aware

[ ] Q4: Did I apply any forbidden patterns despite knowing they're banned?
    (Purple, Bento grids, generic layouts for frontend tasks, etc.)

RESULT:
  3-4 → Submit response
  2   → Adjust and resubmit
  0-1 → DO NOT submit. Restart with correct persona depth.
```

---

## QUICK DECISION MATRIX

> Use this as a fast lookup when you need a quick answer:

| Request starts with... | Task Type | Persona Depth | Load What |
|---|---|---|---|
| "Fix", "Debug", "Why is", "Error:", "Find the bug" | DISC | MIN | Announcement only |
| "What is", "How does", "Explain" | DISC | MIN | Announcement only |
| "Analyze", "Audit", "Scan", "Check for" | DISC | MIN | Announcement only |
| "Refactor", "Improve", "Review", "Optimize" | HYBRID | 🟡 SHORT | Rules section |
| "Add tests for", "Write tests", "Generate tests" | HYBRID | 🟡 SHORT | Rules section |
| "Create", "Build", "Design", "Make", "Implement" | ALIGN | 🟢 FULL | Full agent + skills |
| "Write a", "Generate a", "Add a new" | ALIGN | 🟢 FULL | Full agent + skills |
| Unclear / mixed | UNCLEAR | → ASK | Clarify first |

---

## 🔗 Integration Points

This skill is referenced in:

- `rules/GEMINI.md` → Agent Routing Checklist (Steps 2 & 3)
- `skills/intelligent-routing/SKILL.md` → PRISM Task Type Gate (Section 2)
- `workflows/fe-pro.md` → Gate 2.5 PRISM Self-Verify
- `workflows/be-pro.md` → (planned) Gate 2.5 PRISM Self-Verify

**Invoke this skill when:**
- You are uncertain about how deeply to load an agent
- The request type is ambiguous
- You need to defend your routing decision to the user
- You are building a new workflow and need to embed PRISM logic

---

## 📚 References

| # | Source | URL |
|---|---|---|
| [1] | PRISM Paper — Abstract | [arxiv.org/abs/2603.18507](https://arxiv.org/abs/2603.18507) |
| [2] | PRISM Paper — Full HTML | [arxiv.org/html/2603.18507v1](https://arxiv.org/html/2603.18507v1) |
| [3] | Key Finding §3.1 | Persona damages discriminative accuracy: −3.6% MMLU, −0.65 Coding |
| [4] | Key Finding §3.2 | Persona boosts alignment: +17.7% safety (JailbreakBench), +0.65 Extraction |
| [5] | Key Finding §5.2 | Binary routing surpasses expert prompting: Qwen 73.5 vs 72.2 expert vs 71.8 base |
