# Design: `IAxisLabelValueConverter` for Formatter Value-Type Aware Axis Labeling

## Status

Draft design for EngineeringGraphControl implementation planning.

This document captures the agreed design for enhancing EngineeringGraphControl so axis tick label formatting can respect `IValueFormatter.ValueType` without changing the numeric geometry model, renderer contracts, or existing default behavior.

## Purpose

EngineeringGraphControl currently supports an `IValueFormatter` abstraction that declares the type of value it formats through `ValueType`. However, the current axis tick label formatting path always passes a `double` coordinate value to the formatter.

This design introduces an optional axis label value conversion seam so EngineeringGraphControl can distinguish between:

- the numeric coordinate value used for geometry, and
- the semantic value supplied to the formatter for label text.

The immediate motivating use case is Personal Ledger balance forecast graphing, where:

- the X-axis represents dates, naturally modeled as `DateOnly`, and
- the Y-axis represents currency/balances, naturally modeled as `decimal`.

The broader goal is to make EngineeringGraphControl more generally capable while preserving its existing graph model, snapshot, presentation, and rendering architecture.

## Background

EngineeringGraphControl is intentionally layered:

```text
GraphModel
    ->
GraphSnapshot
    ->
GraphPresentationModel
    ->
Renderer
```

The existing design separates semantic graph data, immutable snapshot construction, device-agnostic presentation geometry, and renderer-specific drawing.

The current graph presentation layer builds axis tick geometry from numeric values, which is appropriate and should remain unchanged. The design issue is that the same numeric values are also used directly as formatter inputs.

Current conceptual behavior:

```text
Axis min/max/increment
    ->
double tick values
    ->
formatter.Format(double)
```

This effectively ignores `IValueFormatter.ValueType` for axis tick labels.

## Problem Statement

`IValueFormatter` declares a formatter input type:

```csharp
public interface IValueFormatter
{
    FormatterId Id { get; }

    Type ValueType { get; }

    string Format(object value, IFormatProvider formatProvider = null);
}
```

However, axis tick label formatting currently assumes the formatter input is always a `double`.

That means EngineeringGraphControl presently conflates two related but distinct concepts:

```text
Axis coordinate value
Axis label source value
```

For traditional engineering graph scenarios, this is usually fine:

```text
coordinate value:    42.0
label source value:  42.0
formatter type:      double
```

For semantic graph scenarios, the values may differ:

```text
coordinate value:    739434.0
label source value:  DateOnly
formatter type:      DateOnly
```

or:

```text
coordinate value:    1234.56
label source value:  decimal
formatter type:      decimal
```

The current system can still render the graph, but consumers are forced to tunnel semantic values through `double` earlier than necessary.

## Design Goals

- Respect the semantic intent of `IValueFormatter.ValueType`.
- Preserve numeric coordinate geometry.
- Preserve the existing renderer contract.
- Preserve existing default behavior for current EngineeringGraphControl consumers.
- Avoid introducing mandatory complexity for simple numeric graphs.
- Allow consumers to opt into semantic label values only when needed.
- Keep the enhancement aligned with the existing model -> snapshot -> presentation lifecycle.

## Non-Goals

This design does not:

- redesign EngineeringGraphControl;
- change renderer behavior;
- change the abstract Cartesian presentation geometry model;
- require renderers to understand `DateOnly`, `decimal`, or other semantic value types;
- require all axes to use semantic label values;
- remove or replace existing numeric formatter behavior;
- require strict runtime validation between formatter and converter types.

## Core Design Decision

Introduce an optional `IAxisLabelValueConverter` abstraction.

The converter adapts a numeric axis coordinate value into the semantic value that should be passed to the axis formatter.

```text
Coordinate value
    ->
optional label value converter
    ->
formatter input value
    ->
formatter.Format(...)
```

If no converter is provided, EngineeringGraphControl preserves existing behavior by passing the coordinate `double` directly to the formatter.

## Proposed Contract

```csharp
public interface IAxisLabelValueConverter
{
    Type TargetValueType { get; }

    object Convert(double coordinateValue, IFormatProvider formatProvider = null);
}
```

### Contract Semantics

- `TargetValueType` describes the semantic type produced by the converter.
- `Convert` receives the numeric coordinate-domain value used by the graph geometry.
- `Convert` returns the semantic value to pass into `IValueFormatter.Format`.
- The converter is used only for axis tick label formatting.
- The converter does not affect series geometry, axis geometry, renderer behavior, or coordinate calculations.

## Axis Model Integration

`IAxisModel` should expose an optional converter:

```csharp
public interface IAxisModel
{
    // Existing members omitted for brevity.

    IValueFormatter Formatter { get; }

    IAxisLabelValueConverter LabelValueConverter { get; }
}
```

The default axis model implementation should support the new property.

### Default Behavior

