---
name: research-protocol
description: Protocol for performing Breadth-First internet search followed by Depth-First scraping, and structuring live data into Zero-Hallucination Markdown Reports.
---

# Deep Research & Scraping Protocol

This skill dictates the exact methodology the `research-analyst` and Supervisor must follow to extract, analyze, and format real-time internet data without hallucination.

## The Methodology: Breadth → Depth → Synthesize

When confronted with a research task, DO NOT just run a single Google search and read the first link. Follow this 3-Step Scientific Method:

### Step 1: Breadth-First Search (Data Discovery)
1. Deconstruct the user's query into 3 to 7 distinct **Search Angles**.
   - *Example:* "Next.js vs Remix performance" → Angle 1 (Next.js Vitals benchmarks), Angle 2 (Remix latency tests), Angle 3 (Reddit developer opinions 2024).
2. Execute `search_web` queries for each angle simultaneously.
3. Collect an initial pool of promising URLs.

### Step 2: Depth-First Scrape (Data Extraction)
1. **Apply Dynamic Limit:** Check the allowance set by the Supervisor (e.g., Max 5 URLs). Select the top-tier URLs from your pool prioritizing authoritative sources or specific data points.
2. **Scrape:** Use `read_url_content` to consume the textual data.
3. **Fallback:** If a massive JavaScript payload blocks the text, or if visual evidence of a graph/chart is needed, deploy the `browser_subagent` to physically navigate to the page and capture a screenshot.

### Step 3: Synthesis & Reporting (The Output)
Your final delivery must be an Artifact named `{topic}-live-report.md`.

#### Mandatory Structural Rules for Reports:
1. **The Timestamp Box:** At the very top, state the execution timestamp.
   `> Research Conducted: 2026-04-04 00:xx UTC`
2. **Comparative Tables (If Applicable):** If the user asks to compare A vs B, or if data naturally aligns, you MUST render a Markdown Table. Do not just write walls of text.
3. **Reference Footnotes:** Every claim MUST have a bracketed number pointing exactly to a URL. 
   - *Correct:* "Next.js 14 introduced Server Actions which reduces client bundle size [1]."
   - *Wrong:* "Next.js is faster now."

```markdown
## References
[1] Vercel Blog - Server Actions (Fetched: 2026-04-04): `https://vercel.com/blog/...`
[2] Subagent Visual Proof: `[Screenshot: Remix Network Waterfall](file:///artifacts/remix_network.webp)`
```

## Avoid Typical AI Research Pitfalls
- **The "Good Enough" Trap:** Don't stop at reading Wikipedia. Dig into Reddit threads, official documentation, or GitHub issues using targeted search operators (`site:github.com`).
- **The Date Trap:** Always verify the date on the article you scrape. If you scraped an article from 2022 to answer a question about 2026 tech, you failed.
- **Over-Scraping:** Be mindful of context windows. If `read_url_content` returns 20,000 lines of junk sidebar HTML, `grep_search` or summarize immediately before parsing the next URL.
