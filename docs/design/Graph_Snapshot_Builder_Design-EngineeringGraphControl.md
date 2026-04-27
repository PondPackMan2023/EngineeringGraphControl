# Graph Snapshot Builder Design

## Purpose
The GraphSnapshotBuilder is responsible for projecting an immutable GraphModel
into a concrete GraphSnapshot suitable for presentation and rendering.

## Key Responsibilities

### Field Data
- Field definitions provide **storage / working units**
- FieldSnapshot values are derived from field definitions during snapshot build
- FieldSnapshot values are **converted explicitly** to axis display units during binding

### Axis Semantics
- AxisModel defines display semantics, including display units and orientation
- AxisSnapshot represents the resolved axis state for a given graph snapshot

### Unit Conversion
Unit conversion occurs **only** during snapshot building, when binding field data
to an axis:

- Storage units come from IGraphFieldDefinition
- Display units come from IAxisModel
- Converted values are included in the FieldSnapshot used by rendering

No implicit assumptions are made that storage units equal display units.

### Axis Titles
Axis titles are derived from axis semantics:

```
<Axis Label> (<Axis Display Unit>)
```

FieldSnapshots do not own display-unit labels and must not influence axis title text.

## Invariants
- Snapshots are immutable
- All conversions happen exactly once per snapshot build
- Replacing an axis (e.g., ChangeAxisUnit) requires a full snapshot rebuild

## Rationale
Explicit conversion and axis-centric semantics prevent subtle bugs, support
multi-axis scenarios, and ensure correctness when display preferences change.
