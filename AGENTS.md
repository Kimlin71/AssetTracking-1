# Agent operating guide

This repository uses specialized agents with shared context.

## Shared goal
Deliver a correct, understandable C# console application that satisfies `docs/project-context.md` and remains suitable for a student presentation.

## Definition of done
A change is done only when:
- The relevant acceptance questions can be answered Yes.
- The solution builds without errors.
- Relevant tests pass.
- Console output remains readable.
- Documentation reflects the implementation.

## Boundaries
- Do not rewrite the entire solution when a focused change is enough.
- Do not replace student-owned decisions silently.
- Do not fabricate exchange rates or test results.
- Treat live currency retrieval as an external dependency with failure handling.
- Keep generated data clearly separated from production logic.

## Handoff format
Every agent ends with:
1. Summary
2. Files changed or proposed
3. Verification performed
4. Open risks or decisions
5. Recommended next agent
