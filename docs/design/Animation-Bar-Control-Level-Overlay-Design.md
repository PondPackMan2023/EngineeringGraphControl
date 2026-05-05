# Animation Bar Design – Control-Level Interaction Overlay

> **Status:** Design document
> 
> **Scope:** Defines the architecture and responsibilities for the Animation Bar feature as a *control-level interaction overlay*. This design intentionally excludes export renderers (Bitmap, Metafile).

---

## Overview

The **Animation Bar** is a vertical indicator that allows users to scrub across X-axis data interactively. It is:

- Rendered **only in the WinForms UI**
- Drawn **on top of all series**, clipped to the plot area
- Positioned by **X-value index**, not data value or pixel offset
- Interactive (click, drag, snap)
- Capable of programmatic repositioning

The Animation Bar is **not part of the shared renderer pipeline**. Instead, it is implemented as a **control-owned interaction overlay** rendered by `EngineeringGraphControl`.

---

## Architectural Classification

### What the Animation Bar Is

- ✅ A UI-only interaction affordance
- ✅ A temporary, non-exportable overlay
- ✅ Driven by user input and control state

### What the Animation Bar Is Not

- ❌ A data series
- ❌ A layout participant
- ❌ A renderer feature
- ❌ Part of bitmap or metafile export

---

## Layer Responsibility Breakdown

### Presentation & Layout Layer

- Owns all abstract geometry and axis mappings
- Provides:
  - Plot area bounds
  - X-axis index → abstract coordinate mapping

> **Note:** The presentation model does *not* store animation bar state.

---

### Renderers (WinForms / Bitmap / Metafile)

- Render only persistent presentation elements:
  - Axes
  - Grid
  - Series
  - Titles
  - Legends

- ✅ Remain identical across output targets
- ❌ Do not know animation bar exists

---

### EngineeringGraphControl (Owner of Animation Bar)

The control exclusively owns:

- Animation bar state (`XIndex`)
- Hit testing and drag tracking
- Snapping logic (nearest X-index)
- Event dispatch when user moves the bar
- UI-only rendering of the overlay

Rendering order (WinForms):

1. Call `WinFormsGraphRenderer`
2. Render animation bar overlay in device space

---

## Geometry & Coordinate Model

- **Authoritative position:** `XIndex`
- Conversion flow:
  
  ```
  XIndex → Abstract X Coordinate → Device X Coordinate
  ```

- Vertical span:
  - From plot area bottom to plot area top

- Rendering uses:
  - Device-space clipping to plot area bounds
  - No pixel state stored between frames

---

## Interaction Behavior

### User Interaction

- Hit test against device-space animation bar line
- On drag:
  - Mouse X → abstract X
  - Abstract X → nearest XIndex
  - Update control state
  - Raise event (e.g., `AnimationBarMovedByUser`)

### Programmatic Control

- Public API allows setting `XIndex`
- Updating index triggers repaint (no special cases)

---

## Rendering Characteristics (UI Only)

- Drawn last (highest Z-order)
- Does not consume layout space
- Does not affect plot sizing
- Does not participate in layout pressure or degradation

---

## Explicit Design Rules

- Anything **interactive or non-exportable** must be rendered by the control
- Shared renderers must remain pure and export-safe
- No renderer-specific conditionals are allowed in shared layers
- Device-space rendering is allowed *only* in the control

---

## Rationale

This design:

- Preserves renderer abstraction purity
- Avoids feature skew between export and UI renderers
- Aligns with existing ADRs for geometry, layout, and degradation
- Establishes a clean pattern for future interaction overlays

---

## Future Extensions Enabled

- Selection cursors
- Hover guidelines
- Ranges / scrubbers
- Transient highlights
- Tooltips and annotations

All without modifying renderer implementations or export behavior.
