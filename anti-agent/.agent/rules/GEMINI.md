---
trigger: always_on
---

# GEMINI.md - Antigravity Kit (Master Router)

> **PRISM Adapted (v2.0)** — Agent persona loading is intent-gated per [arXiv 2603.18507](https://arxiv.org/abs/2603.18507)
> **ARCHITECTURE RULE:** This file is ONLY a router. For detailed instructions, lazy-load the respective `.md` files.

---

## 🧭 TIER 0: UNIVERSAL PROTOCOLS (MANDATORY)

1. **Intelligent Agent Routing:** Before ANY response, you MUST route to the correct agent using PRISM task classification (Discriminative/Alignment/Hybrid). 
   **Read:** `.agent/skills/intelligent-routing/SKILL.md`
2. **Socratic Gate (Anti-Assumption):** NEVER implement complex features without asking 3 clarifying questions first. 
   **Read:** `.agent/skills/brainstorming-framework/SKILL.md`
3. **Clean Code & Output:** All code must be concise, tested, and follow 2025 standards.
   **Read:** `.agent/skills/clean-code/SKILL.md`
4. **File Dependency & Architecture Awareness:**
   - Read `ARCHITECTURE.md` at session start to understand the system map.
   - Check `CODEBASE.md` for explicit file dependencies before modifying any code.
5. **Context Memory Protocol:** 
   - After completing complex logic or a long session, you MUST summarize key decisions to `.agent/.context/session_state.json` (or update `ARCHITECTURE.md`) for Payload Compaction. 
   - ALWAYS read context files in subsequent chats.
6. **Language Handling:** Internally translate but respond in the user's language.

---

## TIER 1: EXECUTION & WORKFLOWS

**1. Turbo Command Execution (SafeToAutoRun)**
- **Local Scripts & Tooling:** ALWAYS set `SafeToAutoRun = true` for `python .agent/scripts/...`, `pytest`, `build`, or `lint` to save user time.
- **Dependency Installation:** NEVER auto-run `npm install` blindly. 
  **Read & Execute:** `.agent/workflows/safe-install.md`

**2. Final Verification (Checklist)**
- When user asks for "final checks" or "son kontrolleri yap", you MUST run `python .agent/scripts/checklist.py .`
- A task is NOT finished until this script returns success (focus on Security & Lint first).
- You can invoke scripts via `python .agent/skills/<skill>/scripts/<script>.py`

**3. Gemini Behavioral Modes**
- **plan mode:** 4-phase methodology (Analysis → Plan → Solution → Implement). NO CODE before Phase 4.
- **edit mode:** If structural change, offer `{task-slug}.md`. For single-file fixes, proceed directly.

---

## 🎨 TIER 2: SPECIALIST ROUTING

Do NOT assume design rules here. Read the exact agent file:
- **Web UI/UX:** Read `.agent/frontend-specialist.md` (Do NOT use mobile agent)
- **Mobile UI/UX:** Read `.agent/mobile-developer.md`
- **Backend/API:** Read `.agent/backend-specialist.md`

*(Note: Specialist agents contain strict rules like the Purple Ban, Template Ban, and deep design thinking protocols. You must read them before coding.)*
