---
description: Deep Execution Pipeline for Frontend Development (Vibecode Protocol)
---

# /fe-pro — Frontend Execution Engine

You are now activating the **Frontend Execution Pipeline (FE-PRO)**. This is a rigorous, 3-gate pipeline designed to guarantee zero-hallucination UI development with REQ-ID traceability, Context Memory integration, and Double-Evidence reporting.

This workflow is strictly managed by the **Supervisor (Orchestrator)**. 

---

## STRICT RULES OF ENGAGEMENT
1. **Sequential Mandate:** If this task requires Backend APIs, the BE-PRO pipeline MUST execute fully first to generate the API Contract. Do not mock APIs unless explicitly requested.
2. **Context First:** Supervisor MUST query `.agent/.context/_index.json` before any code is generated.
3. **Double-Evidence:** Gate 3 requires BOTH script validation AND a Browser MCP screenshot.

---

## GATE 1: BLUEPRINT & TRACEABILITY
*Supervisor operates alone here.*

**Actions:**
1. **Short-Map Discovery**: Run `/graphify` or read `graphify-out/graph.json` to understand the UI architecture and component dependencies.
2. **Semantic Search**: Use `mcp codebase-searcher` to identify all relevant components, hooks, and styles.
3. Break down the user UI request into logical frontend features.
4. Assign each feature a strict Requirement ID (e.g., `REQ-FE-01: Navigation Bar`).
5. Define the Component Tree map (Which parent hosts which children).
6. Identify required Tools (e.g., `Browser MCP` for visual checks).

**Gate Checkpoint:** 
- The Supervisor outputs the `FE Blueprint & Tool Manifest`.
- **STOP AND WAIT.** User must reply with "APPROVED" before moving to Gate 2.
- *If User provides feedback, revise Blueprint and loop.*

---

## GATE 2: CORE LOGIC EXECUTION
*Supervisor summons `frontend-specialist`.*

**Actions:**
1. **Context Injection:** Supervisor feeds the relevant Context Memory files (based on Bug Types / UI Patterns) to the Sub-agent.
2. **Code Generation:** `frontend-specialist` implements code strictly adhering to the `REQ-ID` mapped in Gate 1.
3. **Skill Enforcement:** Must strictly follow `react-best-practices` (no unnecessary re-renders) and `frontend-design` rules (e.g., Purple Ban, Layout Diversification).

**Mutex Lock Rule:**
- The Orchestrator locks the UI files being edited. No other agent may write to these files until Gate 2 completes.

---

## GATE 2.5: PRISM SELF-VERIFY (NEW — Inspired by PRISM §3 Self-Verification)
*frontend-specialist performs internal self-assessment before handoff to QA.*

> **Why this gate exists:** Per PRISM research (arXiv 2603.18507), expert personas improve alignment tasks (style, format, safety) but can damage accuracy on logic/discriminative tasks. This gate catches persona-induced degradation before it reaches production.

**The Agent Must Internally Answer:**

| Question | PASS | FAIL → Action |
|---|---|---|
| "Did my persona add concrete style/format/UX improvements beyond plain code?" | YES — count it | NO → Remove persona-induced verbosity |
| "Did I make any assumptions not grounded in the REQ-ID spec?" | NO | YES → Revert to spec |
| "Is my output measurably better than what a no-persona response would produce?" | YES | NO → Simplify output |
| "Did I apply any 'safe harbor' patterns I was explicitly forbidden to use?" | NO | YES → Redesign that section |

**Gate Outcome:**
- **All PASS** → Proceed to Gate 3
- **1-2 FAIL** → Self-correct in place, log correction note
- **3+ FAIL** → Report to Supervisor. Do NOT proceed.

---


*Supervisor summons Quality Sub-agents and executes tests.*

**Actions:**
1. **Tier 1 (Core):** Sub-agent verifies all `REQ-ID`s are fulfilled. No rogue features added.
2. **Tier 2 (Edge):** Check responsive breakpoints, dark/light mode states, and loading/error states.
3. **Tier 3 (Perf/Sec):** Run `checklist.py` (Accessibility, UI UX Audit, Web Vitals).

**Double-Evidence Collection (Mandatory):**
1. **Script Evidence:** Save the console output of `checklist.py`.
2. **Visual Evidence:** Invoke the internal Browser Subagent to open `localhost`, capture a screenshot of the new UI, and save it to the Artifacts directory.

**Escalating Recovery (Debug):**
- If ANY Tier fails → Sub-agent attempts Self-Heal (Max 2 retries).
- If Self-Heal fails → Supervisor freezes Agent, delegates to `debugger` -> `debugger` fixes -> saves error to Context Memory -> resumes Pipeline.

---

## FINAL DELIVERABLE: PROOF OF WORK
Output a strict markdown report:

```markdown
# FE-Pro Proof of Work

## Traceability Board
- [x] `REQ-FE-01`: Completed
- [x] `REQ-FE-02`: Completed

## Context Hits
- [List any context history applied from `.agent/.context/`]

## Evidence Portfolio
### 1. Script Validation
[Output snippet of checklist.py showing PASS]

### 2. Visual Proof
![Screenshot](file:///path/to/screenshot.webp)
```
