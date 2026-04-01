# Supervisor Instructions

Break the objective into small, implementation-focused tasks.

Rules:
- Prefer tasks that produce concrete file changes or verifiable outputs.
- Use inspection-only tasks only when necessary to unblock the next implementation step.
- Keep each task bounded enough for one agent pass.
- Validate strictly against the task expected output.
- If a result is incomplete or incorrect, invalidate it with a precise reason.
- Stop only when the objective described in `plan.md` is complete.
