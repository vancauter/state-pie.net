## Action Checklist (derived from code review)

1. [] Add developer note about external project references
    - File: `README.md` or `docs/internal/dependencies.md`
    - Purpose: Document that `SimpleLog` (or other referenced projects) provide concurrency primitives used by the
      library.

2. [x] Add optional logging for swallowed observer exceptions
    - File: `src/AtomicState/Internal/StateStream.cs` (or a logging helper)
    - Purpose: Surface subscriber failures to diagnostic logs while preserving the current isolation semantics.

3. [] Clarify CombineReducers pairing in docs
    - File: `src/AtomicState/Store/Factory.cs` (XML docs) and `README.md`
    - Purpose: Make it explicit that the returned initial `IState` and `Reducer` must be used together and that other
      root shapes are unsupported.

4. [x] Sanity-check CI/test task (reviewed, do not implement at this point)
    - Command: `dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"`
    - Purpose: Ensure CI runs non-performance tests by default; add a note to the repo `CONTRIBUTING` or CI
      configuration if missing.

5. [x] (Optional) Add a short example in README linking to `dev/AtomicState/Program.cs`
    - Purpose: Quickstart for new contributors to run the dev harness and exercise typical usage. (reviewed, not
      needed - will add examples dir at later stage)
