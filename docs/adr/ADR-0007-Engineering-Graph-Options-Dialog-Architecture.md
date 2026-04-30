# ADR-007: Architecture of the Engineering Graph Options Dialog

**Status:** Accepted  **Date:** 2026-04-30  **Deciders:** Graphing / Engineering Tools Team

---

## Context

Multiple engineering products require a consistent, extensible mechanism for editing graph *presentation* settings, such as titles, axes formatting, series visibility, and legend placement.

Key constraints and requirements for this solution include:

- Target frameworks include **.NET Framework 4.8** and **.NET 6.0 (Windows)**
- UI technology is **WinForms**, with full design-time support required
- Graph presentation settings must be editable without mutating the graph or domain model directly
- Users must be able to cancel edits without side effects
- Consumers must retain control over whether and when updated options are applied
- The solution must support future extension without destabilizing existing behavior

Earlier implementations and prototypes exposed UI controls directly to domain or options objects, leading to unclear ownership, difficult cancel semantics, and limited testability.

---

## Decision

We introduced a layered architecture for the Engineering Graph Options Dialog with the following core decisions:

1. **Editor Models** are used to represent mutable, UI-facing state for each presentation concern (Titles, Series, Axes, Legend).
2. A central **GraphOptionsPresentationModel** orchestrates editor models and manages translation between immutable options and editable state.
3. The dialog follows an **immutable-in / mutable-edit / immutable-out** workflow.
4. The WinForms dialog exposes a **single static entry point** for consumers.
5. The dialog never applies options itself; the caller retains full control.

These decisions collectively define the architecture of the Options Dialog.

---

## Key Architectural Decisions

### 1. Separation via Editor Models

Rather than binding UI controls directly to `GraphPresentationOptions` or domain objects, we introduced dedicated editor models, each responsible for one presentation concern.

**Rationale:**
- Prevents accidental mutation of live graph state
- Enables clean cancel semantics
- Provides a stable surface for UI binding
- Improves unit testability

---

### 2. GraphOptionsPresentationModel as Orchestrator

All editor models are owned and constructed by a single presentation model:

```
GraphOptionsPresentationModel
```

This model:
- Reads from `IGraphModel` and existing `GraphPresentationOptions`
- Constructs all editor models
- Produces a new `GraphPresentationOptions` via `BuildGraphPresentationOptions()`

**Rationale:**
- Centralizes option translation logic
- Keeps UI controls ignorant of persistence and domain concerns
- Provides a single seam for unit testing and future validation

---

### 3. Immutable-in / Mutable-edit / Immutable-out

The dialog lifecycle follows a strict pattern:

1. Existing options are treated as immutable input
2. Editor models are freely mutated during the dialog session
3. A new options instance is created only when the user confirms

**Rationale:**
- Prevents partial mutation
- Simplifies reasoning about Cancel vs OK
- Enables safe reuse of options objects

---

### 4. Static Dialog Entry Point

The dialog exposes a single static method:

```csharp
GraphPresentationOptions OpenOptions(
    IGraphModel graphModel,
    GraphPresentationOptions existingOptions,
    IWin32Window ownerWindow = null)
```

**Behavior:**
- Returns a new options instance when the user selects OK
- Returns the original options instance unchanged on Cancel
- Never returns null

**Rationale:**
- Simplifies consumption
- Avoids nullable return contracts (target framework compatibility)
- Hides dialog wiring and presentation model construction
- Aligns with idiomatic WinForms usage

---

### 5. Dialog Does Not Apply Options

The dialog is strictly responsible for editing and returning options. It does not apply changes to the graph or control.

**Rationale:**
- Preserves separation of concerns
- Allows callers to decide timing and conditions for application
- Supports consumers that want to discard, preview, or defer application

---

## Alternatives Considered

### Direct UI Binding to GraphPresentationOptions

**Rejected** due to:
- Difficult cancel semantics
- Tight coupling between UI and persistence
- Poor testability

---

### Returning DialogResult or Using `out` Parameters

**Rejected** in favor of returning the options instance directly:
- Reduces boilerplate
- Avoids nullable patterns
- Places decision-making at the call site

---

### Adding Font and Unit Selection in Initial Implementation

**Deferred** intentionally:
- Fonts are currently renderer-owned
- Units may require domain-level decisions
- Avoids half-persisted UI features

---

## Consequences

### Positive

- Clear ownership boundaries between UI, models, and domain
- Safe cancel semantics with no side effects
- Fully testable without UI automation
- Easy consumption via a single API
- Extensible design for future presentation features

### Trade-offs

- Additional types compared to direct UI binding
- Slightly higher upfront complexity

These trade-offs were accepted in favor of long-term maintainability and correctness.

---

## Result

The Engineering Graph Options Dialog is implemented as a robust, extensible, and consumer-friendly feature. The architecture ensures correctness, testability, and future evolution without breaking existing behavior.

This ADR documents the rationale behind the chosen design and should be referenced alongside the feature design documentation and implementation.
