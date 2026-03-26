---
name: total-refactor-plan
description: Design a complete, modernized architectural refactor plan for the repository using deep research.
agent: deep-research.agent
model: GPT-5 mini
---

You are responsible for producing a **comprehensive architectural refactor plan** for this repository. The outcome must
be a **fully defined target architecture** and a **step-by-step transformation plan**.

---

# Objectives

- Perform **mandatory deep research** into modern .NET architecture and best practices.
- Design a **modular, SOLID-compliant architecture**.
- Maximize **maintainability, testability, scalability, and observability**.
- Apply **clear structural improvements**, including renaming, splitting, and reorganizing code.
- Deliver a **complete design document** and **migration plan**.

---

# Phase 1 — Research (Required)

Conduct thorough research before making any design decisions. Base all conclusions on established best practices.

Focus areas:

- Modern .NET architectural patterns (e.g., Clean Architecture, Vertical Slice, Modular Monolith)
- SOLID principles in real-world systems
- Dependency Injection and Dependency Inversion strategies
- Functional design patterns (pure/static functions, immutability)
- Project and solution structuring in medium-to-large .NET codebases
- Testing strategies (unit, integration, boundary testing)
- Observability (logging, tracing, metrics)

Use research insights to justify all major architectural decisions.

---

# Phase 2 — Repository Analysis

Perform a structural analysis of the current codebase:

- Read all files under `./src`
- Infer responsibilities from:
    - Directory structure
    - File names
    - Class and method names

Identify:

- Architectural smells
- Violations of SOLID principles
- Tight coupling and poor abstractions
- Overly large or ambiguous modules
- Opportunities for functional decomposition

---

# Phase 3 — Target Architecture Design

Design the desired end-state architecture.

## Core requirements

- Enforce **SOLID principles**
- Prefer **composition over inheritance**
- Apply **Dependency Inversion** across boundaries
- Use **pure/static functions** where determinism is possible
- Keep **interfaces small, cohesive, and domain-focused**
- Clearly separate **domain, application, and infrastructure concerns**

## Structural expectations

- Introduce a **logical and scalable project/folder structure**
- Split monolithic components into **well-defined modules**
- Group interfaces by **subject/domain**, not by technical type
- Rename files, classes, and directories to **industry-standard terminology**
- Remove or replace poor abstractions

## Project Boundaries & Constraints

- Minimize the number of new projects in the solution.
- Do **not** introduce new projects unless there is a clear architectural boundary that cannot be achieved within the
  existing structure.s
- Prefer **modularization within a project** (folders, namespaces, internal boundaries) over splitting into multiple
  projects.
- Only introduce a new project when it provides clear benefits in:
    - Independent deployment or versioning
    - Strong isolation of infrastructure concerns
    - Compile-time boundary enforcement that cannot be achieved otherwise
- Consider the **size and complexity of the codebase** before proposing additional projects.
- Avoid over-engineering through premature decomposition.

You are allowed to make **breaking changes** without constraints.

---

# Phase 4 — Refactor Plan (Execution Strategy)

Define all steps required to transition from the current state to the target architecture.

This must include:

- File and directory restructuring
- Renaming and moving code
- Refactoring logic and abstractions
- Updating dependency wiring
- Updating and fixing all tests
- Updating developer tooling and examples
- Ensuring build and test pipelines remain functional

Constraints:

- Do **not** include migration guides or backward compatibility layers
- Focus only on achieving the clean end state

---

# Phase 5 — Deliverables

## Versioning

- Choose the next logical semantic version (e.g., `v0.3.0`)
- Use this version consistently across all outputs

## Design Document

Create: `docs/design/<name>-design-v<version>.md`

## Requirements

- Follow the structure and naming conventions of existing design documents
- Include:

## 1. Architectural Overview

- High-level explanation of the new architecture
- Key design decisions with justification (based on research)

## 2. Project Structure

- Full proposed directory and project layout

## 3. Module Breakdown

- Responsibilities and boundaries of each module

## 4. Design Principles Applied

- How SOLID, modularity, and functional design are enforced

## 5. Refactor Plan

- Step-by-step transformation plan

## 6. File Manifest (Mandatory)

- Explicit list of:
    - Renamed files
    - Moved files
    - Deleted files
    - Newly introduced files

## 7. System Guarantees

- Build succeeds
- Tests pass
- Developer workflows remain operational

---

# Final Step (Mandatory)

After the refactor plan is complete:

- Update all `SKILL.md` files
- Update repository-level documentation
- Update public API references

---

# Execution Requirements

- Think deeply before producing output
- Base decisions on research, not assumptions
- Favor clarity and determinism over verbosity
- Produce a **complete, implementation-ready design**
