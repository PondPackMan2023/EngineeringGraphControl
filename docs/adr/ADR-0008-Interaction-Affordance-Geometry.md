# ADR-0008 — Interaction Affordance Geometry (Non-Space-Owning)

## Status
Accepted

## Context
Existing layout and rendering ADRs clearly define space ownership (ADR-0004 / ADR-0005) and axis identity and mapping invariants (ADR-0006). As interactive capabilities (hit-testing, context menus, interaction affordances) were introduced, it became clear that **interaction affordance geometry** represents a distinct category of abstract geometry not previously defined.

Attempts to model interaction affordances strictly within layout-owned axis bands proved insufficient for human interaction tolerance and created undesirable layout side effects. This revealed a missing abstraction rather than a flaw in existing ADRs.

## Decision
Introduce **Interaction Affordance Geometry** as a first-class abstract geometry concept that is:

- Expressed in normalized abstract geometry space
- Deterministic and renderer-agnostic
- Explicitly **non-space-owning**
- Explicitly **layout-independent**

Interaction affordance geometry exists solely to capture **user intent**, not to describe visual truth or to influence layout.

## Invariants

### 1. No Space Ownership
Interaction affordance geometry SHALL NOT:
- Own layout space
- Participate in band sizing or allocation
- Contribute to layout pressure resolution
- Cause plot area shrinkage or expansion

Layout ownership remains governed exclusively by ADR-0004 and ADR-0005.

### 2. Layout Independence
Interaction affordance geometry MAY:
- Overlap layout-owned geometry (axis bands, plot area)
- Extend beyond visual or layout bounds

Layout geometry SHALL NOT react to the presence of interaction affordances.

### 3. Orthogonality to Visual Geometry
Interaction affordance geometry:
- Is NOT required to match visual geometry
- May be larger or asymmetric relative to visual elements
- Must never depend on renderer output, pixels, DPI, or visual stroke measurements

Visual geometry remains governed by ADR-0003.

### 4. No Impact to Axis Identity or Mapping
Interaction affordance geometry SHALL NOT:
- Alter axis identity
- Alter axis inset semantics
- Alter axis stacking behavior
- Alter renderer mapping pipelines

Axis invariants remain governed exclusively by ADR-0006.

## Consequences

### Positive
- Enables reliable axis interaction without affecting layout
- Preserves deterministic layout and rendering behavior
- Avoids renderer-dependent heuristics
- Makes interaction tolerance explicit and testable

### Negative
- Introduces an additional abstract geometry category that must be understood by contributors

## Relationship to Other ADRs

- [ADR-0003: Unchanged (visual geometry)](ADR-0003-Graph-Presentation-Model-Abstract-Geometry-Plane.md)
- [ADR-0004: Unchanged (layout semantics)](ADR-0004-Graph-Presentation-Model-Layout-Semantics-and-Space-Ownership.md)
- [ADR-0005: Unchanged (layout pressure resolution)](ADR-0005-Layout-Pressure-Resolution-and-Degradation-Policy.md)
- [ADR-0006: Unchanged (axis identity and renderer mapping)](ADR-0006-Axis-Identity-Insets-And-Renderer-Mapping-Invariants.md)

ADR-0008 is strictly additive and resolves a previously undefined geometry responsibility.
