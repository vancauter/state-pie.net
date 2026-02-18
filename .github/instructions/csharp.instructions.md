```yaml
description: "C#-specific guidelines (machine-friendly)"
applyTo: "**/*.cs"
metadata:
	sdkDetected: "8.0.418"
	previewFeaturesUsed: true
	previewFeatureNotes: "Collection expressions (C# 15): collection literals, spread (..), with(...) args"
settings:
	nullableEnabled: true
	recommendLangVersion: "preview"
guidance:
	- id: tooling
		title: Tooling and language
		bullets:
			- "This repo uses C# collection-expression features; preserve them where present."
			- "Contributors without preview SDKs can install the appropriate preview (e.g., .NET 11 preview) or convert locally to conventional initializers."
			- "Optional: pin SDK with global.json or set <LangVersion>preview in projects for reproducible builds."
	- id: nullability
		title: Nullable reference types
		bullets:
			- "Project enables <Nullable>enable. Trust compiler annotations and avoid redundant null checks where types indicate non-nullability."
			- "Prefer 'is null' / 'is not null' for runtime checks."
	- id: formatting
		title: Formatting
		bullets:
			- "Use .editorconfig formatting; run 'dotnet format' before commits. Respect .prettierrc when present for rules like line length and wrapping."
			- "Use nameof(...) instead of raw strings for member names."
examples:
	- description: Collection expression examples (C# 15)
		code: |
			string[] values = ["one", "two", "three"];
			List<string> names = [with(capacity: values.Length * 2), .. values];
			HashSet<string> set = [with(StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"];
```

# C# Development (human-readable)

This file contains C#-specific guidance (nullable rules, C# 15 collection-expression notes, and examples). General contributor guidance (formatting, CI, tests, thread-safety) is maintained in `.github/copilot-instructions.md` to avoid duplication.
