# ADR-0005: Layout Pressure Resolution, Band Growth, and Degradation Policy

## Status
Accepted

---

## Context

ADR-0003 defines the **abstract geometric plane** used by the GraphPresentationModel.  
ADR-0004 defines **space ownership and layout semantics**, including immutable bands,
plot-area shrinkage, and insertion behavior.

Together, these ADRs establish *where* layout occurs and *who owns which regions*.

However, neither ADR fully defines what happens when the total space demanded by
layout bands (titles, axis titles, legends, etc.) exceeds the available bounds.
This situation arises commonly due to:

- Multi-line titles and subtitles
- Long or rotated axis titles
- Large legends
- Small control sizes or embed contexts

Without an explicit policy, layout pressure results in:
- Silent overlap
- Zero-sized plot areas
- Renderer-side “fixes”
- Violations of the PresentationModel/Renderer contract

This ADR defines how layout pressure is resolved in a **deterministic, layered,
and enforceable way**, without leaking rendering concerns into the PresentationModel.

---

## Decision

The GraphPresentationModel SHALL enforce an explicit **layout pressure resolution
policy** with the following characteristics:

1. The PlotArea acts as the **sole sink** for layout pressure.
2. A **minimum viable PlotArea size** is enforced by the PresentationModel.
3. Layout bands may **request growth**, but growth is granted only while sufficient
   PlotArea remains.
4. When space is insufficient, layout elements degrade or disappear according to a
   **defined priority order**.
5. Size measurement is provided by the renderer only as **advisory input**; all
   allocation decisions are made by the PresentationModel.

---

## Minimum Viable PlotArea

The PlotArea MUST NOT be allowed to shrink below a minimum viable size.

- The minimum size is expressed in **abstract model units**, not pixels.
- This minimum guarantees:
  - Valid coordinate mappings
  - Non-degenerate geometry
  - Stable downstream layout behavior

Reaching the minimum PlotArea does not constitute a rendering error; it constitutes
a **layout-pressure boundary condition** handled by policy.

---

## Growable vs Fixed Bands

Layout bands are classified by *structural existence* and *growability*.

### Structurally Existing Bands

The following bands exist as first-class structural entities in the
PresentationModel:

- Title
- Subtitle
- Axis Title (Left / Right / Bottom / Top)
- Legend
- PlotArea

### Growability Rules

| Band        | Growable | Growth Direction                |
|-------------|----------|---------------------------------|
| Title       | Yes      | Vertical                        |
| Subtitle    | Yes      | Vertical                        |
| Axis Title  | Yes      | Orthogonal to axis orientation  |
| Legend      | Yes      | Direction of insertion          |
| PlotArea    | No       | N/A (shrinkable only)           |

Tick labels and grid lines do **not** own bands and are not growable; they are rendered
within axis-title or plot regions.

---

## Layout Pressure and Withdrawal Model

Layout proceeds as follows:

1. The PlotArea is initialized to the full available bounds.
2. Structural bands claim space from the PlotArea according to ADR-0004 ordering.
3. Growable bands request additional thickness based on content needs.
4. Each request is evaluated against remaining PlotArea.
5. If granting a request would violate the minimum PlotArea, the request is limited
   or denied according to degradation priority.

At no point may a band overlap another band, nor may the renderer adjust geometry
after this process.

---

## Degradation Priority Order

When available space is insufficient, layout elements degrade in the following order
(from lowest to highest priority):

1. **Legend**
2. **Axis Titles**
3. **Axis Tick Labels**
4. **Subtitle**
5. **Title**

Higher-priority elements are preserved longer and degraded last.

---

## Degradation Semantics by Band

### Legend
- Stops growing
- May reduce content
- May switch to overlay mode (if permitted)
- May be omitted entirely

### Axis Titles
- Stop growing beyond minimum
- May clip or elide text
- May be hidden if required to preserve minimum PlotArea

### Axis Tick Labels
- May rotate, skip, or clip
- May be hidden while preserving ticks

### Subtitle
- May clip or be omitted

### Title
- Last to be clipped or omitted
- If even the Title cannot fit without violating minimum PlotArea, the layout is
  considered **overconstrained**, but geometry invariants still hold

---

## Measurement and Growth Resolution Contract

### Renderer Responsibilities

The renderer MAY:
- Measure text size based on font, DPI, rotation, and wrapping
- Report **minimum required thickness** per band

The renderer MUST NOT:
- Allocate space
- Reorder bands
- Shrink the PlotArea
- Apply padding or margins beyond provided geometry

### PresentationModel Responsibilities

The PresentationModel SHALL:
- Decide whether growth requests are granted, limited, or denied
- Enforce priority-based degradation
- Preserve all structural and ordering invariants
- Emit final geometry for all bands

Measurement input is advisory, not authoritative.

---

## Invariants

The following invariants MUST always hold:

- PlotArea never shrinks below its minimum viable size
- Only the PlotArea absorbs layout pressure
- Bands never overlap
- Renderer never modifies PresentationModel geometry
- Layout behavior is deterministic for a given input

---

## Consequences

### Positive

- Deterministic and testable layout behavior
- Graceful degradation under extreme constraints
- Clear separation of concerns
- Elimination of renderer-side layout fixes
- Robust support for dynamic text and resizing

### Costs

- Additional logic in the PresentationModel to resolve growth and priority
- Explicit handling of overconstrained layouts

These costs are accepted as necessary to achieve correctness and architectural clarity.

---

## Related Decisions

- [ADR-0002: Graph Presentation Model Layering and Lifecycle](ADR-0002-Graph-Presentation-Model-Layering-and-Lifecycle.md)
- [ADR-0003: GraphPresentationModel Abstract Geometry Plane](ADR-0003-Graph-Presentation-Model-Abstract-Geometry-Plane.md)
- [ADR-0004: GraphPresentationModel Layout Semantics and Space Ownership](ADR-0004-Graph-Presentation-Model-Layout-Semantics-and-Space-Ownership.md)

ADR-0005 completes the layout architecture by defining how the system behaves under
space pressure while preserving all prior guarantees.