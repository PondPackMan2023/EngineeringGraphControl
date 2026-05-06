# ADR-0009: Control-Level Interaction Overlays with Rendered-Geometry Feedback

- **Status:** Accepted
- **Date:** 2026-05-06
- **Deciders:** Graphing / UI Architecture Team
- **Related ADRs:** [ADR-0003 (Abstract Geometry)](ADR-0003-Graph-Presentation-Model-Abstract-Geometry-Plane.md), [ADR-0004 (Layout Semantics)](ADR-0004-Graph-Presentation-Model-Layout-Semantics-and-Space-Ownership.md), [ADR-0005 (Layout Degradation)](ADR-0005-Layout-Pressure-Resolution-and-Degradation-Policy.md)

---

## Context

The Animation Bar feature introduced a class of **interactive, non-exportable UI overlays** that are rendered only in the WinForms control layer and are driven by user interaction (dragging, snapping, hover feedback).

As the feature evolved, it became clear that certain visual behaviors—specifically *intersection markers between the animation bar and data series*—could not be implemented correctly using domain-space data alone, especially in the presence of:

- Multiple Y-axes with differing scales
- Differing series start/end extents
- Future zoom and scaling support
- Discrete point/scatter series vs continuous line series

Initial index-based and abstract-space approaches produced visually incorrect results. Correct behavior required computing intersections against the **actual rendered geometry** used to draw each series.

This ADR formalizes the architectural pattern required to support such features while preserving existing renderer and model boundaries.

---

## Decision

We adopt the following architectural decision:

> **Certain interaction overlays may require read-only access to ephemeral, device-space rendered geometry exposed by UI renderers, strictly for control-level overlay computation.**

Key elements of the decision:

1. **Rendered Geometry Exposure (UI-only)**
   - UI renderers (e.g., WinForms) may expose *read-only, per-render* geometry snapshots (e.g., device-space polylines).
   - This exposure is ephemeral and valid only during a single paint pass.

2. **One-Way Dependency**
   - Geometry flows from renderer → control only.
   - Renderers remain unaware of overlays, interaction semantics, or consuming features.

3. **Control-Level Ownership**
   - All interaction logic (snapping, intersection resolution, marker rendering) remains owned by the control.
   - Presentation models remain unchanged and unaware of interaction overlays.

4. **Renderer Isolation Preserved**
   - Export renderers (Bitmap / Metafile) are not required to expose geometry.
   - Export output must remain unaffected by interaction overlays.

5. **Series-Type Semantics**
   - Continuous series (Line) use geometric intersection against rendered polylines.
   - Discrete series (Point / Scatter) use center-based snapping semantics, with the animation bar itself snapping to point centers.

---

## Rationale

This approach allows interaction overlays to be:

- **Visually correct** (aligned with what the user actually sees)
- **Architecturally contained** (no presentation-model pollution)
- **Extensible** (future cursors, tooltips, ranges, zoom helpers)
- **Export-safe** (no impact on non-UI render paths)

The alternative—attempting to reconstruct or approximate renderer geometry in the control—was deemed fragile and error-prone, especially as rendering behavior evolves.

This pattern matches established behavior in professional charting engines and is necessary to meet user expectations for advanced interactive features.

---

## Consequences

### Positive

- Enables correct interaction behavior in multi-axis and scaled scenarios
- Unlocks future advanced UI overlays without architectural refactoring
- Maintains strict separation between data, presentation, rendering, and interaction

### Trade-offs

- UI renderers have a slightly expanded responsibility
- Requires discipline to ensure geometry exposure remains read-only and ephemeral

These trade-offs are acceptable and explicitly constrained by this ADR.

---

## Scope

This ADR applies to:

- Control-level, non-exportable interaction overlays
- Features requiring alignment with *rendered* visuals rather than data abstractions

This ADR does **not** apply to:

- Data series modeling
- Presentation-model geometry
- Export or headless rendering paths

---

## Related Work

- Animation Bar feature (AB-0 through AB-3e)
- Intersection marker overlays
- Future cursor, tooltip, and range-selection features

---

## Status

This ADR is **Accepted** and in effect as of completion of the Animation Bar feature.
