# ADR-0004 Addendum: Series Presentation Intent and Legend Glyph Semantics

## Status
Accepted (Addendum)

## Context
ADR-0004 defines layout semantics and space ownership within the Graph
Presentation Model, clearly separating structural layout concerns (axes,
legends, titles) from plot-area content. Subsequent work introduced
`LineRenderMode` to express presentation intent for line series, enabling
LineOnly, PointsOnly, and LineAndPoints behaviors.

While `LineRenderMode` does not alter layout ownership or pressure resolution,
it introduces new *presentation semantics* that must be reflected consistently
across both plot geometry and symbolic representations such as legend glyphs.

## Decision
Legend glyphs must reflect series presentation intent without affecting layout
semantics or plot geometry. For `SeriesType.Line`, legend glyph selection is
based on `LineRenderMode`:

- **LineOnly**: render a line-only legend glyph
- **PointsOnly**: render a point-only legend glyph
- **LineAndPoints**: render a combined line + point glyph

Non-line series (e.g., Scatter) retain their existing legend behavior.

This decision introduces explicit legend-presentation semantics while preserving
all layout invariants defined in ADR-0004.

## Consequences
- Legend glyphs accurately communicate how a series is presented in the plot
  area.
- No changes are made to layout ownership, band sizing, or pressure resolution.
- Plot renderers remain responsible only for rasterizing geometry; legend
  semantics are resolved earlier in the presentation pipeline.
- Future presentation intents can extend legend semantics without impacting
  layout or renderer architecture.

## Relationship to ADR-0004
This addendum clarifies how *presentation intent* influences symbolic
representation (legend glyphs) while remaining fully compliant with the layout
and space ownership principles established by ADR-0004.
