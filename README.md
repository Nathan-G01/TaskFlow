# TaskFlow

TaskFlow is an orchestration loop for AI-supervised task execution.

The supervisor reads `plan.md` and `progress.md`, creates the next bounded task, reviews the agent result, and decides whether the run can continue. The agent executes exactly one task at a time. The orchestrator connects both roles, persists reviewed history, and logs task lifecycle events.

TaskFlow also expects two role instruction files beside `plan.md`:

- `supervisor.md`
- `agent.md`

These files are injected into provider prompts and are part of the runtime contract.

## Core Loop

1. Load strategic context from `plan.md`.
2. Load reviewed task history from `progress.md`.
3. Ask the supervisor provider to create the next task.
4. Dispatch that task to the execution provider.
5. Ask the supervisor provider to validate or invalidate the result.
6. Append the reviewed task to `progress.md`.
7. Log runtime events to console and `taskflow.log`.

Supervisor and agent providers are independent. The current implementation uses Codex CLI for both roles, but the runtime keeps them separate.

## Codex Provider

`TaskFlow.Providers.Codex` is now a real Codex CLI-backed provider.

Requirements:

- `codex` must be installed and available on `PATH`
- the machine must already be authenticated with `codex login`
- provider responses must be strict JSON only

Runtime behavior:

- supervisor runs through `codex exec` in `read-only`
- agent runs through `codex exec` in `workspace-write`
- each operation is single-shot
- transport or protocol failures fail fast

The provider uses strict request/response DTOs and a JSON schema for:

- `CreateNextTaskAsync`
- `ReviewTaskAsync`
- `ExecuteAsync`

## Provider Protocol

Provider responses must be strict JSON only:

- no markdown fences
- no leading or trailing commentary
- no missing required fields
- no mismatched task IDs
- no unknown top-level fields

Internal runtime models stay typed:

- `TaskAssignment`
- `TaskResult`
- `SupervisorTaskDecision`
- `TaskReview`

Provider DTOs are formalized through:

- `CreateNextTaskRequest` / `CreateNextTaskResponse`
- `ReviewTaskRequest` / `ReviewTaskResponse`
- `ExecuteTaskRequest` / `ExecuteTaskResponse`

## `progress.md` Format

`progress.md` is the reviewed business ledger. It is append-only and markdown-first.

Each reviewed task is persisted as one section:

```md
## Task 11111111-1111-1111-1111-111111111111
- Timestamp: 2026-04-01T18:00:00.0000000+00:00
- ValidationStatus: Validated
- ExecutionStatus: Succeeded
- Title: Create provider abstraction
- Instructions: Introduce a strict JSON protocol for provider responses.
- ExpectedOutput: Provider protocol DTOs and parser added.
- ResultSummary: Added the protocol layer and strict JSON validation.
- ValidationSummary: Result matches the expected output and can be used as the basis for the next task.
```

Text outside task sections is ignored by the parser.

## Logging

Runtime lifecycle events are written to:

- console
- `taskflow.log`

Logged events include:

- run start
- task proposed
- task dispatched
- task completed
- task reviewed
- task persisted
- run completed
- run stopped on invalidation or protocol error

`progress.md` is the reviewed history. `taskflow.log` is the operational log.

## Configuration

Codex runtime settings live in [`src/TaskFlow.Cli/appsettings.json`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Cli/appsettings.json).

Example:

```json
{
  "TaskFlow": {
    "Providers": {
      "Codex": {
        "Command": "codex",
        "Supervisor": {
          "Model": "gpt-5-codex",
          "Sandbox": "ReadOnly",
          "Approval": "Never",
          "Ephemeral": true,
          "WorkingDirectory": "."
        },
        "Agent": {
          "Model": "gpt-5-codex",
          "Sandbox": "WorkspaceWrite",
          "Approval": "OnRequest",
          "Ephemeral": true,
          "WorkingDirectory": "."
        }
      }
    }
  }
}
```

Relative paths are resolved against the repository root.
`Profile` is optional and should only be set if you have a real Codex profile configured locally.

## Sample Usage

Example `plan.md`:

```md
# Formalize provider protocol

The next milestone is to define strict JSON request/response contracts for supervisor and agent providers.

Keep the run deterministic.
Keep tasks small.
Persist reviewed history in markdown.
```

Run with default files:

```bash
dotnet run --project src/TaskFlow.Cli
```

Run with explicit state files:

```bash
dotnet run --project src/TaskFlow.Cli -- .\\plan.md .\\progress.md --supervisor-provider codex --agent-provider codex
```

Run with explicit log path:

```bash
dotnet run --project src/TaskFlow.Cli -- .\\plan.md .\\progress.md --supervisor-provider codex --agent-provider codex --log-path .\\taskflow.log
```

Required files beside `plan.md`:

```text
plan.md
progress.md
supervisor.md
agent.md
```

## Structure

`TaskFlow.Providers.Abstraction` is organized by responsibility:

- `Core`
- `Engines`
- `Factories`
- `Protocol`

The main architecture diagrams live in:

- [`docs/taskflow-class-diagram.puml`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/docs/taskflow-class-diagram.puml)
- [`docs/taskflow-sequence-diagram.puml`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/docs/taskflow-sequence-diagram.puml)
- [`docs/roadmap.md`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/docs/roadmap.md)
