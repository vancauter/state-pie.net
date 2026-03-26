# Reducer Guidelines

- Rule: return the same instance when no meaningful change occurred.
- Prefer `return state;` for no-op branches to preserve identity.
- Avoid allocating equal-but-new objects in no-op paths; this triggers publications.
- For composite reducers, ensure each slice returns the original slice instance when unchanged.
- Tests: include `Reducer_NoOp_PreservesReference` and `Reducer_NewButValueEqual_TriggersChange` examples.
