# Pie Graph Design

## Status

Design draft for implementation planning.

> ***_NOTE:_*** This document describes the planned Pie Graph subsystem design. It is intentionally a design document, not an ADR. Per project procedure, the ADR should be written after implementation is complete so that it reflects the final implemented reality.

## Purpose

The goal of this design is to introduce first-class Pie Graph support for the graphing framework used by mission-personal-ledger (`mpl`) and future consumers.

The Pie Graph implementation will be architecturally independent from the existing Cartesian `EngineeringGraphControl` implementation. It will reuse the same general lifecycle pattern that has proven successful for Cartesian graphs:

```text
Model
  -> Snapshot
  -> Presentation
  -> Renderer
  -> Control Host
```

However, Pie Graphs will not derive from or extend the existing Cartesian graph model, snapshot, presentation model, or renderer types.

## Design Principle

The guiding principle for this implementation is:

> Reuse architectural concepts, not Cartesian graph implementation details.

The Pie Graph subsystem should be designed as if no Cartesian graph implementation existed. The existing codebase may still provide useful patterns, conventions, or utilities, but the Pie Graph subsystem should not be forced into Cartesian abstractions such as axes, series, plot areas, zoom rectangles, axis formatting, or axis interaction.

Shared code may be reused or extracted later where genuine commonality naturally appears. Reuse is allowed, but it is not a primary design goal.

> ***_NOTE:_*** Let trouble find us rather than looking for it. Avoid premature abstraction and avoid introducing shared base types until actual duplication or integration pressure proves that they are needed.

## High-Level Architecture

### New Core Project

Create a new non-UI project:

```text
Graphing.Core.Pie
```

Target framework:

```text
net10.0
```

This project is strictly non-UI and should not reference WinForms or WPF.

### Project Responsibilities

`Graphing.Core.Pie` owns the Pie Graph framework:

```text
Graphing.Core.Pie
├── Models
├── Snapshot
├── Presentation
├── Rendering
└── Geometry
```

The project should contain all framework-level Pie Graph logic required to build snapshots, construct presentation geometry, and provide renderer-consumable primitives.

### UI Host Controls

The Pie Graph control surface will be hosted separately by framework-specific projects:

```text
Graphing.Controls.WPF
└── EngineeringPieGraphControl
```

```text
Graphing.Controls.WinForms
└── EngineeringPieGraphControl
```

WPF is the priority host. WinForms support may follow after the WPF path is validated.

## Architectural Independence From Cartesian Graphs

The Pie Graph subsystem is a parallel implementation, not a new graph type inside the Cartesian graphing system.

### Explicit Non-Goals

The Pie Graph implementation will not introduce:

- A common root interface shared with Cartesian `IGraphModel`
- A `GraphType` discriminator on Cartesian graph models
- Pie-specific branches inside Cartesian graph models
- Pie-specific branches inside Cartesian snapshot builders
- Pie-specific branches inside Cartesian presentation models
- Pie-specific branches inside Cartesian renderers
- Fake axes
- Fake series
- Empty axis collections as compatibility shims
- Dummy Cartesian graph structures

### Cartesian Concepts Are Out of Scope

The Pie Graph subsystem does not use:

- `IAxisModel`
- `IGraphSeriesModel`
- `IGraphFieldDefinition`
- Cartesian `GraphPresentationModel`
- Cartesian `GraphSnapshot`
- Axis interaction
- Zoom rectangles
- Axis layout bands
- Plot-area-based layout

These concepts belong to Cartesian graphing and should not be forced into the Pie Graph design.

## Lifecycle

The Pie Graph lifecycle is:

```text
PieGraphModel
  -> PieGraphSnapshot
  -> PieGraphPresentationModel
  -> PieGraphRenderer
  -> EngineeringPieGraphControl
```

Each stage has a distinct responsibility.

| Layer | Responsibility |
|---|---|
| Model | Defines user/domain data: title, unit, formatter, and slices |
| Snapshot | Computes derived values: total, percentage, start angle, sweep angle |
| Presentation | Builds normalized, renderer-consumable geometry and visual layout |
| Renderer | Consumes geometry and draws it |
| Control | Hosts the graph, orchestrates lifecycle, and integrates with WPF/WinForms |

## Model Layer

### Purpose

The model layer represents the semantic Pie Graph data. It should be small, explicit, immutable or effectively immutable where practical, and free of rendering concerns.

