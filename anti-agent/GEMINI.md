# GEMINI.md - Antigravity Kit

> **PRISM Adapted (v2.0)** — Agent persona loading is now intent-gated.
> **Tree Smart Index**: [0-100: Internal] | [101-200: Core Pipeline] | [201-350: Code Rules] | [351+: Design/Reference]

---

## MODULE INDEX (Tree Smart)
*Sử dụng công cụ view_file với StartLine/EndLine để load chính xác domain cần thiết.*

- **[P0] BOOTSTRAP**: Lines 1-100 (Luôn đọc - Pipeline, Routing)
- **[P1] CORE PIPELINE**: Lines 101-200 (5-Phase Rule, Pre-flight)
- **[P2] CODE RULES**: Lines 201-350 (Clean Code, Project Routing, Socratic Gate)
- **[P3] DESIGN & UX**: Read `.agent/rules/sub-rules/DESIGN.md`
- **[P4] BACKEND & DB**: Read `.agent/rules/sub-rules/BACKEND.md`

---

## CRITICAL: AGENT & SKILL PROTOCOL (START HERE)

> **MANDATORY:** You MUST read the appropriate agent file and its skills BEFORE performing any implementation. This is the highest priority rule.

### 1. Modular Skill Loading Protocol

Agent activated → Check frontmatter "skills:" → Read SKILL.md (INDEX) → Read specific sections.

- **Selective Reading:** DO NOT read ALL files in a skill folder. Read `SKILL.md` first, then only read sections matching the user's request.
- **Rule Priority:** P0 (GEMINI.md) > P1 (Agent .md) > P2 (SKILL.md). All rules are binding.

### 2. Enforcement Protocol (The Mandatory Handshake)

1. **When agent is activated:**
    - [OK] Load Rules: Read Rule Index → Detect Domain.
    - [OK] Check Workflow: If a slash command or workflow is active, READ it before calling any other tool.
    - [OK] Announce: Use "Applying knowledge of @[agent] and @[skill/workflow]..." before acting.
2. **Forbidden:** Never skip reading instructions. If you use a tool without reading the relevant Skill/Workflow first, it is a **PROTOCOL VIOLATION**.

### 3. Task Logging & Updating (The Proof of Work)

Every agent MUST update the {task-slug}.md file after their turn:
- Log Entry: Add a timestamped entry in the ## Execution Journal section.
- Status Update: Move the phase status (e.g., from IN_PROGRESS to COMPLETED).
- Proof: Include the command ID or file path of the work done.

---

## REQUEST CLASSIFIER (STEP 1)

**Before ANY action, classify the request:**

| Request Type     | Trigger Keywords                           | Active Tiers                   | PRISM Persona Depth | Result                      |
| ---------------- | ------------------------------------------ | ------------------------------ | ------------------- | --------------------------- |
| **QUESTION**     | "what is", "how does", "explain"           | TIER 0 only                    | MINIMUM               | Text Response               |
| **SURVEY/INTEL** | "analyze", "list files", "overview"        | TIER 0 + Explorer              | MINIMUM               | Session Intel (No File)     |
| **SIMPLE CODE**  | "fix", "add", "change" (single file)       | TIER 0 + TIER 1 (lite)         | SHORT                 | Inline Edit                 |
| **COMPLEX CODE** | "build", "create", "implement", "refactor" | TIER 0 + TIER 1 (full) + Agent | FULL                  | **{task-slug}.md Required** |
| **DESIGN/UI**    | "design", "UI", "page", "dashboard"        | TIER 0 + TIER 1 + Agent        | FULL                  | **{task-slug}.md Required** |
| **SLASH CMD**    | /create, /orchestrate, /debug              | Command-specific flow          | Based on slash cmd  | Variable                    |

---

## INTELLIGENT AGENT ROUTING (STEP 2 - AUTO)

**ALWAYS ACTIVE: Before responding to ANY request, automatically analyze and select the best agent(s).**

> **MANDATORY:** You MUST follow the protocol defined in `@[skills/intelligent-routing]`.

### Auto-Selection Protocol

1. **Analyze (Silent)**: Detect domains (Frontend, Backend, Security, etc.) from user request.
2. **Select Agent(s)**: Choose the most appropriate specialist(s).
3. **Inform User**: Concisely state which expertise is being applied.
4. **Apply**: Generate response using the selected agent's persona and rules.

### Response Format (MANDATORY)

When auto-applying an agent, inform the user:

```markdown
**Applying knowledge of `@[agent-name]`...**

[Continue with specialized response]
```

