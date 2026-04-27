# ADR-0003: Graph Presentation Model Layering and Lifecycle

## Status
Accepted (inherited and enforced by EngineeringGraphControl)

## Context
EngineeringGraphControl adopts the same architectural layering and lifecycle model
originally introduced in OpenFlowsGraphControl. This ADR is intentionally retained
because the underlying architectural concerns remain the same:

- Clear separation between domain data, presentation semantics, and rendering
- Immutable model and snapshot lifecycles
- Explicit rebuilds rather than incremental mutation

Recent work in EngineeringGraphControl (axis unit changes, snapshot rebuilding,
and title derivation) has reaffirmed the validity of this decision.

## Decision
The graph system is composed of three strictly-layered stages:

1. **GraphModel** – the immutable, semantic description of a graph
2. **GraphSnapshot** – a concrete, read-only projection of the model suitable for rendering
3. **GraphPresentationModel** – device-agnostic presentation primitives

All transitions between layers occur via full rebuilds. No layer mutates objects
owned by another layer.

## Consequences
- Graph updates are atomic and predictable
- Presentation behavior is testable and deterministic
- Axis semantics (units, titles) are owned by axis models and snapshots, not fields

This ADR is binding for EngineeringGraphControl and should not be changed without
introducing a new ADR.
