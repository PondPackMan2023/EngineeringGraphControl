# ADR-0010: Zoom Interaction Semantics for EngineeringGraphControl

- **Status:** Accepted
- **Date:** 2026-05-06
- **Deciders:** Graphing / UI Architecture Team
- **Related ADRs:**
  - [ADR-0009 – Control-Level Interaction Overlays with Rendered-Geometry Feedback](ADR-0009-Control-Level-Interaction-Overlays.md)

---

## Context

`EngineeringGraphControl` historically relied on the TeeChart control for interactive zoom behavior. Users of OpenFlows products have long-established expectations for how zoom operates, particularly in graphs with multiple Y-axes.

As native zoom support was implemented directly in `EngineeringGraphControl`, several previously implicit behaviors required explicit definition, including:

- What constitutes a zoom gesture
- How zoom interacts with multiple Y-axes
- How axis ranges are adjusted
- How zoom reset behaves

These behaviors are user-visible, foundational, and must remain stable over time to ensure consistent interaction semantics across products.

This ADR formalizes the **semantic contract** for zoom behavior, independent of specific implementation details.

---

## Decision

We adopt the following zoom semantics for `EngineeringGraphControl`:

> **Zoom is defined exclusively as mutation of axis minimum and maximum values in response to a user-defined zoom rectangle and gesture semantics.**

Zoom behavior is implemented entirely at the control level, builds on the architectural pattern defined in ADR-0009, and introduces no new rendering or presentation-model responsibilities.

---

## Zoom Enablement

- Zoom support is **opt-in** and controlled by the consumer via a boolean property:
  - `ZoomEnabled`
- Zoom is disabled by default.
- When disabled, all zoom interaction logic is inactive.

---

## Gesture Semantics

Zoom gesture interpretation matches TeeChart / OpenFlows expectations:

| Drag Direction | Semantics |
|---------------|-----------|
| Down + Right | Zoom-in |
| Up + Left | Zoom extents (reset) |
| Any other direction | No operation |

Gesture semantics are resolved on mouse release and do not depend on modifier keys.

---

## Zoom Rectangle

- A rubber-band zoom rectangle provides visual feedback during drag interaction.
- The rectangle is drawn as a dotted outline and clipped to the plot area.
- The rectangle is a **control-level, non-exportable overlay**.

The rectangle defines the candidate region for zoom computation; it does not directly manipulate axes.

---

## Axis Zoom Semantics

### X-Axis

- X-axis zoom is **global**.
- On a zoom-in gesture, the X-axis minimum and maximum are set to the domain values corresponding to the rectangle’s horizontal bounds.
- X-axis zoom is applied once per gesture.

### Y-Axes (Multi-Axis Semantics)

- Each visible Y-axis is evaluated **independently**.
- A Y-axis participates in zoom **only if** the zoom rectangle vertically overlaps that axis’s plot region.
- For participating axes:
  - Domain Y minimum and maximum are computed from the overlapping rectangle segment
  - Each axis computes its own independent range
- Y-axes with no vertical overlap remain unchanged.

This design allows:
- Partial zoom affecting only some axes
- Independent zoom ranges per axis

---

## Zoom Reset Semantics

- A zoom-reset gesture invokes a single control API:
  - `ZoomExtents()`
- `ZoomExtents()` restores **all axes (X and all Y)** to their captured default ranges.
- Zoom reset does not depend on current zoom state or axis participation.

---

## Guards and Edge Cases

- Degenerate zoom rectangles (near-zero width or height) do not alter affected axes.
- Degeneracy is evaluated per axis:
  - X-axis zoom may apply even if Y zoom is skipped
  - A Y-axis is skipped independently if its overlap is too small

---

## Architectural Constraints

- Zoom logic resides entirely within `EngineeringGraphControl`.
- Axis min/max mutation is the **only zoom mechanism**.
- Renderers remain unaware of zoom semantics.
- Presentation models are not modified.
- Export (Bitmap / Metafile) behavior is unaffected.

These constraints are consistent with ADR-0009.

---

## Rationale

Formalizing zoom semantics as an ADR:

- Captures a long-standing user interaction contract
- Prevents semantic drift across future enhancements
- Enables safe extension (undo, animation, history) without reinterpretation
- Avoids future reverse-engineering of intent from implementation details

---

## Consequences

### Positive

- Predictable and consistent zoom behavior across products
- Correct handling of multi–Y-axis graphs
- Strong separation between interaction, rendering, and data

### Trade-offs

- Multi-axis zoom logic is more complex than single-axis zoom
- Requires careful testing to ensure per-axis independence

These trade-offs are acceptable and intentional.

---

## Scope

This ADR applies to:

- Interactive zoom behavior in `EngineeringGraphControl`
- Axis range mutation semantics
- Multi–Y-axis interaction rules

This ADR does **not** apply to:

- Renderer design
- Data or presentation-model structure
- Export or headless rendering

---

## Status

This ADR is **Accepted** and in effect as of completion of Phase Z-5.