### Interfaces

Conceptual model contracts:

```csharp
public interface IPieGraphModel
{
    string Title { get; }

    Unit Unit { get; }

    IValueFormatter Formatter { get; }

    IReadOnlyList<IPieSliceModel> Slices { get; }
}
```

```csharp
public interface IPieSliceModel
{
    string Label { get; }

    double Value { get; }
}
```

### Unit and Formatter Ownership

`Unit` and `IValueFormatter` belong on `IPieGraphModel`, not on individual slices.

Rationale:

- A pie chart represents parts of a single whole.
- All slices must share the same unit for percentages to be meaningful.
- Mixed-unit slices would create mathematically invalid comparisons.
- Formatting the raw value is graph-level policy because all slices share the same unit.

Examples:

```text
Spending By Category
Housing      $2,500
Food           $800
Utilities      $400
```

All slices represent currency values.

```text
Time Allocation
Meetings       12 hours
Coding         20 hours
Planning        6 hours
```

All slices represent time values.

### Slice Responsibilities

A slice owns only:

- Label
- Raw numeric value

A slice does not own:

- Percentage
- Angle
- Color
- Tooltip text
- Unit
- Formatter
- Presentation geometry

### Tooltip Considerations

Tooltip support is not a V1 priority, but the model and snapshot should preserve enough information to support future tooltips.

A future tooltip could be derived from:

```text
Label
Formatted Value
Percentage
```

Example:

```text
Brokerage
$125,000
28.4%
```

No explicit tooltip property is required for V1.

## Snapshot Layer

### Purpose

The snapshot layer transforms model data into derived, read-only values suitable for presentation construction.

The snapshot layer owns Pie Graph math.

### Conceptual Types

```text
PieGraphSnapshot
PieSliceSnapshot
```

### PieGraphSnapshot Responsibilities

`PieGraphSnapshot` owns:

- Graph title
- Unit
- Formatter
- Total value
- Slice snapshots

### PieSliceSnapshot Responsibilities

`PieSliceSnapshot` owns:

- Label
- Raw value
- Formatted value
- Percentage
- Start angle
- Sweep angle

### Percentage Calculation

The total value is computed by summing all included slice values:

```text
TotalValue = Sum(slice.Value)
```

Each slice percentage is:

```text
SlicePercentage = slice.Value / TotalValue
```

For display, percentage may later be formatted as:

```text
SlicePercentage * 100
```

### Angle Calculation

Pie slice angles are derived from percentages.

```text
SweepAngle = SlicePercentage * 360 degrees
```

The first slice begins at the configured/default starting angle. Each subsequent slice starts after the previous slice's sweep angle.

Conceptual example:

```text
Slice A: 25%
StartAngle = 0
SweepAngle = 90

Slice B: 75%
StartAngle = 90
SweepAngle = 270
```

### Invalid or Edge Case Data

The design should account for edge cases during implementation planning:

- Empty slice collection
- Null or blank labels
- Zero total
- Negative values
- NaN or infinity
- Very small slices

V1 should define safe behavior for invalid data. The preferred direction is to fail safely and avoid renderer exceptions.

> ***_NOTE:_*** The exact validation behavior can be finalized during implementation phases. The design expectation is that invalid model data should not cause uncontrolled rendering failures.

## Presentation Layer

### Purpose

The presentation layer creates normalized, renderer-consumable geometry.

The presentation layer owns visual layout decisions.

The renderer consumes the resulting geometry and does not compute layout, percentages, colors, or angles.

### Initial Presentation Type

V1 will introduce only:

```text
PieGraphPresentationModel
```

No `DonutGraphPresentationModel` is introduced in V1.

### PieGraphPresentationModel Responsibilities

`PieGraphPresentationModel` owns:

- Pie center
- Pie radius
- Slice presentation geometry
- Legend visibility
- Right-side legend layout
- Palette color assignment
- Text/legend geometry sufficient for rendering

### Geometry Coordinate System

The Pie Graph presentation layer should use normalized abstract geometry consistent with the existing graphing philosophy:

```text
Bottom-left: (0, 0)
Upper-right: (1, 1)
X increases to the right
Y increases upward
```

The renderer is responsible for translating this abstract geometry into device-specific coordinates.

### Conceptual Slice Presentation Geometry

A slice presentation geometry should include enough information for the renderer to draw without performing graph math.

