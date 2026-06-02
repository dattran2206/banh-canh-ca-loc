---
name: code-review-checklist
description: Code review guidelines covering code quality, security, and best practices.
allowed-tools: Read, Glob, Grep
---

# Code Review Checklist

## Quick Review Checklist

### Correctness
- [ ] Code does what it's supposed to do
- [ ] Edge cases handled
- [ ] Error handling in place
- [ ] No obvious bugs

### Security
- [ ] Input validated and sanitized
- [ ] No SQL/NoSQL injection vulnerabilities
- [ ] No XSS or CSRF vulnerabilities
- [ ] No hardcoded secrets or sensitive credentials
- [ ] **AI-Specific:** Protection against Prompt Injection (if applicable)
- [ ] **AI-Specific:** Outputs are sanitized before being used in critical sinks

### Performance
- [ ] No N+1 queries
- [ ] No unnecessary loops
- [ ] Appropriate caching
- [ ] Bundle size impact considered

### Code Quality
- [ ] Clear naming
- [ ] DRY - no duplicate code
- [ ] SOLID principles followed
- [ ] Appropriate abstraction level

### Testing
- [ ] Unit tests for new code
- [ ] Edge cases tested
- [ ] Tests readable and maintainable

### Documentation
- [ ] Complex logic commented
- [ ] Public APIs documented
- [ ] README updated if needed

## AI & LLM Review Patterns (2025)

### Logic & Hallucinations
- [ ] **Chain of Thought:** Does the logic follow a verifiable path?
- [ ] **Edge Cases:** Did the AI account for empty states, timeouts, and partial failures?
- [ ] **External State:** Is the code making safe assumptions about file systems or networks?

### Prompt Engineering Review
```markdown
// Vague prompt in code
const response = await ai.generate(userInput);

// Structured & Safe prompt
const response = await ai.generate({
  system: "You are a specialized parser...",
  input: sanitize(userInput),
  schema: ResponseSchema
});
```

## Anti-Patterns to Flag

```typescript
// Magic numbers
if (status === 3) { ... }

// Named constants
if (status === Status.ACTIVE) { ... }

// Deep nesting
if (a) { if (b) { if (c) { ... } } }

// Early returns
if (!a) return;
if (!b) return;
if (!c) return;
// do work

// Long functions (100+ lines)
// Small, focused functions

// any type
const data: any = ...

// Proper types
const data: UserData = ...
```

## Review Comments Guide

```
// Blocking issues use 🔴
BLOCKING: SQL injection vulnerability here

// Important suggestions use 🟡
🟡 SUGGESTION: Consider using useMemo for performance

// Minor nits use 🟢
🟢 NIT: Prefer const over let for immutable variable

// Questions use ❓
❓ QUESTION: What happens if user is null here?
```

## 🏁 Final Checklist Scripts (Available for Audit)

When performing "son kontrolleri yap", "final checks", or a code audit, you can invoke the following scripts via:
`python .agent/skills/<skill>/scripts/<script>.py`

| Script                     | Skill                 | When to Use         |
| -------------------------- | --------------------- | ------------------- |
| `security_scan.py`         | vulnerability-scanner | Always on deploy    |
| `api_validator.py`         | api-patterns          | API changes         |
| `lint_runner.py`           | lint-and-validate     | Every code change   |
| `type_coverage.py`         | lint-and-validate     | Every code change   |
| `test_runner.py`           | testing-patterns      | After logic change  |
| `schema_validator.py`      | database-design       | After DB change     |
| `ux_audit.py`              | frontend-design       | After UI change     |
| `accessibility_checker.py` | frontend-design       | After UI change     |
| `react_perf_checker.py`    | nextjs-react-expert   | After UI change     |
| `seo_checker.py`           | seo-fundamentals      | After page change   |
| `i18n_checker.py`          | i18n-localization     | After text change   |
| `mobile_audit.py`          | mobile-design         | After mobile change |
| `lighthouse_audit.py`      | performance-profiling | Before deploy       |
| `playwright_runner.py`     | webapp-testing        | Before deploy       |

**Priority Execution Order:**
1. Security → 2. Lint → 3. Schema → 4. Tests → 5. UX → 6. Seo → 7. Lighthouse/E2E
