# Graph Presentation Model Design

## Overview
The GraphPresentationModel represents a device-independent, renderer-agnostic
view of a graph snapshot. It translates snapshot semantics into draw-ready
primitives without introducing new meaning.

## Responsibilities
- Layout axis lines, labels, and gridlines using snapshot data
- Present numeric values already converted into display units
- Consume axis titles directly from AxisSnapshot

## Non-Responsibilities
- No unit conversion
- No mutation of snapshot objects
- No inference of semantic meaning from raw data

## Axis Titles
Axis titles are supplied by IAxisSnapshot and reflect axis display semantics.
The presentation layer must not inspect field snapshots to derive unit labels.

## Design Principles
- Immutable inputs
- Stateless computation
- Deterministic output

This design aligns with enforcement via tests and the EngineeringGraphControl
TestHarness.