Conceptual shape:

```csharp
public sealed class PieSlicePresentationGeometry
{
    public string Label { get; }

    public double Value { get; }

    public string FormattedValue { get; }

    public double Percentage { get; }

    public GeometryPoint3D Center { get; }

    public double OuterRadius { get; }

    public double StartAngle { get; }

    public double SweepAngle { get; }

    public GraphColor Color { get; }
}
```

The exact type names and property types can be adjusted during implementation, but the renderer should receive equivalent information.

### Color Assignment

Color is a presentation concern, not a model concern.

V1 will use a fixed default palette. The initial palette should contain 16 visually distinct colors and repeat after 16 slices.

Future user color customization may be supported through an options dialog or presentation options, but that is deferred.

### Legend Strategy

V1 supports legend-only slice labeling.

Slice labels are not drawn inside slices and no leader lines are implemented in V1.

Legend behavior for V1:

- Legend is visible by default
- Legend can be hidden programmatically
- Legend placement is right-side only
- Other placements are deferred

The design should avoid painting the implementation into a corner. Future support for left, top, or bottom legend placement should remain possible.

### Legend Item Limit

Financial applications such as Money and Quicken often keep pie legends compact by showing a limited number of items and offering access to additional items through a More-style affordance.

For V1, this behavior is not required.

However, the design should keep the future option open for:

- Maximum visible legend item count
- Remaining item count
- More item affordance
- Popup or expanded legend view

### Label Strategy

V1 uses legend labels only.

Deferred label features:

- Inside-slice labels
- Outside labels
- Leader lines
- Collision avoidance
- Automatic label hiding for small slices

## Renderer Layer

### Purpose

The renderer consumes presentation geometry and draws it.

The renderer is not responsible for:

- Calculating totals
- Calculating percentages
- Calculating angles
- Assigning colors
- Performing legend layout
- Deciding whether the chart is Pie or Donut
- Creating presentation geometry

### Renderer Contract

The renderer should accept:

- Device/render context
- Device bounds
- `PieGraphPresentationModel`
- Pie presentation options, if needed

The renderer should draw:

- Pie slices
- Slice borders, if supported
- Legend entries
- Title, if supported by V1

### WPF Priority

The WPF renderer and WPF control path are the priority.

The WinForms renderer/control may be implemented later or after WPF validation, depending on phase planning.

## Control Layer

### WPF Control

Create:

```text
Graphing.Controls.WPF.EngineeringPieGraphControl
```

The WPF control should be binding-friendly and suitable for strict MVVM use.

Likely host-facing properties:

```csharp
public IPieGraphModel PieGraphModel { get; set; }

public PieGraphPresentationOptions PieGraphPresentationOptions { get; set; }

public PieGraphSnapshot ActiveSnapshot { get; }

public PieGraphPresentationModel ActivePresentation { get; }
```

The exact WPF dependency property surface should be finalized during implementation phase planning.

### WinForms Control

Create, or defer based on implementation scope:

```text
Graphing.Controls.WinForms.EngineeringPieGraphControl
```

WinForms is lower priority than WPF for this effort.

If implemented, the WinForms control should follow equivalent lifecycle semantics while preserving framework-specific UI boundaries.

## Presentation Options

Introduce Pie-specific presentation options rather than reusing Cartesian `GraphPresentationOptions`.

Conceptual type:

```text
PieGraphPresentationOptions
```

V1 options may include:

```csharp
public bool LegendVisible { get; }
```

Potential future options:

```text
LegendPlacement
Palette override
Maximum visible legend items
Show values in legend
Show percentages in legend
Slice border visibility
Start angle
Clockwise/counter-clockwise direction
Donut inner radius ratio
```

Only options needed for V1 should be implemented initially.

## Donut Chart Support

Donut charts are explicitly deferred.

The V1 goal is to implement standard pie charts first and validate them in:

- Pie-specific test harness
- WPF control
- Optional WinForms control
- `mpl` integration

After that, donut support will be revisited.

Possible future implementation options:

1. Extend `PieGraphPresentationModel` with donut geometry support
2. Add a separate `DonutGraphPresentationModel`
3. Introduce shared presentation helpers only if actual duplication emerges

No decision is made in V1.

### Why Donut Is Deferred

Pie and donut charts share the same model and snapshot data. The difference is presentation geometry:

