# Contributing to IndTrace

Thank you for your interest in IndTrace.

## License of contributions

IndTrace is licensed under the **GNU Affero General Public License v3.0 or later** (`AGPL-3.0-or-later`,
see `LICENSE`). By opening a pull request, you confirm that:

1. **You accept the license.** Your contribution is licensed to the project and to every recipient under
   `AGPL-3.0-or-later`, the same license as the rest of the repository ("inbound = outbound"). It cannot be
   offered under different or additional terms.
2. **You have the right to submit it.** The contribution is your original work, or you have the right to
   submit it under that license (for example, your employer has agreed, or it comes from a compatible open
   source project whose license and notices you keep intact).
3. **You certify it with a sign-off.** Every commit carries a `Signed-off-by:` line, which certifies the
   [Developer Certificate of Origin 1.1](https://developercertificate.org/). Add it with `git commit -s`:

   ```text
   Signed-off-by: Your Name <you@example.com>
   ```

   Pull requests with unsigned commits are not merged.

## Attribution

- Copyright in your contribution stays with you; the project's history records your authorship. Do not
  remove or alter existing copyright notices or license headers.
- New source files carry the repository's license header, exactly as in the existing files (the analyzers
  check it):

  ```csharp
  // <copyright file="MyFile.cs" company="Exxerpro Solutions SA de CV">
  // Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
  // </copyright>
  // SPDX-License-Identifier: AGPL-3.0-or-later
  ```

- Code adapted from another project must keep its original copyright and license notice, its license must be
  compatible with `AGPL-3.0-or-later`, and the pull request description must name the source.
- Contributors are credited through the Git history and the release notes.

## How to contribute

- Open an issue before starting non-trivial work so the approach can be agreed first.
- The source in this repository is published from the maintainers' upstream tree. Accepted pull requests are
  applied upstream and come back in the next sync, with your authorship and sign-off preserved.
- Keep the design rule: IndTrace **tracks and validates**; it never commands equipment.
- The build treats every warning as an error; no null-forgiving operators (`!`), no `#pragma` suppressions.
- Errors flow through `Result<T>` (IndQuestResults), not exceptions across boundaries.
- Use `IDateTimeMachine` for time, never `DateTime.Now`.
- Tests use xUnit v3, Shouldly and NSubstitute. MediatR, AutoMapper, FluentAssertions and Moq are not used.
- Run the test suites with `dotnet run --project <test project>` before opening a pull request; CI runs the
  same suites.
- Never add real customer data, credentials, hostnames or personal information. Test data must be
  fictitious (see `NOTICE.md`).

## Conduct and security

Participation is governed by the [Code of Conduct](.github/CODE_OF_CONDUCT.md). Report vulnerabilities
privately as described in the [security policy](.github/SECURITY.md), never in a public issue.