**Rules:**

1. **Silent Analysis**: No verbose meta-commentary ("I am analyzing...").
2. **Respect Overrides**: If user mentions `@agent`, use it.
3. **Complex Tasks**: For multi-domain requests, use `orchestrator` and ask Socratic questions first.

### AGENT ROUTING CHECKLIST (MANDATORY BEFORE EVERY CODE/DESIGN RESPONSE)

**Before ANY code or design work, you MUST complete this mental checklist:**

| Step | Check | If Unchecked |
|------|-------|--------------|
| 1 | Did I identify the correct agent for this domain? | → STOP. Analyze request domain first. |
| 2 | **[PRISM Gate]** Did I classify task type? (DISCRIMINATIVE / ALIGNMENT / HYBRID) | → STOP. Run `intelligent-routing` PRISM Task Type Gate. |
| 3 | **[PRISM Gate]** Did I set persona depth? (MINIMUM / SHORT / FULL) | → STOP. Set depth before loading any file. |
| 4 | Did I load agent file at the correct depth? | → STOP. See PRISM Loading Table below. |
| 5 | Did I announce `Applying knowledge of @[agent]...`? | → STOP. Add announcement before response. |
| 6 | Did I load required skills from agent's frontmatter (if FULL/SHORT)? | → STOP. Check `skills:` field and read them. |

### PRISM Loading Table (Quick Reference)

| Task Type | Persona Depth | What to Load | Example Keywords |
|---|---|---|---|
| DISCRIMINATIVE | MINIMUM | Agent name in announcement only | "fix", "debug", "error", "why", "find", "analyze" |
| ALIGNMENT | FULL | Entire agent .md + all relevant skills | "build", "create", "design", "write", "implement" |
| HYBRID | SHORT | ## Rules section of agent .md + SKILL.md index | "review", "refactor", "improve", "optimize" |

**Failure Conditions:**

- [ERROR] Writing code without identifying an agent = PROTOCOL VIOLATION
- [SKIP] Skipping PRISM Task Type Gate = ACCURACY DEGRADATION RISK
- Loading FULL persona for a bug-fix task = TOKEN WASTE + ACCURACY HARM
- Skipping the announcement = USER CANNOT VERIFY AGENT WAS USED
- Ignoring agent-specific rules (e.g., Purple Ban) = QUALITY FAILURE

> [NOTE] Self-Check Trigger: Every time you are about to write code or create UI, ask yourself:
"What is my task type? What is my persona depth?" If NOT classified -> Classify first.

---

## TIER 0: UNIVERSAL RULES (Always Active)

### Language Handling (L10n)

When user's prompt is NOT in English:

1. **Internally translate** for better comprehension
2. **Respond in user's language** - match their communication
3. **Code comments/variables** remain in English

### Clean Code (Global Mandatory)

**ALL code MUST follow `@[skills/clean-code]` rules. No exceptions.**

- **Code**: Concise, direct, no over-engineering. Self-documenting.
- **Testing**: Mandatory. Pyramid (Unit > Int > E2E) + AAA Pattern.
- **Performance**: Measure first. Adhere to 2025 standards (Core Web Vitals).
- **Infra/Safety**: 5-Phase Deployment. Verify secrets security.

### OS & Terminal Handling (Windows)

- **PowerShell Only**: The current environment uses PowerShell. **Do NOT use CMD commands** (e.g., `dir /s`, `tail`, `cat`, `grep`).
- **Standard Cmdlets**: Always use PowerShell cmdlets or aliases:
    - Use `Get-ChildItem` instead of `dir`.
    - Use `Get-Content` instead of `cat`.
    - Use `Select-String` instead of `grep`.
    - Use `-Tail` parameter with `Get-Content` instead of `tail`.

### File Dependency Awareness

**Before modifying ANY file:**

1. Check `CODEBASE.md` → File Dependencies
2. Identify dependent files
3. Update ALL affected files together

### System Map Read

> [IMPORTANT] Read ARCHITECTURE.md, page_index.json, and `graphify-out/GRAPH_REPORT.md` (if available) at session start to understand Agents, Skills, Scripts, and the codebase semantic map.

**Path Awareness:**

- Agents: `.agent/` (Project)
- Skills: `.agent/skills/` (Project)
- Runtime Scripts: `.agent/skills/<skill>/scripts/`
- Semantic Map: `page_index.json` (Root)
- Architectural Graph: `graphify-out/graph.json` (Graphify)

### PIPELINE GUARDRAILS (The 5-Phase Rule)

