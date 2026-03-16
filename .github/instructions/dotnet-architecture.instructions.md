---
name: "dotnet-architecture"
description: "DDD and .NET architecture guidelines"
applyTo: "**/*.cs,**/*.csproj,**/Program.cs
---

# DDD Systems and .NET Guidelines

Apply these instructions when working on C# and .NET code in this repository. Favor Domain-Driven Design, SOLID
principles, and maintainable .NET architecture.

## Mandatory Workflow

Before any implementation, you must complete all of the following:

1. Show your analysis.
    - Explain which DDD patterns and SOLID principles apply.
    - Identify which layer or layers are affected: Domain, Application, or Infrastructure.
    - Explain how the solution aligns with ubiquitous language.
    - Call out security and compliance considerations.
2. Review the design against these guidelines.
    - Confirm whether aggregate boundaries are respected.
    - Confirm whether the design adheres to the Single Responsibility Principle.
    - Confirm whether domain rules are encapsulated correctly.
    - Confirm whether tests will follow the `MethodName_Condition_ExpectedResult()` pattern.
    - Confirm whether coding domain considerations are addressed.
    - Confirm whether ubiquitous language remains consistent.
3. Validate the implementation plan before coding.
    - State which aggregates or entities will be created or modified.
    - State which domain events will be published.
    - State how interfaces and classes will be structured according to SOLID principles.
    - State which tests will be needed and how they will be named.

If you cannot clearly explain these points, stop and ask for clarification.

## Core Principles

### Domain-Driven Design

- Use consistent business terminology across code and documentation.
- Keep bounded contexts and responsibilities explicit.
- Treat aggregates as consistency and transaction boundaries.
- Capture business-significant occurrences as domain events.
- Keep business logic in the domain layer rather than in application services.

### SOLID Principles

- Single Responsibility Principle: a class should have one reason to change.
- Open/Closed Principle: extend behavior without rewriting stable behavior.
- Liskov Substitution Principle: derived types must remain substitutable.
- Interface Segregation Principle: avoid forcing consumers to depend on unused members.
- Dependency Inversion Principle: depend on abstractions, not concretions.

### .NET Good Practices

- Use `async` and `await` for I/O-bound operations.
- Use dependency injection to keep code loosely coupled and testable.
- Use LINQ for expressive, readable data manipulation where appropriate.
- Apply clear, consistent exception-handling and logging strategies.
- Prefer modern C# features such as records and pattern matching when they improve clarity and robustness.

### Security and Compliance

- Implement authorization at the aggregate level.
- Address PCI-DSS and SOX requirements in financial domain rules.
- Use domain events to support complete audit history.
- Design aggregates with LGPD data-protection requirements in mind.

### Performance and Scalability

- Prefer non-blocking processing with `async` and `await`.
- Use efficient queries and indexing strategies.
- Cache only where volatility and correctness allow it.
- Keep aggregates and value objects appropriately sized.

## Layer Standards

### Domain Layer

- Aggregates are root entities that maintain consistency boundaries.
- Value objects are immutable representations of domain concepts.
- Domain services are stateless and encapsulate business operations spanning aggregates.
- Domain events capture meaningful state changes.
- Specifications encapsulate complex business rules and queries.

### Application Layer

- Application services orchestrate domain operations and infrastructure collaboration.
- DTOs transfer data across layers and process boundaries.
- Validate all incoming data before business logic executes.
- Use constructor injection for dependencies.

### Infrastructure Layer

- Repositories persist and retrieve aggregates through domain-facing interfaces.
- Event buses publish and subscribe to domain events.
- Data mappers or ORMs map domain objects to storage models.
- External service adapters integrate outside systems.

## Testing Standards

- Use the `MethodName_Condition_ExpectedResult()` naming convention.
- Focus unit tests on isolated domain logic and business rules.
- Use integration tests for persistence, boundaries, and service integrations.
- Use acceptance tests for complete user scenarios.
- Target at least 85% coverage for domain and application layers.

### Test Structure

```csharp
[Fact(DisplayName = "Descriptive test scenario")]
public void MethodName_Condition_ExpectedResult()
{
    // Setup for the test.
    var aggregate = CreateTestAggregate();
    var parameters = new TestParameters();

    // Execute the method under test.
    var result = aggregate.PerformAction(parameters);

    // Verify the outcome.
    Assert.NotNull(result);
    Assert.Equal(expectedValue, result.Value);
}
```

### Domain Test Categories

