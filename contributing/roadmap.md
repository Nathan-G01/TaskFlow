# Roadmap

This roadmap reflects the current state of the codebase and the next highest-value work.

## Near Term

- Claude Code integration
  Add a new provider project for Claude Code so the supervisor can run on Claude while agents continue to run on Codex.
- Options and configuration improvements
  Improve `appsettings.json` structure, validation, defaults, and override ergonomics without falling back to environment-variable sprawl.
- Better release and CI flows
  Separate build/test CI from release publishing and tighten version/tag automation around `Directory.Build.props`.
- Test coverage
  Add automated tests for protocol parsing, `progress.md` persistence, orchestrator flow, and provider command construction.

## Provider Work

- Real multi-provider support
  Register and document at least one provider beyond Codex.
- Stronger provider diagnostics
  Preserve enough failing provider output to debug transport and protocol issues without logging excessive prompt content.
- Retry and recovery policy
  Decide how the orchestrator should behave on invalidation, protocol defects, and transient provider failures.

## Runtime Improvements

- Richer supervisor behavior
  Improve task sizing, task repair after invalidation, and completion detection.
- Better agent reuse strategy
  Revisit how agents are reused across tasks and whether session-aware providers should keep state between executions.
- More structured run context
  Expand beyond `plan.md` and `progress.md` if additional durable state becomes necessary.

## Developer Experience

- More samples
  Add additional end-to-end examples beyond the Python Pong sample.
- Better documentation
  Add provider authoring guidance, release instructions, and concrete examples of expected provider JSON.
- Packaging improvements
  Produce cross-platform release binaries and document supported runtime assumptions.
