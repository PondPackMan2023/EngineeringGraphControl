# ADR-0003: GraphPresentationModel Abstract Geometry Plane

## Status
Accepted

## Context

The original OpenFlowsGraphControl and its successor, EngineeringGraphControl, intentionally separate **presentation geometry** from **rendering concerns**. This separation is documented in:

- *GraphPresentationModel_Design.md* (design-level intent)
- *ADR-0002: Graph Presentation Model Layering and Lifecycle*

These documents clearly establish:

- The GraphPresentationModel is renderer-agnostic
- It describes *what* is presented and *where*, not *how* it is drawn
- Rendering responsibilities (pixels, DPI, device transforms) belong exclusively to renderer implementations

However, neither the original design documentation nor ADR-0002 explicitly defines the **abstract geometric plane** on which the GraphPresentationModel operates.

As the EngineeringGraphControl evolved—particularly with more advanced legend, axis, and title interactions—this missing definition has led to implicit, inconsistent assumptions about geometry (e.g., axis direction, origin location, and relative ordering).

These inconsistencies manifested as layout defects that could not be reliably corrected through local fixes, revealing the need for a formally documented geometric contract.

## Decision

The GraphPresentationModel SHALL operate on a **single, explicit, abstract Cartesian geometry plane** with the following characteristics:

### Coordinate System

- The geometry plane is **Cartesian**.
- Coordinates are **normalized**, unless otherwise stated.

### Origin and Orientation

- **(0, 0)** is defined as the **bottom-left** corner of the presentation surface.
- **(1, 1)** is defined as the **upper-right** corner of the presentation surface.
- The **X-axis increases to the right**.
- The **Y-axis increases upward**, from bottom to top.

This coordinate system is **absolute and invariant** within the GraphPresentationModel.

### Semantic Interpretation

Within this geometry plane:

- “Above” means **greater Y**.
- “Below” means **lesser Y**.
- “Left of” means **lesser X**.
- “Right of” means **greater X**.

All geometric comparisons, min/max operations, adjacency calculations, and layout policies MUST be expressed consistently in these terms.

### Scope of Responsibility

The GraphPresentationModel:

- Produces **pure abstract geometry** (points, rectangles, bands, and relationships)
- Performs all layout reasoning using the defined Cartesian plane
- MUST NOT:
  - Perform pixel calculations
  - Make assumptions about screen coordinate orientation
  - Invert axes for rendering convenience
  - Compensate for renderer-specific behavior

Renderer implementations:

- Interpret PresentationModel geometry
- Translate abstract coordinates into device-specific coordinates
- Perform any required axis inversions (e.g., Y-down screen space)
- Handle DPI, scaling, clipping, and rasterization

Any transformation between PresentationModel space and device space MUST occur **at the renderer boundary**, exactly once.

## Consequences

### Positive

- Establishes a single, unambiguous geometric foundation
- Eliminates implicit coordinate assumptions
- Simplifies reasoning about layout behaviors (legends, axes, titles, plot area)
- Prevents cross-layer coupling between presentation logic and rendering
- Enables deterministic, testable layout outcomes
- Improves long-term maintainability and extensibility

### Required Follow-up Work

- Audit existing GraphPresentationModel layout logic against this coordinate convention
- Update documentation and code comments to reference this ADR explicitly
- Ensure all new features and fixes adhere strictly to the defined geometry plane
- Where necessary, adjust renderer implementations to perform explicit coordinate translation

## Related Decisions

- [ADR-0002: Graph Presentation Model Layering and Lifecycle](ADR-0002-Graph-Presentation-Model-Layering-and-Lifecycle.md)

## Notes

This ADR intentionally focuses only on **geometric definition and responsibility boundaries**. It does not prescribe specific layout algorithms or policies, nor does it constrain renderer implementations beyond the requirement to respect the PresentationModel’s abstract geometry contract.

This decision is foundational. All subsequent presentation and layout behavior is expected to build upon it.
