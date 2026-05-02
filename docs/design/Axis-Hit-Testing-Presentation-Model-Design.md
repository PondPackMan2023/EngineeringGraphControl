
# Design Document: Axis Hit Testing via Presentation Model Geometry

## Overview

This document describes the design for **axis hit testing** in `EngineeringGraphControl`, with the specific goal of enabling **right-click interaction on individual axes** (including multiple stacked Y-axes) in a way that is:

- Architecturally correct
- Consistent with existing design decisions
- Fully aligned with established ADRs

This design focuses on **geometry-first hit testing**, where all interaction regions are defined by the `GraphPresentationModel` and **never inferred by the renderer or the control**.

---

## Architectural Foundations

This design explicitly follows:

- **[ADR-0003 – Graph Presentation Model: Abstract Geometry Plane](../adr/ADR-0003-Graph-Presentation-Model-Abstract-Geometry-Plane.md)**
- **[ADR-0004 – Graph Presentation Model: Layout Semantics and Space Ownership](../adr/ADR-0004-Graph-Presentation-Model-Layout-Semantics-and-Space-Ownership.md)**

Key implications from these ADRs:

- All geometry (including interaction geometry) must be expressed in the abstract graph coordinate plane.
- The renderer must act as a pure geometry consumer.
- Layout ownership boundaries must be respected; interaction must not bleed across owned regions.

---

## Goals

The axis hit-testing design must:

1. Allow right-click interaction on **each individual axis**.
2. Support **multiple stacked Y-axes** without ambiguity.
3. Avoid renderer-side logic or inference.
4. Automatically adapt as axis styling changes (e.g., axis thickness).
5. Produce deterministic, testable behavior.

---

## Non-Goals

This design explicitly does **not**:

- Define context menu content or behavior.
- Perform hit testing against tick labels or tick marks.
- Introduce renderer-side hit logic.
- Define hover, drag, or left-click behavior (future work).

---

## Axis Interaction Model

### What Is Clickable

Only the **axis line itself** is considered interactive.

- Tick marks are **visual adornments**, not interaction targets.
- Tick labels are **read-only text**, not interaction targets.

This avoids accidental interaction and cleanly separates concerns.

---

## Axis Hit Region Definition

Each axis emits an **explicit hit region geometry** as part of the `GraphPresentationModel`.

### Geometry Shape

- The hit region is a **narrow rectangle** centered on the axis line.
- The region spans the full length of the axis line.

Orientation-specific behavior:

- **Y-axis**: vertical rectangle with small horizontal thickness
- **X-axis**: horizontal rectangle with small vertical thickness

---

## Hit Region Thickness (Critical Design Choice)

### Relative to Axis Line Thickness

The hit region thickness is derived from the **actual axis line thickness**, not the axis band size and not a fixed constant.

This ensures that:

- Thicker axes are easier to click
- Thinner axes remain precise
- Styling changes automatically affect interaction behavior

### Formula

Let:

- `T` = axis line thickness (abstract units)
- `F` = hit inflation factor (default: `0.5`)

Then:

```
halfHitThickness = T * F
```

This value is applied symmetrically on both sides of the axis line.

The inflation factor is intentionally isolated so it can be tuned later without geometry refactoring.

---

## Stacked Y-Axis Behavior

Multiple Y-axes may be stacked vertically.

- If axis bands **touch**, their hit regions share a border.
- If axis bands include **spacing**, hit regions are separated by a gap.

Hit regions must **exactly match axis band layout**:

- No overlap
- No gap-filling
- No special casing

This follows ADR-0004 space-ownership rules.

---

## Event Model

### Trigger

- Right mouse button click within an axis hit region.

### Event Responsibility

The control fires an event; consumers decide how to respond.

### Proposed Event Data

The event payload should include:

- `IAxisModel` – identifies the axis
- Mouse modifiers – `Ctrl`, `Alt`, `Shift`
- Click location in **control coordinates** (suitable for context menu placement)

The renderer is **not involved** in event generation.

---

## Hit Testing Flow

1. `EngineeringGraphControl` receives a mouse right-click.
2. Control queries the `GraphPresentationModel` for axis hit regions.
3. The first axis whose hit region contains the click point is selected.
4. Axis right-click event is raised with axis and input context.
5. If no axis contains the point, no axis event is raised.

---

## Renderer Responsibilities (Explicitly Limited)

The renderer:

- Draws axis lines, ticks, and labels
- Consumes geometry only

The renderer **must not**:

- Perform hit testing
- Decide interaction regions
- Infer axis orientation or semantics

---

## Design Invariants

The following rules must always hold:

- If something is interactive, its geometry is defined by the PresentationModel.
- Interaction geometry is derived from visual geometry, not layout allocation.
- Geometry adjacency (shared borders vs spacing) dictates interaction behavior.
- No device-space heuristics are used.

---

## Future Considerations

This design supports future extensions without change to core principles:

- Hover feedback
- Axis selection
- Axis drag-to-resize
- Touch input
- Alternate renderers

All such features can reuse the same interaction geometry.

---

## Summary

This design establishes a **geometry-first, ADR-compliant** approach to axis hit testing:

- Interaction is explicit and deterministic
- Multiple axes are handled naturally
- Renderer remains purely visual
- Styling and interaction stay in sync

This document intentionally precedes implementation and should be treated as a binding architectural guide.

---

*End of document.*