If `LabelValueConverter` is `null`, axis tick label formatting behaves exactly as it does today.

```text
No converter
    ->
formatter.Format(doubleCoordinateValue)
```

This preserves backward compatibility for existing consumers and existing `NumericFormatter` usage.

## Snapshot Integration

The converter should flow through the immutable snapshot layer.

Conceptually:

```text
AxisModel.LabelValueConverter
    ->
AxisSnapshot.LabelValueConverter
    ->
GraphPresentationModel.BuildAxisTicks(...)
```

`IAxisSnapshot` and `AxisSnapshot` should therefore carry the optional converter:

```csharp
public interface IAxisSnapshot
{
    // Existing members omitted for brevity.

    IAxisLabelValueConverter LabelValueConverter { get; }
}
```

`GraphSnapshotBuilder` should copy the converter from the source axis model into the axis snapshot.

This follows the existing model -> snapshot -> presentation lifecycle and keeps axis label formatting metadata immutable once the snapshot is built.

## Presentation Layer Integration

`GraphPresentationModel` should continue generating tick coordinate values as `double` values.

Current conceptual flow:

```text
BuildTickValues(...)
    ->
FormatAxisLabel(formatter, doubleValue)
```

Proposed conceptual flow:

```text
BuildTickValues(...)
    ->
ResolveAxisLabelValue(converter, doubleCoordinateValue)
    ->
FormatAxisLabel(formatter, resolvedLabelValue)
```

Example shape:

```csharp
private static object ResolveAxisLabelValue(
    IAxisLabelValueConverter converter,
    double coordinateValue,
    IFormatProvider formatProvider = null)
{
    if (converter == null)
    {
        return coordinateValue;
    }

    return converter.Convert(coordinateValue, formatProvider);
}

private static string FormatAxisLabel(
    IValueFormatter formatter,
    object value,
    IFormatProvider formatProvider = null)
{
    if (formatter != null)
    {
        return formatter.Format(value, formatProvider);
    }

    return Convert.ToString(value, CultureInfo.InvariantCulture);
}
```

The important distinction is:

```text
Tick geometry uses the coordinate value.
Tick label text uses the resolved label value.
```

## Validation Policy

The system should not throw solely because a converter's `TargetValueType` does not exactly match the formatter's `ValueType`.

Reasoning:

- Consumers may intentionally compose converters and formatters in flexible ways.
- Formatter implementations already have the ability to validate input values.
- Hard validation could make the framework unnecessarily restrictive.

However, diagnostic output may be useful.

A debug-only diagnostic may be added when the resolved label value type appears incompatible with the formatter's declared `ValueType`.

Conceptual example:

```csharp
if (formatter != null && labelValue != null && formatter.ValueType != null)
{
    var actualType = labelValue.GetType();
    if (!formatter.ValueType.IsAssignableFrom(actualType))
    {
        Debug.WriteLine(
            string.Format(
                CultureInfo.InvariantCulture,
                "Axis label formatter '{0}' declares value type '{1}', but received value type '{2}'.",
                formatter.Id,
                formatter.ValueType.FullName,
                actualType.FullName));
    }
}
```

This should inform without preventing execution.

> ***_NOTE:_*** This design intentionally favors consumer flexibility over strict framework enforcement. If a formatter cannot handle the supplied value, the formatter remains free to throw its own domain-appropriate exception.

## Example: DateOnly X-Axis

A balance forecast X-axis may use numeric day numbers for geometry while formatting tick labels as actual dates.

### Converter

```csharp
public sealed class DateOnlyDayNumberAxisLabelValueConverter
    : IAxisLabelValueConverter
{
    public Type TargetValueType
    {
        get { return typeof(DateOnly); }
    }

    public object Convert(double coordinateValue, IFormatProvider formatProvider = null)
    {
        var dayNumber = checked((int)Math.Round(coordinateValue));
        return DateOnly.FromDayNumber(dayNumber);
    }
}
```

### Formatter

```csharp
public sealed class DateOnlyValueFormatter : IValueFormatter
{
    public FormatterId Id { get; }

    public Type ValueType
    {
        get { return typeof(DateOnly); }
    }

    public string Format(object value, IFormatProvider formatProvider = null)
    {
        if (!(value is DateOnly date))
        {
            throw new ArgumentException("Value must be of type DateOnly.", nameof(value));
        }

        var provider = formatProvider ?? CultureInfo.CurrentCulture;
        return date.ToString("d", provider);
    }
}
```

### Result

```text
coordinate value:    739434.0
converted value:     DateOnly.FromDayNumber(739434)
formatter receives:  DateOnly
label text:          culture-aware date string
```

## Example: Decimal / Currency Y-Axis

A balance forecast Y-axis may use numeric values for geometry while formatting tick labels as decimal currency values.

### Converter

