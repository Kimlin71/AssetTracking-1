# Asset Tracking Agent Starter

This starter pack adds reusable GitHub Copilot context, custom agents, prompt files, acceptance questions, and workflow documentation to an existing C# Asset Tracking repository.

## Install
Copy the contents of this folder into the root of the C# repository. Preserve the `.github` folder.

## Recommended sequence
1. Run `01-analyze-requirements` with Requirements Analyst.
2. Resolve ambiguities in the acceptance checklist.
3. Run `02-design-slice` with CSharp Architect.
4. Run `03-implement-slice` with CSharp Implementer.
5. Run the Test Designer for rules introduced by the slice.
6. Run `04-review-slice` with Code Reviewer.
7. Correct findings with CSharp Implementer.
8. Run `05-document-slice` with Project Documenter.
9. Commit the verified slice in Git.

## First suggested slice
Create the solution, the abstract Asset type, Computer, MobilePhone, and an in-memory list. Print a basic table. Do not introduce live currency, JSON, menu editing, or optional features in the first slice.
