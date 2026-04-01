# TaskFlow Sample: Python Pong

This sample shows how to run TaskFlow against a simple goal: create a basic Pong game in Python.

## Contents

- `bin/`
  Contains a published `TaskFlow.Cli.exe`.
- `plan.md`
  Strategic plan for the supervisor.
- `progress.md`
  Reviewed task ledger written by TaskFlow as the run advances.
- `workspace/`
  Intended output area for the generated Python game.

## Prerequisites

- Codex CLI installed and available on `PATH`
- Local Codex authentication completed with `codex login`
- .NET runtime available if needed by the published executable

## Build The Sample Executable

From the repository root:

```powershell
dotnet publish .\src\TaskFlow.Cli\TaskFlow.Cli.csproj -c Release -o .\sample\bin
```

This refreshes the published CLI and its dependencies inside `sample/bin`.
`Profile` in `appsettings.json` is optional and should be left unset unless you actually use a local Codex profile.

## Run

From the repository root:

```powershell
.\sample\bin\TaskFlow.Cli.exe .\sample\plan.md .\sample\progress.md --supervisor-provider codex --agent-provider codex --log-path .\sample\taskflow.log
```

## Expected Goal

The run should work toward producing a small playable Pong implementation at:

```text
sample/workspace/pong.py
```

The intended scope is deliberately small:

- Python only
- local desktop play
- keyboard controls for two paddles
- ball movement and wall bounce
- paddle collision
- simple score display

No packaging, no online play, and no advanced graphics are required.
