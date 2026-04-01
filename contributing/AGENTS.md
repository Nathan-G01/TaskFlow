# TaskFlow Contributor Notes

This document is the implementation-oriented guide for contributors working on TaskFlow.

## Purpose

TaskFlow is an orchestration loop for AI-supervised task execution.

The runtime has four distinct responsibilities:

- `Supervisor`
  Reads `plan.md` and prior reviewed history, creates the next bounded task, and reviews agent output.
- `Agent`
  Executes exactly one bounded task through a provider engine.
- `Orchestrator`
  Runs the loop, persists reviewed history, and stops on invalidation or protocol failure.
- `Provider`
  Bridges TaskFlow to an external AI runtime such as Codex CLI.

The important design constraint is that `plan.md` is strategic context, not a predefined backlog. The supervisor generates tasks dynamically from the current run context.

## Current Runtime Loop

The current loop implemented in [`src/TaskFlow.Agents/Orchestration/Orchestrator.cs`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Agents/Orchestration/Orchestrator.cs) is:

1. Load `plan.md`.
2. Load reviewed task history from `progress.md`.
3. Ask the supervisor provider for the next task.
4. Dispatch that task to the agent provider.
5. Ask the supervisor provider to validate or invalidate the result.
6. Append the reviewed task to `progress.md`.
7. Log lifecycle events to console and `taskflow.log`.
8. Stop on invalidation or continue with a new supervisor-created task.

The orchestrator currently fails fast on provider transport errors and protocol errors.

## Project Layout

The active projects are:

- [`src/TaskFlow.Agents`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Agents)
  Supervisor, agent, orchestrator, run-context store, logging.
- [`src/TaskFlow.Cli`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Cli)
  Composition root, CLI parsing, provider selection, `appsettings.json`.
- [`src/TaskFlow.Providers/TaskFlow.Providers.Abstraction`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Providers/TaskFlow.Providers.Abstraction)
  Shared runtime contracts and strict provider protocol DTOs.
- [`src/TaskFlow.Providers/TaskFlow.Providers.Codex`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Providers/TaskFlow.Providers.Codex)
  Codex CLI-backed provider implementation.

### `TaskFlow.Providers.Abstraction`

Keep this project organized by responsibility:

- `Core`
  Canonical runtime models such as `TaskAssignment`, `TaskResult`, `TaskReview`, `SupervisorTaskDecision`, `TaskHistoryEntry`, and status enums.
- `Engines`
  `IEngine` and `ISupervisorEngine`.
- `Factories`
  `IAgentEngineFactory` and `ISupervisorEngineFactory`.
- `Protocol`
  Strict JSON DTOs, prompt generation, parsing, mapping, and `ProviderProtocolException`.

Do not flatten new abstraction types back into the project root.

### `TaskFlow.Providers.Codex`

Keep the Codex provider organized by responsibility:

- `Configuration`
- `Client`
- `Engines`
- `Factories`
- `Support`

The Codex provider should stay an adapter over the abstraction layer, not become a second orchestration system.

## File Contracts

### `plan.md`

`plan.md` is the strategic objective and constraints document. It is intentionally free-form markdown. The supervisor reads it and decides the next bounded task.

Do not treat `plan.md` as:

- a static backlog
- a checklist of tasks to consume in order
- a reviewed execution ledger

### `supervisor.md`

`supervisor.md` lives beside `plan.md` and is injected into supervisor prompt construction.

Use it for role-level planning and review rules such as:

- task sizing
- review strictness
- completion criteria
- repo-specific planning heuristics

### `agent.md`

`agent.md` lives beside `plan.md` and is injected into agent execution prompts.

Use it for role-level execution rules such as:

- edit expectations
- coding constraints
- validation expectations
- repo-specific implementation guidance

### `progress.md`

`progress.md` is append-only reviewed history. It is parsed by [`MarkdownRunContextStore`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Agents/Infrastructure/MarkdownRunContextStore.cs).

Each persisted task section must follow this structure:

```md
## Task 11111111-1111-1111-1111-111111111111
- Timestamp: 2026-04-01T18:00:00.0000000+00:00
- ValidationStatus: Validated
- ExecutionStatus: Succeeded
- Title: Example task
- Instructions: Example task instructions.
- ExpectedOutput: Example expected output.
- ResultSummary: Example execution summary.
- ValidationSummary: Example review summary.
```

Text outside `## Task <guid>` sections is ignored by the parser.

### `taskflow.log`

`taskflow.log` is the operational log, not business history. It is written by [`FileConsoleTaskLogger`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Agents/Infrastructure/FileConsoleTaskLogger.cs).

