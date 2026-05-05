# Design: Renderer Abstraction and Bitmap Renderer Validation

## Status
Proposed

## Purpose

This document defines a **renderer abstraction** for EngineeringGraphControl and
establishes a **Bitmap/PNG renderer** as a concrete validation case. The goal is
not to introduce new end-user features, but to prove that the existing graph
infrastructure cleanly supports **multiple non‑WinForms renderers** without
violating established architectural invariants.

This design is intentionally **implementation‑agnostic**. It defines
responsibilities, invariants, and phase boundaries, while leaving concrete
implementation decisions to follow‑on work.

---

## Motivation

Renderer abstraction was intentionally deferred earlier in development to avoid
premature or speculative design. Recent work introducing `LineRenderMode`,
discrete geometry, and legend glyph semantics has now produced a complete and
stable rendering pipeline with clearly identified variability.

At this point, introducing a second renderer is no longer speculative:

- Geometry selection and topology are resolved before rendering
- Legend semantics are resolved before rendering
- Renderers are pure rasterization stages

A Bitmap/PNG renderer provides a low‑risk, high‑signal validation that:

- Rendering is not tightly coupled to WinForms
- Presentation output can be consumed headlessly
- Graph output can be exported for reports and automation

---

## Architectural Context

The established rendering pipeline is:

1. **Model** – semantic intent (`SeriesType`, `LineRenderMode`)
2. **Snapshot** – immutable projection of model state
3. **Presentation Model** – layout resolution, geometry selection, legend
   semantics
4. **Renderer** – rasterization only

This design operates entirely at step (4) and introduces an explicit boundary at
that layer.

---

## Renderer Responsibility Boundary

### Responsibilities

A renderer:

- Consumes fully resolved presentation output
- Rasterizes:
  - plot geometry (continuous and discrete)
  - axes and grid lines
  - legend glyphs and labels
  - titles and annotations
- Honors presentation intent already encoded in geometry and legend metadata
- Targets a specific drawing surface (control, bitmap, etc.)

### Non‑Responsibilities

A renderer must **not**:

- Inspect or interpret `LineRenderMode` directly
- Select geometry or determine topology
- Resolve layout, axis stacking, or insets
- Modify or infer axis identity or mapping
- Own export or file‑format concerns

This strict boundary ensures renderer interchangeability and testability.

---

## Bitmap/PNG Renderer as Validation Case

The Bitmap renderer is introduced **to validate the renderer abstraction**, not
as a feature expansion.

Key characteristics:

- Renders into an off‑screen bitmap
- Supports explicit size and DPI
- Produces raster output suitable for encoding (e.g., PNG)
- Operates without UI or WinForms dependencies

PNG is treated strictly as an **output encoding**, not a renderer concern.

---

## Alignment with Existing ADRs

This design explicitly aligns with the following ADRs:

- [**ADR‑0003 (Abstract Geometry Plane)**](..\adr\ADR-0003-Graph-Presentation-Model-Abstract-Geometry-Plane.md)
  - Renderers consume abstract geometry without interpretation

- [**ADR‑0004 (Layout Semantics and Space Ownership)**](..\adr\ADR-0004-Addendum-LineRenderMode-LegendGlyphs.md)
  - Renderers do not participate in layout or pressure resolution

- [**ADR‑0005 (Layout Pressure and Degradation Policy)**](..\adr\ADR-0005-Layout-Pressure-Resolution-and-Degradation-Policy.md)
  - Renderer abstraction does not introduce new degradation paths

- [**ADR‑0006 (Axis Identity, Insets, and Renderer Mapping Invariants)**](..\adr\ADR-0006-Axis-Identity-Insets-And-Renderer-Mapping-Invariants.md)
  - Axis identity, insets, and mapping are fixed before rendering
  - All renderers consume the same resolved mapping

The Bitmap renderer serves as an explicit test of these invariants.

---

## Phasing Guidance (Design‑Level)

This document intentionally avoids implementation detail. However, the work is
expected to be decomposable into disciplined phases such as:

- **Phase RB‑0**: Introduce explicit renderer abstraction
- **Phase RB‑1**: Implement Bitmap renderer using existing presentation output
- **Phase RB‑2**: Validation and test coverage

Each phase must preserve renderer purity and architectural invariants.

---

## Non‑Goals and Explicit Exclusions

This design does **not** attempt to:

- Introduce vector (SVG/PDF) rendering
- Define file export APIs
- Guarantee feature parity across all future renderers
- Address performance tuning or caching
- Describe concrete class or method structures

These topics may be addressed in future designs if needed.

---

## Success Criteria

This design is considered successful if:

- WinForms and Bitmap renderers can consume the same presentation output
- No presentation or mapping logic migrates into renderers
- Bitmap rendering produces visually correct output at multiple DPIs
- Renderer abstraction enables future renderers without refactoring upstream
layers

---

## Summary

Introducing a renderer abstraction at this stage is a deliberate and justified
architectural step. The Bitmap/PNG renderer provides a concrete validation that
EngineeringGraphControl rendering is modular, testable, and not bound to
WinForms.

This design preserves all established invariants while enabling controlled
extensibility.
