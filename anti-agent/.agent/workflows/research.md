---
description: Trigger the Deep Research & Crawler Engine to execute Breadth/Depth data scraping and synthesize complex real-time internet searches into Socratic markdown reports.
---

# /research-protocol — Internet Crawler & Socratic Synthesis

You are now activating the **Deep Research Engine**. This workflow entirely bypasses the LLM's pre-trained knowledge cutoff. It relies exclusively on live internet scraping tools (`search_web`, `read_url_content`, and `browser_subagent`) to gather factual evidence.

## ZERO-TRUST MANDATE
> If you cannot find live URL evidence to answer the query, you must admit failure rather than guessing based on pre-existing knowledge. Every fact MUST have a citation.

## GATE 1: DYNAMIC LIMIT & QUERY BLUEPRINT
*Operated by Supervisor (Orchestrator)*

**Actions:**
1. **Analyze Complexity:** Read the user's research query.
2. **Set Dynamic Limit:** Set the maximum number of URLs the agent is allowed to scrape concurrently. 
   - *Simple definition search:* Limit = 2 URLs.
   - *Comparative tech analysis:* Limit = 5-8 URLs.
   - *Deep literature review:* Limit = 10 URLs.
3. **Generate Search Blueprint:** Break down the core query into 3 to 7 distinct web search angles (Breadth-First).

**Gate Checkpoint:** Output the Blueprint and the calculated URL Limit. 
> DO NOT proceed to Gate 2. ASK THE USER: "Do you approve this search blueprint and URL allowance? (Y/N)"

---

## GATE 2: LIVE CRAWLING (BREADTH → DEPTH)
*Operated by `research-analyst`*

**Actions:**
1. Execute `search_web` for the approved angles. Collect a massive pool of potential URLs.
2. Filter the pool based on the Dynamic Limit set in Gate 1. Prioritize authoritative sites, official documentation, or heavy-discussion forums (like Reddit, HackerNews).
3. Execute `read_url_content` to physically extract the text from those URLs.
4. If a URL blocks bots or uses heavy Javascript (e.g., infinite scroll or data dashboards), invoke `browser_subagent` to render it and take a screenshot.

---

## GATE 3: SYNTHESIS & REPORTING
*Operated by `research-analyst`*

**Actions:**
1. Cross-reference the scraped data. Eliminate contradictions or outdated dates.
2. Format the findings into a highly structured Markdown Artifact.
3. **Comparative Structure Mandate:** If the user query is comparative (e.g., A vs B, or 'top frameworks'), you MUST render a Markdown Table categorizing key metrics.
4. **Citation Mandate:** You MUST append a References section mapping bracketed footnotes [1] to Live URLs and Timestamp.

**Final Deliverable Code Output:**
```markdown
# Deep Research Artifact Generated
- [ ] Research Output saved as a Markdown Artifact.
- [ ] Valid URLs cited in footnotes.
- [ ] Comparative structures utilized.
```
