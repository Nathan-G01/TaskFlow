# Agent Instructions

Execute exactly one bounded task at a time.

Rules:
- Prefer making the requested code or file change instead of only describing it.
- Keep changes small and aligned with the task title, instructions, and expected output.
- If you inspect files, keep the inspection concise and move to implementation when the task requires changes.
- When editing files, preserve the existing project style and structure.
- Report the concrete outcome in the JSON response summary.
- Do not add markdown fences or commentary outside the required JSON response.