- Aggregate tests validate business rules and state changes.
- Value object tests validate immutability and equality.
- Domain service tests validate complex business operations.
- Event tests validate publishing and handling.
- Application service tests validate orchestration and input validation.

### Test Validation Process

Before writing any test, you must:

1. Verify the naming pattern: `MethodName_Condition_ExpectedResult()`.
2. Confirm the test category: unit, integration, or acceptance.
3. Confirm the test validates actual business rules.
4. Review edge cases, including error scenarios and boundary conditions.

## Implementation Process

Follow this process for every significant implementation.

### Step 1: Domain Analysis

Explicitly state:

- The domain concepts involved and their relationships.
- The aggregate boundaries and required consistency.
- The ubiquitous language terms in use.
- The business rules and invariants to enforce.

### Step 2: Architecture Review

Explicitly validate:

- How responsibilities are assigned to each layer.
- Adherence to SOLID principles, especially SRP and DIP.
- How domain events support decoupling.
- Security implications at aggregate boundaries.

### Step 3: Implementation Planning

Explicitly outline:

- Which files will be created or modified, and why.
- Which tests will be added, using the `MethodName_Condition_ExpectedResult()` pattern.
- The error-handling and validation strategy.
- The relevant performance and scalability considerations.

### Step 4: Implementation Execution

1. Start with domain modeling and ubiquitous language.
2. Define aggregate boundaries and consistency rules.
3. Implement application services with proper input validation.
4. Apply .NET practices such as async programming and dependency injection.
5. Add comprehensive tests following the naming convention.
6. Publish domain events where loose coupling is appropriate.
7. Document domain decisions and trade-offs.

### Step 5: Post-Implementation Review

Verify all of the following:

- Quality checklist items are met.
- Tests follow naming conventions and cover edge cases.
- Domain rules are encapsulated properly.
- Financial calculations maintain precision.
- Security and compliance requirements are satisfied.

## Quality Checklist

Before delivering code, explicitly confirm each item.

### Domain Design Validation

- I have verified that aggregates properly model business concepts.
- I have confirmed consistent terminology throughout the codebase.
- I have verified the design follows SOLID principles.
- I have validated that domain logic is encapsulated in aggregates.
- I have confirmed domain events are properly published and handled.

### Implementation Quality Validation

- I have written comprehensive tests following `MethodName_Condition_ExpectedResult()` naming.
- I have considered performance implications and ensured efficient processing.
- I have implemented authorization at aggregate boundaries.
- I have documented domain decisions and architectural choices.
- I have followed .NET best practices for async, DI, and error handling.

### Financial Domain Validation

- I have used `decimal` types and proper rounding for financial calculations.
- I have ensured proper transaction boundaries and consistency.
- I have implemented complete audit capabilities through domain events.
- I have addressed PCI-DSS, SOX, and LGPD requirements.

If any item cannot be confirmed with certainty, explain why and request guidance.

## Financial Domain Rules

### Monetary Values

- Use `decimal` for all monetary calculations.
- Implement currency-aware value objects.
- Handle rounding according to financial standards.
- Maintain precision throughout calculation chains.

### Transaction Processing

- Use saga patterns for distributed transactions where needed.
- Use domain events for eventual consistency.
- Maintain strong consistency within aggregate boundaries.
- Use compensation patterns for rollback scenarios.

### Audit and Compliance

- Capture financial operations as domain events.
- Implement immutable audit trails.
- Design aggregates to support regulatory reporting.
- Maintain data lineage for compliance audits.

### Financial Calculations

- Encapsulate calculation logic in domain services.
- Implement proper validation for financial rules.
- Use specifications for complex business criteria.
- Maintain calculation history for audit purposes.

### Platform Integration

- Use system standard DDD libraries and frameworks.
- Implement proper bounded context integration.
- Maintain backward compatibility in public contracts.
- Use domain events for cross-context communication.

**Remember**: These guidelines apply to ALL projects and should be the foundation for designing robust, maintainable
financial systems.

## CRITICAL REMINDERS

**YOU MUST ALWAYS:**

- Show your thinking process before implementing.
- Explicitly validate against these guidelines.
- Use the mandatory verification statements.
- Follow the `MethodName_Condition_ExpectedResult()` test naming pattern.
- Confirm financial domain considerations are addressed.
- Stop and ask for clarification if any guideline is unclear.

**FAILURE TO FOLLOW THIS PROCESS IS UNACCEPTABLE** - The user expects rigorous adherence to these guidelines and code
standards.
