# WPF EngineeringGraphControl Binding Contract

Status: Active
Date: 2026-07-02

## Purpose

This document defines the host-side binding contract for `Graphing.Controls.EngineeringGraphControl` in `Graphing.Controls.WPF`.

It is intended for strict WPF usage where code-behind is minimal and control behavior is driven by XAML bindings and view-model state.

## Host Integration Model

`EngineeringGraphControl` is a `FrameworkElement` with dependency-property-backed host inputs.

Primary host inputs:

- `GraphModel` (`IGraphModel`)
- `GraphPresentationOptions` (`GraphPresentationOptions`)
- `GraphSnapshotBuilderProvider` (`IGraphSnapshotBuilderProvider`, optional)
- `ZoomEnabled` (`bool`)
- `ZoomExtentsRequestVersion` (`int` trigger token)

Operational methods remain available:

- `SetGraphSource(IGraphModel graphModel, GraphPresentationOptions options = null)`
- `ZoomExtents()`

These methods are primarily convenience APIs. In strict MVVM hosts, use bound properties for regular operation.

## Binding Semantics

### GraphModel

When `GraphModel` changes, the control rebuilds its internal snapshot/presentation state and invalidates rendering.

### GraphPresentationOptions

When `GraphPresentationOptions` changes, the control rebuilds active presentation state for the current model.

### GraphSnapshotBuilderProvider

This optional provider enables host-supplied snapshot construction. When set (or changed), the control rebuilds snapshot/presentation state for the current model using:

- `IGraphSnapshotBuilderProvider.CreateGraphSnapshotBuilder()`
- `IGraphSnapshotBuilder.Build(IGraphModel graphModel, GraphPresentationOptions options = null)`

If not bound, the control falls back to the default internal `GraphSnapshotBuilder`.

### ZoomEnabled

Controls whether zoom drag interaction is active. Turning it off clears active drag state.

### ZoomExtentsRequestVersion

This property is a monotonically changing trigger token. Any value change requests a zoom-extents operation.

Typical usage: increment this property from a command in the view model.

## Minimal XAML Example

```xml
<g:EngineeringGraphControl
    GraphModel="{Binding GraphModel}"
    GraphPresentationOptions="{Binding GraphPresentationOptions}"
    GraphSnapshotBuilderProvider="{Binding GraphSnapshotBuilderProvider}"
    ZoomEnabled="{Binding ZoomEnabled}"
    ZoomExtentsRequestVersion="{Binding ZoomExtentsRequestVersion}" />
```

## ViewModel Trigger Pattern

```csharp
public int ZoomExtentsRequestVersion
{
    get => _zoomExtentsRequestVersion;
    private set
    {
        if (_zoomExtentsRequestVersion == value)
        {
            return;
        }

        _zoomExtentsRequestVersion = value;
        OnPropertyChanged();
    }
}

private void RequestZoomExtents()
{
    ZoomExtentsRequestVersion++;
}
```

## Boundary Rules

- Keep host integration binding-first.
- Avoid view code-behind that calls control methods in normal flows.
- Keep `Graphing.Controls.WPF` free of WinForms dependencies.
- Keep non-UI logic (view models, command orchestration) in non-WPF assemblies where practical.

## Notes

- `ActiveSnapshot`, `ActivePresentation`, and `ActiveOptions` are read-only runtime state accessors for diagnostics and advanced host inspection.
- `GraphSnapshotBuilderProvider` is intended for advanced composition scenarios (custom snapshot policies, instrumentation, testing seams).
- Options editor behavior is intentionally out of scope for the current WPF harness phase.
