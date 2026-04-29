# EngineeringGraphControl – Built-in Options Editor Design

## Status

Proposed

---

## Motivation

EngineeringGraphControl intentionally presents an immutable view over an immutable engineering graph model (`IGraphModel`). While consumers can build custom editors on top of this API today, there is strong value in providing a **built-in, default options editor** that allows users to customize *presentation* without modifying underlying model semantics.

The goals of this editor are:

- Provide a simple, discoverable way to edit common presentation options
- Respect immutability and architectural boundaries
- Avoid chart-framework complexity (e.g., TeeChart-style editors)
- Remain easy to extend **by developers**, not by end users

This document describes the intended design, responsibilities, and architectural boundaries for that editor.

---

## Non-Goals

The built-in options editor is **not** intended to:

- Act as a generic chart editor or charting framework
- Allow users to add/remove axes or otherwise modify graph structure
- Mutate `IGraphModel` or any model-layer objects
- Introduce reusable UI frameworks or binding infrastructure
- Mirror or compete with TeeChart-level configurability

---

## Architectural Principles

This design follows the same principles already established in EngineeringGraphControl:

- **Presentation over semantics** – the editor changes how the graph is shown, not what it means
- **Immutability** – all edits produce new immutable option objects
- **Clear ownership** – the model owns structure; the editor owns configuration
- **Minimal surface area** – only expose options that are meaningful and supported

---

## High-Level Responsibilities

### Graphing.Controls

- Owns `EngineeringGraphControl`
- Owns `IGraphModel`, `GraphPresentationOptions`, and the PresentationModel
- Remains UI-agnostic and editor-agnostic
- Exposes presentation state but does not host editing workflows

### Graphing.Editors (new project)

- Hosts the built-in options editor UI
- References Graphing.Controls
- Contains all WinForms UI, binding, and adapter logic
- Produces new `GraphPresentationOptions` instances
- Does **not** act as a reusable editor framework

Graphing.Controls must never depend on Graphing.Editors.

---

## Editor Interaction Model

The options editor operates as a pure transformation:

1. Consume:
   - `IGraphModel` (read-only)
   - Current `GraphPresentationOptions`
2. Allow user edits in a temporary, mutable adapter
3. Produce a **new** `GraphPresentationOptions` instance
4. Let the host decide whether to apply or discard the result

Canceling the dialog produces no side effects.

---

## Design Pattern

The editor uses a **Presentation Model / Adapter** pattern:

- Inspired by MVVM, but adapted for WinForms
- No commands, messaging, or reusable frameworks
- Adapter objects are mutable and dialog-scoped
- Core graph and presentation types remain immutable

This keeps responsibilities clear and avoids leaking UI concepts into core layers.

---

## Options Editor Adapter Model

The adapter mirrors only *editable* presentation concerns and exists purely to support the UI.

### GraphOptionsEditorModel (root)

- Holds child editor models for each option category
- Builds a new `GraphPresentationOptions` on confirmation

---

### Series Options

Series are provided by `IGraphModel` and cannot be added or removed.

Per-series configurable options:

- Visibility
- Display label (override of model-provided label)
- Order (z-order for rendering)
- Color

Overrides are stored in `GraphPresentationOptions`; defaults always come from the model.

---

### Axis Options

Axes are fixed by the graph model (`IGraphModel.Axes`). The editor enumerates and configures them but never modifies structure.

Each axis supports:

- Visibility
- Axis title text (override)
- Units (display units, read-only from `UnitRegistry` / `IAxisModel`)
- Minimum value (Auto vs Fixed)
- Maximum value (Auto vs Fixed)
- Major tick increment (Auto vs Fixed)

All numeric values are entered directly in **display units**. No unit conversion is required.

---

### Legend Options

Legend configuration is presentation-only and applies globally:

- Visibility
- Position (Left, Right, Top, Bottom)

Legend configuration does not affect graph structure or series binding.

---

### Title & Subtitle Options

Titles describe the plot area and are treated as presentation bands.

Editable options:

- Title text (override)
- Subtitle text (override)
- Visibility
- Font family
- Font size (restricted to a reasonable range)

Titles and subtitles always align horizontally to the plot area, never the full control width.

---

### Background Options

Limited, high-value visual options only:

- Control background color
- Plot area background color

These options affect appearance only and never geometry semantics.

---

## Overrides vs Defaults

For all editable text values (series labels, axis titles, title/subtitle text):

- `IGraphModel` provides defaults
- `GraphPresentationOptions` may provide overrides
- The effective value is resolved as:

```
Presentation override ?? model default
```

Clearing an override reverts to the model-provided value.

---

## UI Structure

To remain simple and navigable:

- A left-hand navigation list selects the option category
- A right-hand panel edits the selected category
- No nested tabs
- No dynamic structure editing

This layout scales naturally as options are added without becoming overwhelming.

---

## Extensibility Guidelines

Adding a new editable option should require:

1. Updating the adapter model
2. Mapping to/from `GraphPresentationOptions`
3. Adding UI controls in the appropriate editor panel

No changes to rendering or layout code should be required.

---

## Out of Scope (Explicit)

The editor will not support:

- Adding/removing axes or series
- Changing axis types
- Minor tick configuration (until implemented)
- Renderer-level styling knobs
- Framework-style plugin extensibility

---

## Future Phasing

Recommended phased implementation:

1. Core adapter definitions
2. Series editor UI
3. Axis editor UI
4. Legend editor UI
5. Title/subtitle editor UI
6. Background options

Each phase should be independently usable and testable.

---

## Summary

The built-in options editor for EngineeringGraphControl is a **product-level convenience**, not a framework feature. By combining immutable models with a lightweight presentation adapter and a focused UI, it provides meaningful customization while preserving architectural clarity, testability, and long-term maintainability.
