# Antigravity Kit Architecture

> Comprehensive AI Agent Capability Expansion Toolkit

---

## Overview

Antigravity Kit is a modular system consisting of:

- **16 Specialist Agents** — Organized into Supervisor (1) + Workers (15)
- **38 Skills** — Domain-specific knowledge modules (including context-protocol)
- **20 Workflows** — Slash command procedures (including /context)
- **Context Memory Layer** — Persistent JSON-based context store for cross-session knowledge

---

## Directory Structure

```plaintext
.agent/
├── ARCHITECTURE.md          # This file
├── agents/                  # 16 Specialist Agents
├── skills/                  # 38 Skills (incl. context-protocol)
├── workflows/               # 20 Slash Commands (incl. /context)
├── rules/                   # Global Rules
├── scripts/                 # Master Validation Scripts
└── .context/                # Persistent Context Memory (gitignored)
    ├── _index.json           # Master index (categories + IDs)
    ├── _archive/             # Compacted low-reuse entries
    └── *.json                # Context files by task/bug type
```

---

## Agents (16)

Specialist AI personas for different domains.

| Agent                    | Focus                      | Role          | Domain Group  |
| ------------------------ | -------------------------- | ------------- | ------------- |
| `orchestrator`           | Multi-agent coordination   | **Supervisor** | ALL           |
| `frontend-specialist`    | Web UI/UX                  | Worker        | frontend      |
| `seo-specialist`         | Ranking, visibility        | Worker        | frontend      |
| `performance-optimizer`  | Speed, Web Vitals          | Worker        | frontend      |
| `backend-specialist`     | API, business logic        | Worker        | backend       |
| `database-architect`     | Schema, SQL                | Worker        | backend       |
| `security-auditor`       | Security compliance        | Worker        | security      |
| `debugger`               | Root cause analysis        | Worker        | quality       |
| `test-engineer`          | Testing & QA               | Worker        | quality       |
| `devops-engineer`        | CI/CD, Docker              | Worker        | devops        |
| `mobile-developer`       | iOS, Android, RN           | Worker        | platform      |
| `game-developer`         | Game logic, mechanics      | Worker        | platform      |
| `research-analyst`       | Internet scraping, data    | Worker        | intelligence  |
| `project-planner`        | Discovery, task planning   | Support       | —             |
| `documentation-writer`   | Manuals, docs              | Support       | —             |
| `explorer-agent`         | Codebase analysis          | Support       | —             |

---

## Skills (38)

Modular knowledge domains that agents can load on-demand based on task context.

### Frontend & UI

| Skill                   | Description                                                           |
| ----------------------- | --------------------------------------------------------------------- |
| `nextjs-react-expert`   | React & Next.js performance optimization (Vercel - 57 rules)          |
| `web-design-guidelines` | Web UI audit - 100+ rules for accessibility, UX, performance (Vercel) |
| `tailwind-patterns`     | Tailwind CSS v4 utilities                                             |
| `frontend-design`       | UI/UX patterns, design systems                                        |

### Backend & API

| Skill                   | Description                    |
| ----------------------- | ------------------------------ |
| `api-patterns`          | REST, GraphQL, tRPC            |
| `nodejs-best-practices` | Node.js async, modules         |
| `python-patterns`       | Python standards, FastAPI      |

### Database

| Skill             | Description                 |
| ----------------- | --------------------------- |
| `database-design` | Schema design, optimization |

### Testing & Quality

| Skill                   | Description              |
| ----------------------- | ------------------------ |
| `testing-patterns`      | Jest, Vitest, strategies |
| `webapp-testing`        | E2E, Playwright          |
| `tdd-workflow`          | Test-driven development  |
| `code-review-checklist` | Code review standards    |
| `lint-and-validate`     | Linting, validation      |

### Security

| Skill                   | Description              |
| ----------------------- | ------------------------ |
| `vulnerability-scanner` | Security auditing, OWASP |
| `red-team-tactics`      | Offensive security       |

### Architecture & Planning

| Skill           | Description                |
| --------------- | -------------------------- |
| `app-builder`   | Full-stack app scaffolding |
| `architecture`  | System design patterns     |
| `plan-writing`  | Task planning, breakdown   |
| `brainstorming-framework` | Socratic questioning       |

### Mobile & Platform

| Skill           | Description           |
| --------------- | --------------------- |
| `mobile-design` | Mobile UI/UX patterns |
| `game-development` | Game logic, mechanics |

### SEO & Intelligence