```text
Pie:
InnerRadius = 0
OuterRadius = R

Donut:
InnerRadius > 0
OuterRadius = R
```

However, donut charts may introduce additional layout needs such as center text, center totals, or different label behavior. Those requirements should be evaluated after pie charts are working.

## Test Harness

Create a new Pie-specific test harness rather than forcing Pie Graph scenarios into the existing Cartesian harness.

The test harness should validate:

- Basic pie rendering
- Multiple slices
- Fixed palette behavior
- Legend visibility
- Right-side legend layout
- Empty or invalid data handling
- Large slice counts
- Small slice percentages
- Currency-like values for `mpl` scenarios

WPF harness support is the priority.

Potential project direction:

```text
Graphing.TestHarness.Pie.WPF
```

or equivalent, depending on existing solution conventions.

## mpl Integration

After Pie Graph support is validated in the test harness, integrate it into `mpl`.

Likely initial `mpl` use case:

```text
Spending by Category
```

Other likely future uses:

- Asset allocation
- Budget breakdown
- Income breakdown
- Expense category summaries

Integration should be straightforward once the WPF control accepts an `IPieGraphModel` and renders correctly.

## V1 Scope

### Included

- New `Graphing.Core.Pie` project targeting `net10.0`
- Independent Pie model layer
- Independent Pie snapshot layer
- Independent Pie presentation layer
- Pie renderer
- WPF `EngineeringPieGraphControl`
- Pie-specific WPF test harness
- Fixed 16-color repeating palette
- Right-side legend
- Programmatic legend visibility
- Legend-only labels
- UnitRegistry `Unit` support on graph model
- `IValueFormatter` support on graph model
- `mpl` integration after framework validation

### Optional / Lower Priority

- WinForms `EngineeringPieGraphControl`

### Deferred

- Donut charts
- User-customizable colors
- Options dialog
- Slice labels
- Leader lines
- Slice selection
- Slice hover interaction
- Tooltips beyond derived data support
- Exploded slices
- Animation
- More/expanded legend affordance
- Alternate legend placements
- Multi-ring charts

## Proposed Implementation Phase Seeds

The implementation phase document should be generated separately, but the design naturally suggests the following phase sequence.

### PIE-1: Project and Model Layer

- Create `Graphing.Core.Pie`
- Add model interfaces and default implementations
- Add UnitRegistry and formatter dependencies
- Add basic model tests

### PIE-2: Snapshot Layer

- Add snapshot types
- Add snapshot builder
- Compute totals, percentages, start angles, and sweep angles
- Add validation and edge-case tests

### PIE-3: Presentation Layer

- Add `PieGraphPresentationModel`
- Add slice geometry
- Add fixed palette
- Add right-side legend geometry
- Add legend visibility option
- Add presentation tests where practical

### PIE-4: WPF Renderer and Control

- Add WPF renderer support
- Add `EngineeringPieGraphControl` in `Graphing.Controls.WPF`
- Bind model/options inputs
- Render active presentation

### PIE-5: Pie Test Harness

- Add Pie-specific WPF test harness
- Add representative scenarios
- Validate runtime rendering visually

### PIE-6: mpl Integration

- Add initial Pie Graph usage in `mpl`
- Start with a category/value scenario such as spending by category
- Validate formatting, unit handling, palette behavior, and legend display

### PIE-7: WinForms Control Evaluation

- Decide whether WinForms support is required immediately
- Implement WinForms host if needed

### PIE-8: Donut Investigation

- Revisit donut charts after V1 pie charts work end-to-end
- Decide whether to extend `PieGraphPresentationModel` or add `DonutGraphPresentationModel`

## Design Summary

The Pie Graph subsystem will be a new, independent graphing framework for pie-family visualizations.

It will live in a new non-UI `Graphing.Core.Pie` project targeting `net10.0`, with WPF and optionally WinForms host controls provided by their respective UI projects.

The subsystem will follow the proven lifecycle pattern:

```text
Model
  -> Snapshot
  -> Presentation
  -> Renderer
```

but will remain independent from the Cartesian graphing implementation.

The V1 implementation focuses on standard pie charts only, with WPF as the priority host, a fixed palette, right-side legend, legend-only labels, UnitRegistry integration, and `IValueFormatter` support.

Donut charts are intentionally deferred until after standard pie charts are working in the Pie test harness and `mpl`.
