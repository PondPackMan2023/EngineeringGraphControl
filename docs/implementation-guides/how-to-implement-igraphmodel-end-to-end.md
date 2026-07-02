# How to Implement an IGraphModel End-to-End

## Overview

This guide defines a practical, repeatable pattern for building a valid IGraphModel from scratch.

It is written for new developers (and AI code generators) and shows both:

- Using the default model classes provided by Graphing.Core
- Implementing the interfaces directly with your own types

Important: using the default classes is optional. Implementing interfaces directly is fully supported.

For a concrete in-repo construction reference, see:

- src/Graphing.TestScenarios/Scenarios/ScenarioDefinitions.cs

## Core Interface Contract

A graph model is composed from four core model interfaces:

```csharp
public interface IGraphModel
{
    IReadOnlyList<IAxisModel> Axes { get; }
    IReadOnlyList<IGraphSeriesModel> Series { get; }

    IGraphModel ChangeAxisUnit(AxisId axisId, Unit unit);
    IGraphModel ChangeAxisFormat(AxisId axisId, IValueFormatter formatter);
    IGraphModel ChangeAxisUnitAndFormat(AxisId axisId, Unit unit, IValueFormatter formatter);
    IGraphModel ChangeAxisUnits(IReadOnlyDictionary<AxisId, Unit> unitChanges);
}

public interface IAxisModel
{
    AxisId Id { get; }
    AxisOrientation Orientation { get; }
    AxisSide Side { get; }
    Unit Unit { get; }
    string UnitLabel { get; }
    IValueFormatter Formatter { get; }
    AxisScaleType ScaleType { get; }
    bool IsAutoRange { get; }
    double? MinimumValue { get; }
    double? MaximumValue { get; }

    IAxisModel ChangeUnit(Unit newUnit);
    IAxisModel ChangeFormat(IValueFormatter newFormatter);
}

public interface IGraphSeriesModel
{
    SeriesId SeriesId { get; }
    string Label { get; }
    SeriesType SeriesType { get; }
    LineRenderMode LineRenderMode { get; }
    IGraphFieldDefinition XField { get; }
    IGraphFieldDefinition YField { get; }
    IAxisModel XAxis { get; }
    IAxisModel YAxis { get; }
}

public interface IGraphFieldDefinition
{
    string Label { get; }
    string Name { get; }
    Unit Unit { get; }
    Array GetValues();
}
```

## Step 1 - Choose an Implementation Strategy

You have two valid strategies.

### Strategy A (Fastest): Use default classes

- AxisModel
- GraphSeriesModel
- GraphModel

This is the quickest path and matches existing scenario construction patterns.

### Strategy B (Advanced): Implement interfaces directly

Create your own classes implementing:

- IAxisModel
- IGraphSeriesModel
- IGraphModel

Use this when you need custom immutability, custom validation, metadata storage, or domain-specific behavior.

Both strategies are valid and supported.

## Step 2 - Implement Field Definitions

Subclassing GraphFieldDefinitionBase is allowed and recommended.

GraphFieldDefinitionBase already implements Label, Name, and Unit, leaving only GetValues() for you.

```csharp
public sealed class DemandFieldDefinition : GraphFieldDefinitionBase
{
    private readonly Array _values;

    public DemandFieldDefinition(string name, string label, Unit unit, Array values)
        : base(name, label, unit)
    {
        _values = values;
    }

    public override Array GetValues() => _values;
}
```

Why this is recommended:

- Reduces boilerplate
- Keeps field classes focused on value sourcing
- Aligns with existing scenario patterns

## Step 3 - Build Axes

Create at least one X axis and one Y axis.

Default-class example:

```csharp
var xAxis = new AxisModel(
    new AxisId("time"),
    AxisOrientation.X,
    AxisSide.Bottom,
    Units.Time.Second,
    "sec",
    new NumericFormatter("fmt-time", UnitsRegistry.Default, "Time", "F0"));

var yAxis = new AxisModel(
    new AxisId("pressure"),
    AxisOrientation.Y,
    AxisSide.Left,
    Units.Pressure.Psi,
    "psi",
    new NumericFormatter("fmt-pressure", UnitsRegistry.Default, "Pressure", "F2"));
```

## Step 4 - Build Series

Each series must define:

- Stable SeriesId
- Label
- SeriesType
- XField and YField
- XAxis and YAxis

```csharp
var series = new GraphSeriesModel(
    new SeriesId("pressure-1"),
    "Pressure",
    SeriesType.Line,
    xField,
    yField,
    xAxis,
    yAxis,
    LineRenderMode.LineOnly);
```

## Step 5 - Compose the IGraphModel

Default-class composition:

```csharp
var graph = new GraphModel(
    new[] { xAxis, yAxis },
    new[] { series });
```

At this point, graph is a valid IGraphModel instance.

## Step 6 - Validity Checklist (Important)

For predictable snapshot and rendering behavior, your graph should satisfy all of these:

- Axis IDs are unique across graph.Axes
- Every series has non-null XAxis and YAxis
- Series axis IDs exist in graph.Axes
- XField.GetValues() and YField.GetValues() return arrays with equal lengths for each series
- Units are intentional (axis unit can differ from field unit; snapshot build will convert)
- IDs and labels are stable and meaningful