| Skill              | Description                   |
| ------------------ | ----------------------------- |
| `seo-fundamentals` | SEO, E-E-A-T, Core Web Vitals |
| `research-protocol` | Deep internet research & scraping |

### Special & Meta

| Skill                     | Description               |
| ------------------------- | ------------------------- |
| `clean-code`              | Coding standards (Global) |
| `behavioral-modes`        | Agent personas            |
| `parallel-agents`         | Multi-agent patterns      |
| `mcp-builder`             | Model Context Protocol    |
| `documentation-templates` | Doc formats               |
| `i18n-localization`       | Internationalization      |
| `performance-profiling`   | Web Vitals, optimization  |
| `systematic-debugging`    | Troubleshooting           |
| `intelligent-routing`     | PRISM task routing        |
| `rust-pro`                | Rust systems programming  |
| `context-protocol`        | Memory management layer   |

---

## Workflows (20)

Slash command procedures. Invoke with `/command`.

| Command          | Description              |
| ---------------- | ------------------------ |
| `/brainstorm`    | Socratic discovery       |
| `/create`        | Create new features      |
| `/debug`         | Debug issues             |
| `/deploy`        | Deploy application       |
| `/enhance`       | Improve existing code    |
| `/orchestrate`   | Multi-agent coordination |
| `/plan`          | Task breakdown           |
| `/preview`       | Preview changes          |
| `/status`        | Check project status     |
| `/test`          | Run tests                |
| `/ui-ux-pro-max` | Design with 50 styles    |
| `/be-pro`        | Backend execution pipe   |
| `/fe-pro`        | Frontend execution pipe  |
| `/index-repo`    | PageIndex generator      |
| `/lsp-connector` | LSP preparing            |
| `/context`       | Memory management        |
| `/research`      | Deep research engine     |
| `/safe-install`  | Secure package install   |
| `/triage-wiki`   | Wiki context compression |
| `/payload-compaction` | Context token optimization |

---

## Skill Loading Protocol

```plaintext
User Request → Skill Description Match → Load SKILL.md
                                            ↓
                                    Read references/
                                            ↓
                                    Read scripts/
```

### Skill Structure

```plaintext
skill-name/
├── SKILL.md           # (Required) Metadata & instructions
├── scripts/           # (Optional) Python/Bash scripts
├── references/        # (Optional) Templates, docs
└── assets/            # (Optional) Images, logos
```

---

## Scripts (Master)

Master validation scripts that orchestrate skill-level scripts.

| Script          | Purpose                                 |
| --------------- | --------------------------------------- |
| `checklist.py`  | Priority-based validation (Core checks) |
| `verify_all.py` | Comprehensive verification (All checks) |

---

## Context Memory System

Persistent JSON-based context store that eliminates repeated work across sessions.

### Architecture

```
.agent/.context/
├── _index.json         ← Master index
├── _archive/           ← Compacted entries
└── {type}.json          ← Context files
```

### Agentic OS Pattern Coverage (12/18)

| # | Pattern | Implementation |
|---|---------|---------------|
| #1 | Query Loop | Iteration Protocol |
| #2 | State Machine | Completion Checklist |
| #3 | Escalating Recovery | 5-Level Search |
| #7 | Coordinator Restriction | FORBIDDEN list |
| #8 | Fork Isolation | Conflict Resolution Protocol |
| #9 | Context Defense | Lazy load → Compact → Archive |
| #10 | Permission Classification | Permission Matrix + Violation Log |
| #15 | Security Sandbox | Validation Gate |
| #16 | Reconciliation | /context status workflow |

---

## Statistics

| Metric              | Value                         |
| ------------------- | ----------------------------- |
| **Total Agents**    | 16                            |
| **Total Skills**    | 38                            |
| **Total Workflows** | 20                            |
| **Total Scripts**   | 7 (master/utility)            |

---

## 🔗 Quick Reference

| Need     | Agent                 | Skills                                |
| -------- | --------------------- | ------------------------------------- |
| Web App  | `frontend-specialist` | nextjs-react-expert, frontend-design  |
| API      | `backend-specialist`  | api-patterns, nodejs-best-practices   |
| Mobile   | `mobile-developer`    | mobile-design                         |
| Security | `security-auditor`    | vulnerability-scanner                 |
| Testing  | `test-engineer`       | testing-patterns, webapp-testing      |
| Debug    | `debugger`            | systematic-debugging                  |
| Rust     | `rust-pro`            | rust-pro                              |