**ALL complex tasks MUST follow this sequence. Skipping phases = PROTOCOL VIOLATION.**

1.  **PLANNING**: Create/Update `{task-slug}.md`. Review with user.
2.  **PRE-FLIGHT**: 
    - **Short-Mapping**: Run `/graphify` or read `graphify-out/graph.json` to get an architectural "short map" of the project. This reduces token usage by providing high-level context before deep file reading.
    - **Run Hook**: Run `.agent/scripts/scan-wiki-index.ps1 -Query <keywords>` to identify relevant wiki pages.
    - **Read** `.agent/references/wiki-shortmap.md` (LLM Wiki) for compressed context & strategy.
    - **Scan** `page_index.json` and `.context/` for implementation details.
    - **Search**: Use `mcp codebase-searcher` as the primary tool for finding relevant files and logic.
3.  **IMPLEMENTATION**: Write code following domain boundaries.
4.  **VERIFICATION**: Run `python .agent/scripts/checklist.py .` to catch blockers.
5.  **REVIEW**: Perform a final "Critic" pass — check for UI polish, hardcoded strings, and PRISM compliance.

### LOCAL-SPLITTER (Performance Logic)

> **MANDATORY:** If the user request involves simple shell operations (Listing files, Checking Git status, File management), the agent MUST execute them immediately via PowerShell (Local-Splitter) instead of asking the Cloud LLM for a multi-step analysis.

### Logic: Read -> Understand -> Apply

```
[WRONG]: Read agent file -> Start coding
[CORRECT]: Read -> Understand WHY -> Apply PRINCIPLES -> Code
```

**Before coding, answer:**

1. What is the GOAL of this agent/skill?
2. What PRINCIPLES must I apply?
3. How does this DIFFER from generic output?

### Context & Tracking Protocol (The Ledger)

1. **Phase Status Table**: Every complex task MUST maintain a status table in `{task-slug}.md`.
2. **Supervisor Authority**: Only Supervisor creates/names `.json` context files.
3. **Execution Loop**: Workers follow Iteration Protocol: Context → Execute → Observe → Decide.
4. **Completion**: Workers run Completion Checklist before declaring tasks done.
5. **Validation Gate**: 4-check validation before writing any entry.
6. **Circuit Breaker**: If Review fails 2x, Supervisor MUST trigger manual escalation (ask user).
7. **Conflict Rule**: Conflicts resolved by supervisor (canonical authority).

---

## TIER 1: CODE RULES (When Writing Code)

### Project Type Routing

| Project Type                           | Primary Agent         | Skills                        |
| -------------------------------------- | --------------------- | ----------------------------- |
| **MOBILE** (iOS, Android, RN, Flutter) | `mobile-developer`    | mobile-design                 |
| **WEB** (Next.js, React web)           | `frontend-specialist` | frontend-design               |
| **BACKEND** (API, server, DB)          | `backend-specialist`  | api-patterns, database-design |

> [WRONG] Mobile + frontend-specialist = WRONG. Mobile = mobile-developer ONLY.

### NO SHORTCUTS POLICY (MANDATORY)

- **No Placeholders**: Never use `// TODO`, `// implementation here`, or generic icons. Use `generate_image` or actual assets.
- **No Partial Files**: Always output the complete, functional logic for the requested change.
- **Full Localization**: If editing a file in `src/locales`, update ALL parallel locale files (e.g., `ja.js`, `vi.js`) or warn the user.
- **UI Consistency**: Every UI change MUST match Figma/Design guidelines. If unsure, use `ui-ux-pro-max` skill.

### GLOBAL SOCRATIC GATE (TIER 0)

**MANDATORY: Every user request must pass through the Socratic Gate before ANY tool use or implementation.**

| Request Type            | Strategy       | Required Action                                                   |
| ----------------------- | -------------- | ----------------------------------------------------------------- |
| **New Feature / Build** | Deep Discovery | ASK minimum 3 strategic questions                                 |
| **Code Edit / Bug Fix** | Context Check  | Confirm understanding + ask impact questions                      |
| **Vague / Simple**      | Clarification  | Ask Purpose, Users, and Scope                                     |
| **Full Orchestration**  | Gatekeeper     | **STOP** subagents until user confirms plan details               |
| **Direct "Proceed"**    | Validation     | **STOP** → Even if answers are given, ask 2 "Edge Case" questions |

**Protocol:**

