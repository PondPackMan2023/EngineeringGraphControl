## Architecture Decision Record (ADR)

### ADR Number

ADR-0011

### Title

Pie Graph Architecture and Interaction Model

### Status

Accepted — July 2026

### Context

EngineeringGraphControl originally provided Cartesian graph visualization using a layered architecture consisting of Model, Snapshot, Presentation, Renderer, and Control layers.

A reusable Pie Graph framework was required to support financial, reporting, and business visualization scenarios while preserving the architectural principles already established within EngineeringGraphControl.

The implementation needed to:

- Remain framework-agnostic
- Support binding-first WPF integration
- Preserve testability
- Provide stable immutable identity
- Support future interaction features
- Avoid coupling presentation, rendering, and interaction behavior

Consumer applications require stable slice identity for:

- Tooltips
- Commands
- Navigation
- Drill-down workflows

Identity must remain independent from display labels.

### Decision

The Pie Graph framework SHALL use the same layered architecture as existing Cartesian graphing infrastructure:

```text
Model
 -> Snapshot
 -> Presentation
 -> Renderer
 -> Control
```

The framework introduces:

```text
PieSliceId
```

as the stable immutable identifier for pie slices.

Identity flows through:

```text
PieSliceModel
 -> PieSliceSnapshot
 -> PieSlicePresentationGeometry
```

unchanged.

The framework provides:

- Pie rendering
- Legend generation
- Presentation options
- Hit testing
- Tooltips
- Consumer-driven command support

The WPF control exposes:

```text
PieSliceDoubleClickCommand
```

for consumer interaction handling.

Application-specific navigation behavior remains outside the graphing framework.

### Rationale

This approach:

- Aligns Pie Graphs with existing EngineeringGraphControl architecture
- Preserves separation of concerns
- Improves testability
- Enables reusable interaction infrastructure
- Supports stable immutable identity
- Creates a consistent mental model across graph types

The introduction of:

```text
IdentityBase<TIdentity, TValue>
```

provides a reusable identity foundation for future framework identifiers.

### Alternatives Considered

#### Direct Renderer-Based Architecture

Render pie slices directly from model data.

Pros:

- Simpler initial implementation

Cons:

- Reduced testability
- Difficult interaction support
- Poor alignment with existing graphing architecture

Rejected.

#### Label-Based Slice Identification

Use display labels as interaction identifiers.

Pros:

- Minimal implementation effort

Cons:

- Labels are mutable
- Labels are presentation data
- Poor support for navigation and drill-down scenarios

Rejected.

#### Application-Specific Interaction Logic

Allow applications to inject behavior directly into the framework.

Pros:

- Flexible

Cons:

- Creates application coupling
- Reduces framework reusability

Rejected.

### Consequences

#### Positive

- Consistent graph architecture
- Stable immutable identity
- Consumer-driven interactions
- Improved testability
- Reusable hit-testing infrastructure
- Future extensibility

#### Negative

- Additional infrastructure layers
- Slightly increased implementation complexity
- Increased number of framework types

### Follow-Up Work

Potential future enhancements:

- Additional interaction gestures
- Keyboard accessibility
- Slice highlighting
- Additional command surfaces
- IdentityBase adoption by additional graph identifiers

### Constraints and Rules (Optional)

- PieSliceId MUST remain immutable.
- Hit testing MUST operate on presentation geometry.
- Renderers MUST remain rendering-focused.
- Consumer commands MUST remain application-agnostic.
- Display labels MUST NOT be used as slice identity.

### References

- ADR-0001 Graph Presentation Model Layering and Lifecycle
- ADR-0003 Graph Presentation Model Abstract Geometry Plane
- WPF EngineeringPieGraphControl Binding Contract

### Summary

EngineeringGraphControl now includes a fully layered Pie Graph framework supporting rendering, identity, presentation, hit testing, tooltips, and consumer-driven command interactions while preserving the architectural principles established by the existing graphing system.
