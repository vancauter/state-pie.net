---
name: "dotnet-testing"
description: "Use when creating, editing, or reviewing C# and xUnit test files."
applyTo: "tests/**/*.cs,**/*Tests.cs"
---

# .NET testing guidance

- Prefer focused, deterministic tests that prove one observable behavior at a time.
- Name tests with `MemberOrBehavior_WhenCondition_ExpectedOutcome`.
- Use `Fact` for a single scenario and `Theory` when the same assertions apply across multiple data rows.
- Keep test bodies easy to scan with clear Arrange, Act, and Assert separation.
- Match the narrowest test level to the behavior: unit by default, integration only for real boundaries, and performance tests only for explicit budgets or regressions.
- Use traits only when they affect execution policy. If the repository already separates suites with conventions such as `Category=Perf`, keep using that convention.
- Keep setup local until it is clearly reused. Move shared helpers to a common location only when duplication becomes noise.
- Write concurrency tests only when concurrency is the behavior under test. Use deterministic coordination primitives and explicit timeouts.
- Prefer simple hand-written helpers and fakes when they make the test easier to understand than a mocking framework.
- After non-trivial test changes, run the narrowest relevant test command or task.