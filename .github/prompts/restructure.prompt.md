---
name: "restructure"
description: "Fully review and restructure the project for cleaner architecture, naming, and tests."
argument-hint: "Optional constraints, non-negotiable names, or areas to preserve"
agent: "C# Expert"
---

# Full Project Restructure

Perform a full-source restructuring pass for the current repository.

If the user provides an argument, treat it as a hard constraint or additional priority while still performing the
complete restructure.

## Mission

- Read every source file under `./src` before deciding on structural changes.
- After the full `./src` review, inspect dependent `./tests`, `./dev`, and any other relevant project files that must be
  updated to match the restructure.
- Review the whole project structure with SOLID principles, maintainability, and .NET conventions in mind.
- Create, edit, add, move, rename, and remove files or directories when that materially improves the project.
- Review naming at every level: directories, files, types, members, and public API terms.
- Adapt tests so they match the new structure, names, and behavior.

## Workflow

1. Read the repository instructions, relevant documentation, the full `./src` tree, and then the dependent `./tests` and
   `./dev` code that the restructure may affect.
2. Build an inventory of the current structure, responsibilities, naming patterns, and architectural smells.
3. Evaluate the design against SOLID principles.
    - Single Responsibility Principle
    - Open/Closed Principle
    - Liskov Substitution Principle
    - Interface Segregation Principle
    - Dependency Inversion Principle
4. Decide whether the current project layout matches the library's actual size and purpose. Do not over-engineer a small
   library, but remove avoidable structural ambiguity.
5. Restructure the codebase into a cleaner, more conventional layout where justified.
    - Move files into better directories.
    - Collapse unnecessary layers.
    - Split files or types with mixed responsibilities.
    - Rename unclear or verbose files, folders, classes, delegates, or methods.
    - Align namespaces with the new structure.
6. Update any affected development or consumer code that depends on renamed or moved types.
7. Update the test project to reflect the new structure and naming.
8. Validate the result by running the relevant build and test commands or tasks.

## Restructuring Standards

- Read all of `./src` before proposing the new structure.
- Prefer root-cause cleanup over cosmetic edits.
- Preserve behavior unless a structural correction requires a deliberate behavioral change.
- Keep the design proportionate to the project.
- Avoid introducing interfaces, layers, or folders that do not clearly improve extensibility, substitution, or
  testability.
- Use consistent naming that matches the domain and public intent of the library.
- Ensure tests still cover the renamed and reorganized code paths.

## Validation

- Build the solution after restructuring.
- Run the test suite after restructuring.
- If anything cannot be validated, state exactly what was not validated and why.

## Output Expectations

1. Start with a concise architecture and naming assessment of the original structure.
2. Present the restructuring plan before editing files.
3. After implementation, summarize the structural changes, naming changes, and any intentional API or behavior changes.
4. Report build and test results.
5. List any remaining trade-offs or follow-up work.
6. Clean up empty directories and remove any temporary files or notes created during the process.

## References

- [DDD and .NET architecture guidelines](../instructions/dotnet-architecture.instructions.md)
