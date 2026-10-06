# Contributing to IndTrace

Thank you for your interest in IndTrace.

- Open an issue before starting non-trivial work so the approach can be agreed first.
- The build treats every warning as an error; no null-forgiving operators (`!`), no `#pragma` suppressions.
- Errors flow through `Result<T>` (IndQuestResults), not exceptions across boundaries.
- Use `IDateTimeMachine` for time, never `DateTime.Now`.
- Tests use xUnit v3, Shouldly and NSubstitute. MediatR, AutoMapper, FluentAssertions and Moq are not used.
- Keep the design rule: IndTrace **tracks and validates**; it never commands equipment.

By submitting a contribution you agree that it is licensed under the GNU AGPL v3.0 or later, the license of
this repository.