Note: duplicate axis IDs are rejected during snapshot construction.

## Direct Interface Implementation Example

If you do not want to use GraphModel/AxisModel/GraphSeriesModel, implement interfaces directly.

> _**NOTE: The following methods are illustrative stubs. In production, implement actual immutable update behavior and return a new valid model when a change is requested.**_

```csharp
public sealed class CustomGraphModel : IGraphModel
{
    public CustomGraphModel(IReadOnlyList<IAxisModel> axes, IReadOnlyList<IGraphSeriesModel> series)
    {
        Axes = axes;
        Series = series;
    }

    public IReadOnlyList<IAxisModel> Axes { get; }
    public IReadOnlyList<IGraphSeriesModel> Series { get; }

    public IGraphModel ChangeAxisUnit(AxisId axisId, Unit unit)
    {
        // Implement your immutable replacement semantics.
        return this;
    }

    public IGraphModel ChangeAxisFormat(AxisId axisId, IValueFormatter formatter)
    {
        // Implement your immutable replacement semantics.
        return this;
    }

    public IGraphModel ChangeAxisUnitAndFormat(AxisId axisId, Unit unit, IValueFormatter formatter)
    {
        // Implement your immutable replacement semantics.
        return this;
    }

    public IGraphModel ChangeAxisUnits(IReadOnlyDictionary<AxisId, Unit> unitChanges)
    {
        // Implement your immutable replacement semantics.
        return this;
    }
}
```

Guidance:

- Keep these methods deterministic
- Prefer immutable replacement over in-place mutation
- Ensure returned model still satisfies the validity checklist

## Optional: Custom IGraphSnapshotBuilder and IGraphSnapshot

If you need custom snapshot behavior, implement these interfaces:

```csharp
public interface IGraphSnapshotBuilder
{
    IGraphSnapshot Build(IGraphModel graphModel, GraphPresentationOptions options = null);
}

public interface IGraphSnapshotBuilderProvider
{
    IGraphSnapshotBuilder CreateGraphSnapshotBuilder();
}

public interface IGraphSnapshot
{
    IReadOnlyList<ISeriesSnapshot> Series { get; }
    IReadOnlyList<IAxisSnapshot> Axes { get; }
}
```

Minimal custom snapshot pattern:

```csharp
public sealed class CustomGraphSnapshot : IGraphSnapshot
{
    public CustomGraphSnapshot(IReadOnlyList<ISeriesSnapshot> series, IReadOnlyList<IAxisSnapshot> axes)
    {
        Series = series;
        Axes = axes;
    }

    public IReadOnlyList<ISeriesSnapshot> Series { get; }
    public IReadOnlyList<IAxisSnapshot> Axes { get; }
}

public sealed class CustomSnapshotBuilder : IGraphSnapshotBuilder
{
    public IGraphSnapshot Build(IGraphModel graphModel, GraphPresentationOptions options = null)
    {
        // Build custom series and axis snapshots here.
        return new CustomGraphSnapshot(new List<ISeriesSnapshot>(), new List<IAxisSnapshot>());
    }
}

public sealed class CustomSnapshotBuilderProvider : IGraphSnapshotBuilderProvider
{
    public IGraphSnapshotBuilder CreateGraphSnapshotBuilder() => new CustomSnapshotBuilder();
}
```

When custom snapshot building is useful:

- Additional validation or policy
- Instrumentation
- Alternate snapshot derivation logic

If no provider is supplied by a host, the default internal snapshot builder behavior is used.

## End-to-End Construction Example

This is the complete, minimal flow using defaults plus recommended field subclassing.

```csharp
var times = new[] { 0d, 3600d, 7200d, 10800d };
var pressures = new[] { 44.1d, 43.8d, 43.6d, 43.2d };

var xField = new DemandFieldDefinition("time", "Time", Units.Time.Second, times);
var yField = new DemandFieldDefinition("pressure", "Pressure", Units.Pressure.Psi, pressures);

var xAxis = new AxisModel(new AxisId("time"), AxisOrientation.X, AxisSide.Bottom, Units.Time.Second, "sec", new NumericFormatter("fmt-time", UnitsRegistry.Default, "Time", "F0"));
var yAxis = new AxisModel(new AxisId("pressure"), AxisOrientation.Y, AxisSide.Left, Units.Pressure.Psi, "psi", new NumericFormatter("fmt-pressure", UnitsRegistry.Default, "Pressure", "F2"));

var series = new GraphSeriesModel(new SeriesId("pressure-1"), "Pressure", SeriesType.Line, xField, yField, xAxis, yAxis);

IGraphModel graph = new GraphModel(new[] { xAxis, yAxis }, new[] { series });
```

## Summary

A valid graph follows this conceptual flow:

Field Definitions -> Axes -> Series -> Graph Model -> (optional) Snapshot Builder -> Graph Snapshot

Key takeaways:

- Default classes are optional conveniences, not requirements
- Direct interface implementation is acceptable
- Subclassing GraphFieldDefinitionBase is recommended
- Keep IDs, axis mappings, and value arrays coherent
