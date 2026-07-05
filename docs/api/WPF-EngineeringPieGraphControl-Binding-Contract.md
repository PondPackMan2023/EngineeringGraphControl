## WPF EngineeringPieGraphControl Binding Contract

Status: Active
Date: 2026-07-05

### Purpose

This document defines the host-side binding contract for EngineeringPieGraphControl in Graphing.Controls.WPF.

The control is intended for binding-first WPF usage where behavior is driven by XAML bindings and view-model state.

### Host Integration Model

EngineeringPieGraphControl is a FrameworkElement with dependency-property-backed host inputs.

Primary host inputs:

- PieGraphModel (IPieGraphModel)
- PieGraphPresentationOptions (PieGraphPresentationOptions)
- PieSliceDoubleClickCommand (ICommand, optional)

In strict MVVM hosts, bindings should be preferred over control manipulation.

### Binding Semantics

#### PieGraphModel

When PieGraphModel changes, the control rebuilds its snapshot and presentation state and invalidates rendering.

#### PieGraphPresentationOptions

When PieGraphPresentationOptions changes, the active presentation model is rebuilt.

Supported options include:

- LegendVisible
- UseShortLegend
- ShowLegendBorder

#### PieSliceDoubleClickCommand

Executed when the user double-clicks a pie slice.

Command parameter:

```csharp
PieSliceInteractionContext
```

Context includes:

- PieSliceId
- Label
- Value
- FormattedValue
- Percentage

No command is executed when:

- No slice is under the cursor.
- The command is null.
- CanExecute returns false.

### Tooltip Behavior

The control provides built-in tooltip support.

Tooltip content:

```text
Label
FormattedValue
Percentage
```

Tooltips are:

- Hover-delay driven
- Presentation-geometry hit-test driven
- Automatically managed by the control

### Minimal XAML Example

```xml
<g:EngineeringPieGraphControl
    PieGraphModel="{Binding PieGraphModel}"
    PieGraphPresentationOptions="{Binding PieGraphPresentationOptions}"
    PieSliceDoubleClickCommand="{Binding PieSliceDoubleClickCommand}" />
```

### Boundary Rules

- Keep host integration binding-first.
- Avoid application logic in the control.
- Use PieSliceId for identity.
- Do not use display labels as identifiers.
- Keep application-specific navigation in host applications.

### Notes

The control includes:

- Pie rendering
- Legend generation
- Hit testing
- Tooltips
- Double-click command support
- Stable immutable slice identity
