---
name: test-and-push
description: Writes unit and integration tests for the current code changes in this .NET solution, runs the full test suite, and — only if everything passes — commits and pushes to the claude-push branch for testing. Use when the user asks to "test and push", "write tests for my changes", or after implementing a code change that needs test coverage before it goes up for review.
---

You are executing a test-then-push workflow for the ProductCrudApp .NET solution
(Controller -> Service -> Repository -> EF Core, xUnit + Moq + EF Core InMemory +
Microsoft.AspNetCore.Mvc.Testing in `ProductCrudApp.Tests`).

Follow these steps in order. Do not skip the gate in step 5 — tests must pass before
anything is committed or pushed.

## 1. Identify what changed

Run `git status --short` and `git diff` (and `git diff --staged`) to see which
production files changed — new files count too. Read each changed file fully, not
just the diff hunk, so you understand the surrounding behavior.

## 2. Write or update tests for the change

For every changed file under `ProductCrudApp.Api/`, decide what test coverage it
needs and add/update it in `ProductCrudApp.Tests/`, matching existing conventions:

- **Unit tests** (`ProductServiceTests.cs`, `ProductRepositoryTests.cs`-style): test
  a single layer in isolation. Mock dependencies with Moq for service tests; use the
  EF Core InMemory provider (a fresh database name per test) for repository tests.
  Cover the happy path, not-found/null cases, and any new branching logic.
- **Integration tests**: use `WebApplicationFactory<Program>` from
  `Microsoft.AspNetCore.Mvc.Testing` to boot the real app in-memory and exercise
  actual HTTP endpoints end-to-end (routing, model binding, DI, validation,
  status codes) — don't just call controller action methods directly, since that
  bypasses model binding and validation. A working `CustomWebApplicationFactory`
  already exists at `ProductCrudApp.Tests/CustomWebApplicationFactory.cs` —
  reuse it rather than writing a new one; it swaps the SQL Server `AppDbContext`
  registration for EF Core InMemory so no real SQL Server is needed.
  **Gotcha already solved there**: on this EF Core version, removing
  `DbContextOptions<AppDbContext>` and `AppDbContext` from the service
  collection is *not* enough to replace the `UseSqlServer` registration from
  `Program.cs` — its configuration survives via
  `IDbContextOptionsConfiguration<AppDbContext>` and gets combined with
  `UseInMemoryDatabase`, producing a "two database providers registered"
  `InvalidOperationException` at runtime. That extra
  `services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>))`
  call is required before calling `AddDbContext` with the InMemory provider —
  don't remove it if refactoring this file.

Only add tests relevant to what changed — don't pad coverage for untouched code.

## 3. Build

Run `dotnet build Dot-Net-Project.sln` from the repo root — the plain `.sln`,
not the `.slnx` file (both exist; `dotnet build` with no argument fails with
MSB1011 because there's more than one solution file in the directory). Fix any
compile errors before moving on.

## 4. Run the full test suite

Run `dotnet test Dot-Net-Project.sln` from the repo root (covers both the unit
and integration tests in `ProductCrudApp.Tests`).

## 5. Gate: only proceed if everything passed

- **If the build failed or any test failed**: stop here. Do not commit or push.
  Report which tests failed and why, and fix the underlying issue (code or test)
  before retrying from step 3.
- **If build succeeded and all tests passed**: continue to step 6.

## 6. Commit

- Run `git status` and review exactly what's staged — never blanket `git add -A`
  without checking; exclude build output (`bin/`, `obj/` — already gitignored) and
  double-check no secrets (connection strings, API keys) are being added. This repo
  keeps its SQL connection string in `dotnet user-secrets`, not in `appsettings.json`
  — verify that convention hasn't been broken before committing.
- Write a commit message describing the change and the tests added, focused on why,
  not a restatement of the diff.

## 7. Push to the claude-push branch

- Create the `claude-push` branch locally if it doesn't exist yet (based on the
  current branch), or switch to it if it does.
- Push with `git push origin claude-push` (create it with `-u` the first time).
  Always push as new commits on top of the branch's existing history — never
  force-push this branch.
- Never push directly to `main`.
- Report the pushed branch/commit and mention that a PR against `main` can be
  opened from there if the user wants one — but do not open the PR yourself unless
  asked.

## Updating this skill

This file is the single source of truth for the workflow — edit the steps above
directly to change target branch name, test commands, coverage expectations, or
commit/push conventions. No code changes needed elsewhere.
