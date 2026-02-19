# NuGet Publication — TODO

This checklist captures the remaining work to make AtomicRx production-ready for publishing to NuGet.

Priority: High → Low

- [ ] Package metadata & CI
  - Add package metadata to `src/AtomicRx.csproj` (PackageId, Version, Authors, Description, RepositoryUrl,
      PackageReleaseNotes).
  - Add GitHub Actions to build, run tests, `dotnet pack`, and optionally publish on release.

- [ ] XML docs & API docs
  - Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `src/AtomicRx.csproj`.
  - Produce API documentation (DocFX or similar) or include API summary in `README.md`.

- [ ] Tests & coverage
  - Add unit tests for `OnCompleted` delivery on `Store.Dispose()` and `OnError` behavior.
  - Test reducer exception handling and decide whether to surface as `OnError` or not.
  - Add tests for `HotSwapReducer` concurrency, middleware composition, and edge cases.

- [ ] Public API stabilization & docs
  - Finalize public API surface and document recommended usage patterns (Factory helpers, action creation, selectors).
  - Add usage examples to `README.md` and a short migration note if any breaking changes are expected.

- [ ] Release hygiene
  - Add `CHANGELOG.md` and `CONTRIBUTING.md` with a versioning policy (SemVer).
  - Add a release checklist: build, tests, pack, sign (optional), publish.

- [ ] Observable error semantics (decision)
  - Decide and document whether reducer/middleware exceptions should result in `OnError` or be handled by
      middleware/callers.
  - Implement and test the chosen behavior.

- [ ] Performance & robustness
  - Benchmark dispatch performance; optimize if necessary.
  - Audit `SerializeState` and any reflection/serialization choices for production use.

- [ ] Optional ergonomics
  - Add immutability helpers and testing helpers (unit testing utilities to assert state transitions).

Use this file as the canonical publishing checklist — I can take any item and implement it next.
