# Phase 0 – Graph Snapshot Contracts: Immutability Strategy

## Overview

Phase 0 defines four immutable snapshot types that capture the static state of a graph at a point in time:

- **GraphSnapshot**: Root snapshot containing all series
- **SeriesSnapshot**: A single data series with metadata and fields
- **FieldSnapshot**: A single field with unit and formatter metadata
- **AxisSnapshot**: Axis metadata and labels

## Immutability Enforcement Mechanism

All snapshot types use the same immutability pattern, compatible with net48 and net6.0-windows:

### Pattern

```csharp
public sealed class FieldSnapshot
{
    private readonly string _name;
    private readonly string _axisId;

    public string Name { get { return _name; } }
    public string AxisId { get { return _axisId; } }

    public FieldSnapshot(string name, string axisId)
    {
        _name = name;
        _axisId = axisId;
    }
}
```

### Immutability Guarantees

1. **Private readonly fields**: Store all state; cannot be modified after construction.
2. **Read-only properties**: Expose fields through getters only; no setters.
3. **No base class**: All snapshots are `sealed` to prevent subclass circumvention.
4. **No virtual members**: No extension points; no inherited mutation paths.
5. **Constructor initialization**: All state set exactly once during construction.

### Collection Immutability

Collections are immutable via `ReadOnlyCollection<T>`:

```csharp
private readonly IReadOnlyList<FieldSnapshot> _fields;

public IReadOnlyList<FieldSnapshot> Fields
{
    get { return _fields; }
}

public SeriesSnapshot(int id, string label, ChartType chartType, IEnumerable<FieldSnapshot> fields)
{
    _fields = new ReadOnlyCollection<FieldSnapshot>(
        new List<FieldSnapshot>(fields ?? Array.Empty<FieldSnapshot>())
    );
}
```

- Input is defensively copied into a new list.
- Returned via `IReadOnlyList<T>`, preventing modification.
- No public access to the underlying collection.

## Snapshot-Facing Interfaces

Optional interfaces (`IFieldSnapshot`, `IAxisSnapshot`, `ISeriesSnapshot`, `IGraphSnapshot`) provide:

- Read-only consumption contracts
- No behavior or default implementations
- Pure data exposure
- Support for dependency injection or abstraction layers

These interfaces are **not required** in Phase 0; they are provided for future extensibility.

## Non-Scope Constraints Met

✅ No snapshot builders or factories  
✅ No references to `IGraphModel` or `IGraphSeriesModel`  
✅ No axis grouping, derivation, or computation  
✅ No units, numeric formatters, or validation logic  
✅ No helper methods, virtual methods, or inheritance hierarchies  
✅ All snapshot types sealed  
✅ No dependency on OpenFlows runtime types  
✅ Axis identity stored explicitly; never inferred  

## Files Created

- `FieldSnapshot.cs`: Immutable field snapshot
- `AxisSnapshot.cs`: Immutable axis snapshot
- `SeriesSnapshot.cs`: Immutable series snapshot with field collection
- `GraphSnapshot.cs`: Immutable graph snapshot with series collection
- `IFieldSnapshot.cs`: Read-only interface for field consumption
- `IAxisSnapshot.cs`: Read-only interface for axis consumption
- `ISeriesSnapshot.cs`: Read-only interface for series consumption
- `IGraphSnapshot.cs`: Read-only interface for graph consumption

## Namespace

All types are in `Graphing.Controls.Snapshot` within the `Graphing.Controls` project.

## Compilation Targets

- .NET Framework 4.8 (net48)
- .NET 6.0 Windows (net6.0-windows)

Both targets support the immutability patterns used.
