---
description: Trigger opencode CLI as a sub-worker to extract and compress LLM Wiki knowledge (Local-Splitter & SkillReducer pattern)
---

# /triage-wiki - Automatically compress and load context from LLM Wiki (Python Wrapper)

This process turns `Antigravity` (You) into a Master Agent, directing the "Read theoretical documentation" task to `opencode` (Worker Agent) via a control script (Wrapper).

---

## Execution Process (For Antigravity only)

When the user activates `@[/triage-wiki] [Topic to research]`, you (Antigravity) **MUST** follow these steps sequentially:

### Step 1: Deploy Worker (Python Script)

Use the `run_command` tool to activate the dispatch script. This script already wraps safety parameters (120s Timeout, error handling) and Prompts conventions for `opencode`.

Execution command:

`python .agent/scripts/triage_worker.py "[Topic]"`

**Note:**

- Ensure the `Cwd` of the `run_command` command is the project's root directory.

- Set up Wait/Async appropriately to wait for the script to report "Success".

### Step 2: Absorb Context (View File)
After the Terminal reports that the Python Script has finished running and the file has been successfully generated (`[+] Success!`), immediately call the `view_file` tool.

- **Objective:** Read the entire contents of `.agent/tmp_wiki_context.md`.

- At this point, your "brain" has the purest core context without any junk.

### Step 3: Execution
Based on the knowledge gained from Step 2, answer the user's original question or start writing code/fixing bugs. You can manually instruct the system to delete the `tmp_wiki_context.md` file after completion to clean up the project.

---

## Strict Rules
- Do not load random `.md` files in the system if this workflow exists. Prioritize letting `triage_worker.py` handle the surveying task for you.

- Do not let AntiGravity manually run bash `cat` or `grep` commands to merge files.