---
name: "full-restructure"
description: "Fully review and restructure the project for cleaner architecture, naming, and tests."
argument-hint: "Optional constraints, non-negotiable names, or areas to preserve"
agent: "csharp-expert"
---

Perform a full-source restructuring pass for the current repository.

If the user provides an argument, treat it as a hard constraint or additional priority, while still performing the
complete restructure.

Core mandate:

- Read every existing directory, file, and source file under `./src` before deciding on structural changes.
- After the full `./src` review, inspect dependent `./tests`, `./dev`, and other relevant project files that must be
  updated to match the restructure.
- Review the whole project structure with SOLID principles, maintainability, and .NET industry conventions in mind.
- You are allowed to create, edit, add, move, rename, and remove directories and files when that materially improves the
  project.
- Review naming at every level: directories, files, types, members, and public API terms. Prefer concise, specific,
  industry-standard naming.
- Adapt tests after the restructure so they match the new structure, names, and behavior.

Required workflow:

1. Read the repository instructions, relevant docs, the full `./src` tree, and then the dependent `./tests` and `./dev`
   code that the restructure may affect.
2. Build an inventory of the current structure, responsibilities, naming patterns, and architectural smells.
3. Evaluate the design against SOLID principles:
    - Single Responsibility Principle
    - Open/Closed Principle
    - Liskov Substitution Principle
    - Interface Segregation Principle
    - Dependency Inversion Principle
4. Decide whether the current project layout matches the library's actual size and purpose. Do not over-engineer a small
   library, but do remove avoidable structural ambiguity.
5. Restructure the codebase into a cleaner, more conventional layout where justified. This can include:
    - moving files into better directories
    - collapsing unnecessary layers
    - splitting files or types with mixed responsibilities
    - renaming unclear or verbose files, folders, classes, delegates, or methods
    - aligning namespaces with the new structure
6. Update any affected development or consumer code that depends on renamed or moved types.
7. Update the test project to reflect the new structure and naming.
8. Validate the result by running the relevant build and test commands/tasks.

Restructuring standards:

- Read all of `./src` before proposing the new structure. Do not sample only a subset.
- Prefer root-cause cleanup over cosmetic edits.
- Preserve behavior unless a structural correction requires a behavioral change; if behavior changes, make it deliberate
  and explain it.
- Keep the design proportionate to the project. Favor clarity over abstraction count.
- Avoid introducing interfaces, layers, or folders that do not clearly improve extensibility, substitution, or
  testability.
- Use consistent naming that matches the domain and public intent of the library.
- Ensure tests still cover the renamed and reorganized code paths.

Minimum validation:

- Build the solution after restructuring.
- Run the test suite after restructuring.
- If anything cannot be validated, say exactly what was not validated and why.

Output requirements:

1. Start with a concise architecture and naming assessment of the original structure.
2. Then present the restructuring plan before editing files.
3. After implementation, summarize the structural changes, naming changes, and any intentional API or behavior changes.
4. Report build and test results.
5. List any remaining tradeoffs or follow-up work.
6. Cleanup empty directories and remove any temporary files or notes created during the process.

Repository guidance to consult during the task:

- [DDD and .NET architecture guidelines](../instructions/dotnet-architecture-good-practices.instructions.md)