Expected event types:

- `run-start`
- `task-proposed`
- `task-dispatched`
- `task-completed`
- `task-reviewed`
- `task-persisted`
- `run-complete`
- `run-stopped`

## Provider Protocol

The internal runtime uses typed domain models. Providers communicate through explicit JSON DTOs in `TaskFlow.Providers.Abstraction/Protocol`.

The three provider-facing operations are:

- `CreateNextTaskAsync`
- `ReviewTaskAsync`
- `ExecuteAsync`

Important rules:

- provider responses must be a single JSON object
- task IDs must be GUIDs
- review and execution responses must echo the requested task ID
- required fields must always be present
- nullable fields should be `null`, not omitted, when the schema expects presence

The current parser is strict, but it also tolerates common Codex formatting defects such as fenced JSON or invalid single backslashes inside JSON strings. That tolerance exists to keep runs moving; contributors should still treat canonical strict JSON as the contract.

When changing prompts or schemas:

- keep prompt text aligned with the actual schema
- avoid describing fields as “optional” if the schema requires them to exist with `null`
- validate enum/string behavior through `JsonProviderProtocol`
- preserve typed mapping through `ProviderProtocolMapper`

## Codex Provider

The current provider implementation shells out to `codex exec`.

Key points:

- supervisor runs in `ReadOnly`
- agent runs in `WorkspaceWrite`
- provider configuration lives in [`src/TaskFlow.Cli/appsettings.json`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Cli/appsettings.json)
- provider selection still happens via CLI flags

The Codex provider is implemented through:

- [`CodexCliClient`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Providers/TaskFlow.Providers.Codex/Client/CodexCliClient.cs)
- [`CodexSupervisorEngine`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Providers/TaskFlow.Providers.Codex/Engines/CodexSupervisorEngine.cs)
- [`CodexAgentEngine`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Providers/TaskFlow.Providers.Codex/Engines/CodexAgentEngine.cs)
- [`CodexEngineFactory`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Providers/TaskFlow.Providers.Codex/Factories/CodexEngineFactory.cs)

If you add another provider, follow the same separation:

- factory
- supervisor engine
- agent engine
- provider-specific client/configuration

Do not embed provider-specific logic into `TaskFlow.Agents`.

## CLI and Configuration

The CLI entry point is [`src/TaskFlow.Cli/Program.cs`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/src/TaskFlow.Cli/Program.cs).

Current behavior:

- repository root is resolved by walking upward until `TaskFlow.slnx` is found
- default files are `<repo>/plan.md`, `<repo>/progress.md`, and `<repo>/taskflow.log`
- explicit file paths can be passed as positional arguments
- provider names can be passed with:
  - `--supervisor-provider`
  - `--agent-provider`
  - `--log-path`

Codex provider settings come from `src/TaskFlow.Cli/appsettings.json`, not environment variables.

## Sample

The sample scenario lives in [`sample`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/sample).

It contains:

- `plan.md`
- `progress.md`
- `workspace/`
- `README.md`
- `bin/TaskFlow.Cli.exe`

The current sample objective is a basic Python Pong game in `sample/workspace/pong.py`.

Refresh the sample executable with:

```powershell
dotnet publish .\src\TaskFlow.Cli\TaskFlow.Cli.csproj -c Release -o .\sample\bin
```

## Contribution Rules

- Preserve the supervisor/agent/orchestrator/provider separation.
- Keep shared runtime types in `TaskFlow.Providers.Abstraction`, not duplicated in consuming projects.
- Keep namespaces aligned with folders.
- Prefer small, typed contracts over stringly-typed glue.
- Treat `progress.md` as reviewed history only.
- Keep provider prompts, schemas, DTOs, and mappers in sync.
- Do not add environment-variable configuration for providers unless there is a strong reason; current configuration is `appsettings.json`-based.
- When modifying the sample, keep it simple and runnable.

## Validation

Before closing a change, at minimum run:

```powershell
dotnet build TaskFlow.slnx
```

If the sample or published executable changed, also refresh:

```powershell
dotnet publish .\src\TaskFlow.Cli\TaskFlow.Cli.csproj -c Release -o .\sample\bin
```

## Diagrams

Architecture diagrams live in:

- [`docs/taskflow-class-diagram.puml`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/docs/taskflow-class-diagram.puml)
- [`docs/taskflow-sequence-diagram.puml`](c:/Users/NathanGaillard/0repos/PersonalProjects/TaskFlow/docs/taskflow-sequence-diagram.puml)
