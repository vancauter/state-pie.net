---
name: csharp-implement-design
description:
    Use when implementing a preplanned design document in C#/.NET. Supply the design document and any relevant code or
    context for the implementation task.
argument-hint: Supply the design document and any relevant code or context for the implementation task.
agent: csharp-design-implementation.agent
model: GPT-5 mini
---

This prompt is used to implement a preplanned design document in C#/.NET. If no design document is supplied:

- Check if there is a file attachment that is a design document (e.g., named `*.design.md` or similar). If so, use that
  as the design document for the implementation task.
- Otherwise, ask the user to provide the design document and any relevant code or context for the implementation task
  and stop.
