```yaml
description: "General contributor guidance (machine-friendly)"
metadata:
	projectStatus: Development
guidance:
	sourceHygiene:
		- "Always pull the latest main/trunk before making changes."
		- "Run `dotnet build` and `dotnet test` locally before committing."
	apiContracts:
		- "Verify public API signatures remain compatible; update tests/docs when changing them."
	formatting:
		- "Follow `.editorconfig` for code style; use consistent naming and formatting."
		- "Prefer small, focused PRs with tests."
	threading:
		- "Be explicit about thread-safety and document synchronization expectations."
	validation:
		- "Use clear exception types and messages for invalid arguments."
	tests:
		- "Follow existing test naming/style; run `dotnet test`."
	misc:
		- "Use clear, maintainable code and document non-obvious decisions."
```

## Human-readable summary

See the YAML above for structured guidance. In short: keep branches small, run the build/tests locally, follow `.editorconfig`, and add tests for behavior changes.
