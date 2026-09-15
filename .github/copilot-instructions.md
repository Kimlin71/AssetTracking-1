# Project-wide Copilot instructions

## Project purpose
Build a C# console application for asset tracking as an educational mini project.

## Source of truth
1. Follow the project description and acceptance checklist in `docs/project-context.md`.
2. Preserve the required console behavior and output columns.
3. When sources conflict, report the conflict before changing code.

## Coding rules
- Use one public class, enum, or interface per file.
- Use ordinary `//` comments only when they explain why. Do not use XML documentation comments unless explicitly requested.
- Prefer clear names, short methods, guard clauses, and single responsibility.
- Use `decimal` for money and `DateTime` or `DateOnly` consistently for dates.
- Keep domain logic separate from console input/output and file persistence.
- Do not add packages unless the standard library is insufficient and the reason is documented.
- Never hide compilation warnings or exceptions.

## Workflow rules
- Inspect the relevant files before proposing edits.
- Make the smallest coherent change.
- Build after implementation changes.
- Run tests after implementation changes.
- Explain changed files, checks run, remaining risks, and the next recommended task.
- Do not claim a requirement is complete unless it is verified.