```csharp
public sealed class DecimalAxisLabelValueConverter
    : IAxisLabelValueConverter
{
    public Type TargetValueType
    {
        get { return typeof(decimal); }
    }

    public object Convert(double coordinateValue, IFormatProvider formatProvider = null)
    {
        return System.Convert.ToDecimal(coordinateValue, CultureInfo.InvariantCulture);
    }
}
```

### Formatter

```csharp
public sealed class CurrencyValueFormatter : IValueFormatter
{
    public FormatterId Id { get; }

    public Type ValueType
    {
        get { return typeof(decimal); }
    }

    public string Format(object value, IFormatProvider formatProvider = null)
    {
        if (!(value is decimal amount))
        {
            throw new ArgumentException("Value must be of type decimal.", nameof(value));
        }

        var provider = formatProvider ?? CultureInfo.CurrentCulture;
        return amount.ToString("C", provider);
    }
}
```

### Result

```text
coordinate value:    1234.56
converted value:     1234.56m
formatter receives:  decimal
label text:          culture-aware currency string
```

## Backward Compatibility

Existing consumers remain compatible because:

- `LabelValueConverter` is optional.
- The null-converter behavior passes the existing `double` coordinate value to the formatter.
- Existing numeric formatters can continue declaring `ValueType == typeof(double)`.
- Axis geometry, series geometry, and renderer behavior remain numeric.
- Existing graph snapshots without semantic converter behavior continue to behave as before.

## Relationship to Existing EngineeringGraphControl Design

This enhancement reinforces the existing EngineeringGraphControl architecture rather than replacing it.

### GraphModel

The graph model remains the semantic source of graph structure, axes, series, and fields.

### GraphSnapshot

The snapshot becomes the immutable carrier of the axis label value converter selected by the axis model.

### GraphPresentationModel

The presentation model continues to build numeric geometry, but it resolves semantic values before formatting axis tick labels.

### Renderer

Renderers remain unaware of semantic value types. They receive presentation geometry and rendered label text, as before.

## Design Summary

`IAxisLabelValueConverter` separates coordinate-domain values from formatter-domain values.

The central rule is:

```text
Coordinates remain numeric.
Labels may be semantic.
```

This allows EngineeringGraphControl to honor the existing `IValueFormatter.ValueType` abstraction without disrupting rendering, geometry, layout, or existing numeric graph behavior.

## Recommended Implementation Phases

### EGC1 — Add Converter Contract

- Add `IAxisLabelValueConverter`.
- Place it near related formatting or axis model abstractions.
- Add minimal tests for simple converter behavior if appropriate.
- No behavioral changes yet.

### EGC2 — Add Axis Model and Snapshot Support

- Add optional `LabelValueConverter` to `IAxisModel`.
- Add support to the default `AxisModel`.
- Add optional `LabelValueConverter` to `IAxisSnapshot` and `AxisSnapshot`.
- Update `GraphSnapshotBuilder` to copy the converter from axis model to axis snapshot.
- Preserve null/default behavior.

### EGC3 — Update Axis Tick Label Formatting

- Update `BuildAxisTicks` to receive the optional converter.
- Resolve formatter input value from the coordinate value before calling `formatter.Format`.
- Change `FormatAxisLabel` to accept `object` instead of `double`.
- Preserve existing behavior when no converter is present.
- Add debug diagnostic output for apparent formatter/converter value-type mismatches.

### EGC4 — Add Semantic Test Scenarios

- Add a DateOnly-axis test scenario.
- Add a decimal/currency-axis test scenario.
- Verify that formatters receive semantic values when converters are supplied.
- Verify that existing numeric/double scenarios remain unchanged.

### EGC5 — Consume from Personal Ledger

- Update Personal Ledger balance forecast graphing to model dates and balances more semantically.
- Use the EGC converter/formatter pipeline for DateOnly and decimal/currency axis labels.
- Avoid premature tunneling of projection values into `double` inside projection builder logic where possible.

## Acceptance Criteria

- Existing numeric EngineeringGraphControl scenarios continue to work unchanged.
- Axes without `LabelValueConverter` continue passing `double` coordinate values to formatters.
- Axes with `LabelValueConverter` pass converted semantic values to formatters.
- `IValueFormatter.ValueType` is meaningfully supported by the axis tick label pipeline.
- Tick and series geometry remain numeric.
- Renderers do not require changes to support semantic label values.
- The graph lifecycle remains model -> snapshot -> presentation -> renderer.
- No hard validation exception is introduced solely for formatter/converter type mismatch.
- Optional debug diagnostics may report apparent formatter/converter type mismatches.

## Future ADR Candidate

After implementation is complete, create an EngineeringGraphControl ADR documenting the final accepted behavior.

Possible ADR title:

```text
ADR-00XX: Formatter Value-Type Aware Axis Labeling
```

The ADR should be reality-based and should document the implemented API surface, compatibility behavior, diagnostics policy, and any deviations from this design.