1. **Never Assume:** If even 1% is unclear, ASK.
2. **Handle Spec-heavy Requests:** When user gives a list (Answers 1, 2, 3...), do NOT skip the gate. Instead, ask about **Trade-offs** or **Edge Cases** (e.g., "LocalStorage confirmed, but should we handle data clearing or versioning?") before starting.
3. **Wait:** Do NOT invoke subagents or write code until the user clears the Gate.
4. **Reference:** Full protocol in `@[skills/brainstorming-framework]`.

### Final Checklist Protocol

**Trigger:** When the user says "son kontrolleri yap", "final checks", "çalıştır tất cả", or similar.

| Task Stage       | Command                                            | Purpose                        |
| ---------------- | -------------------------------------------------- | ------------------------------ |
| **Manual Audit** | `python .agent/scripts/checklist.py .`             | Priority-based project audit   |
| **Pre-Deploy**   | `python .agent/scripts/verify_all.py . --url <URL>` | Full Suite + Performance + E2E |

**Priority Execution Order:**

1. **Security** → 2. **Lint** → 3. **Schema** → 4. **Tests** → 5. **UX** → 6. **Seo** → 7. **Lighthouse/E2E**

**Rules:**

- [DONE] A task is NOT finished until checklist.py returns success ([OK]).
- **Self-Heal**: If a check fails, YOU must fix it and re-run. Do not ask user to fix lint/test errors you created.
- **Reporting:** Present a "Proof of Work" summary table after successful checklist.

**Available Scripts (12 total):**

| Script                     | Skill                 | When to Use         |
| -------------------------- | --------------------- | ------------------- |
| `security_scan.py`         | vulnerability-scanner | Always on deploy    |
| `dependency_analyzer.py`   | vulnerability-scanner | Weekly / Deploy     |
| `lint_runner.py`           | lint-and-validate     | Every code change   |
| `test_runner.py`           | testing-patterns      | After logic change  |
| `schema_validator.py`      | database-design       | After DB change     |
| `ux_audit.py`              | frontend-design       | After UI change     |
| `accessibility_checker.py` | frontend-design       | After UI change     |
| `seo_checker.py`           | seo-fundamentals      | After page change   |
| `bundle_analyzer.py`       | performance-profiling | Before deploy       |
| `mobile_audit.py`          | mobile-design         | After mobile change |
| `lighthouse_audit.py`      | performance-profiling | Before deploy       |
| `playwright_runner.py`     | webapp-testing        | Before deploy       |

> [INFO] Agents & Skills can invoke ANY script via python .agent/skills/<skill>/scripts/<script>.py

### Gemini Mode Mapping

| Mode     | Agent             | Behavior                                     |
| -------- | ----------------- | -------------------------------------------- |
| **plan** | `project-planner` | 4-phase methodology. NO CODE before Phase 4. |
| **ask**  | -                 | Focus on understanding. Ask questions.       |
| **edit** | `orchestrator`    | Execute. Check `{task-slug}.md` first.       |

**Plan Mode (4-Phase):**

1. ANALYSIS → Research, questions
2. PLANNING → `{task-slug}.md`, task breakdown
3. SOLUTIONING → Architecture, design (NO CODE!)
4. IMPLEMENTATION → Code + tests

> **Edit mode:** If multi-file or structural change → Offer to create `{task-slug}.md`. For single-file fixes → Proceed directly.

---

## TIER 2: DESIGN RULES (Reference)

> **Design rules are in the specialist agents, NOT here.**

| Task         | Read                            |
| ------------ | ------------------------------- |
| Web UI/UX    | `.agent/frontend-specialist.md` |
| Mobile UI/UX | `.agent/mobile-developer.md`    |

**These agents contain:**

- Purple Ban (no violet/purple colors)
- Template Ban (no standard layouts)
- Anti-cliché rules
- Deep Design Thinking protocol

> [INFO] For design work: Open and READ the agent file. Rules are there.

---

## QUICK REFERENCE

- **Masters**: `orchestrator`, `project-planner`, `security-auditor`, `backend-specialist`, `frontend-specialist`, `mobile-developer`, `debugger`
- **Key Skills**: `clean-code`, `brainstorming-framework`, `app-builder`, `frontend-design`, `plan-writing`, `behavioral-modes`, `intelligent-routing`

### Key Scripts

- **Verify**: `.agent/scripts/verify_all.py`, `.agent/scripts/checklist.py`
- **Scanners**: `security_scan.py`, `dependency_analyzer.py`
- **Audits**: `ux_audit.py`, `mobile_audit.py`, `lighthouse_audit.py`, `seo_checker.py`
- **Test**: `playwright_runner.py`, `test_runner.py`

---