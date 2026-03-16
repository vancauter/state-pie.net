**Task:** Perform a comprehensive documentation and comment-refinement pass on all the source code (./src).

**1. Public API (XML Documentation):**

- Apply full XML documentation to all **public** and **protected** members (classes, methods, properties, fields).
- Include `<summary>`, `<param>`, `<returns>` and `<exception>` tags.
- **Exceptions:** Explicitly add `<exception cref="...">` tags for every exception that can be intentionally thrown by a method. Describe the specific condition that triggers the exception.
- Use `<remarks>` for any non-obvious behavior, thread-safety guarantees, or usage constraints.
- Ensure all `<see cref="..."/>` tags correctly point to related types or methods to enable IDE cross-referencing.

**2. Internal Implementation (Inline Comments):**

- Provide XML documentation `<summary>` only for **internal**, **private**, and **protected** members.
- Provide inline comments only for non-trivial logic or "Why" decisions.
- **STRICT RULE:** Omit comments for self-explanatory code (e.g., simple assignments, standard loops, or basic instantiation).
- Focus on explaining the intent of a code block rather than restating the syntax.

**3. Quality Control & Revision:**

- Audit all existing comments. Remove or rewrite any that are redundant, outdated, or poorly structured.
- Standardize all text to follow professional C# conventions (proper casing, punctuation, and clear phrasing).
- Verify that XML tags perfectly match the current method signatures (parameters, return types, and generic type arguments).

**4. Philosophy:**

- Aim for a "clean-code" aesthetic. The documentation should provide maximum utility with minimum visual noise.

---
