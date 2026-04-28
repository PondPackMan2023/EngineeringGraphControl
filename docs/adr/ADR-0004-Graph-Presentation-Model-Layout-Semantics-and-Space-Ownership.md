# ADR-0004: GraphPresentationModel Layout Semantics and Space Ownership

## Status
Accepted

## Context

ADR-0003 formally defines the **abstract geometric plane** for the `GraphPresentationModel`:
- Cartesian coordinates
- (0,0) bottom-left, (1,1) top-right
- X increases right, Y increases upward
- Renderer-agnostic geometry

While ADR-0003 establishes a necessary geometric foundation, experience implementing advanced layout behavior (titles, subtitles, axes, legends, plot resizing) has demonstrated that **geometry alone is insufficient to guarantee correct or predictable layout behavior**.

Specifically, the absence of explicitly defined **layout semantics** has resulted in:
- Overlap between legends and axis titles
- Ambiguity about which regions may move or resize
- Confusion over whether elements consume space or merely shift position
- Repeated regressions when fixing localized layout issues

To ensure deterministic, correct, and maintainable layout behavior, the PresentationModel must define **ownership of space** in addition to geometry.

This ADR establishes those rules.

---

## Decision

The `GraphPresentationModel` SHALL enforce explicit **layout semantics and space-ownership rules** layered on top of the geometric plane defined in ADR-0003.

These rules define **which regions are immutable, which regions may resize, and how layout elements interact spatially**.

---

## Conceptual Layout Regions

The presentation surface is divided into **explicit structural bands**. These bands exist conceptually even if some are zero-sized.

Layout semantics are defined independently for **vertical** and **horizontal** directions.

---

## Vertical Layout Semantics (Top / Bottom)

### Vertical Band Ordering (Highest Y → Lowest Y)

```text
[ Control Top Edge ]
[ Top Legend Band      ]  ← optional, inserts space
[ Title Band           ]  ← immutable
[ Subtitle Band        ]  ← immutable
[ Plot Area            ]  ← resizable
[ Axis Bands (Bottom)  ]  ← immutable
[ Bottom Legend Band   ]  ← optional, inserts space
[ Control Bottom Edge ]
```

### Vertical Rules

- Title and subtitle bands are **immutable** once defined
- Axis bands are **immutable** once defined
- Legends inserted at Top or Bottom are **insertion bands**, not overlays (when `ResizeChart = true`)
- Legend insertion **reduces the plot area height** but MUST NOT move immutable bands
- Plot area boundaries move **inward only**

---

## Horizontal Layout Semantics (Left / Right)

### Horizontal Band Ordering (Lowest X → Highest X)

```text
[ Control Left Edge ]
[ Left Legend Band     ]  ← optional, inserts space
[ Axis Bands (Left)    ]  ← immutable (axis titles, tick labels)
[ Plot Area            ]  ← resizable
[ Axis Bands (Right)   ]  ← immutable (axis titles, tick labels)
[ Right Legend Band    ]  ← optional, inserts space
[ Control Right Edge ]
```

### Horizontal Rules

- Axis-title bands on the Left and Right are **immutable** once defined
- Legends inserted on the Left or Right are **insertion bands**, not overlays (when `ResizeChart = true`)
- Legend insertion **reduces the plot area width** but MUST NOT move axis-title geometry
- Plot area boundaries move **inward only**
- Legends MUST NOT be placed inside axis-title bands

---

## Space Ownership Rules (Authoritative)

### Immutable Regions

The following regions are **immutable once defined**:

- Title band
- Subtitle band
- Axis-title bands (left, right, top, bottom)

Immutable means:
- Their geometry is calculated once
- Their position MUST NOT change in response to legend placement or plot resizing

---

### Resizable Region

The **plot area** is the *only* region that may shrink or expand:

- Legend placement with `ResizeChart = true` reduces plot area size
- Axis bands reduce plot area size
- Title and subtitle reduce plot area size

Plot area boundaries move 
**inward only**; they never expand past immutable bands.

---

### Legend Semantics

Legends are **insertion elements**, not overlays (when `ResizeChart = true`).

Rules:

- Legend placement inserts a **legend band** adjacent to the plot area
- Legend bands consume space between the control edge and immutable regions
- Legends MUST NOT overlap axis-title bands, title bands, or subtitle bands
- Legends MUST NOT move immutable geometry
- Legends MAY only reduce plot area bounds

When `ResizeChart = false`, legends may overlap the plot area but still must respect immutable regions.

---

## Directional Semantics (Derived from ADR-0003)

Legend insertion follows strict directional rules:

| Placement | Effect on Plot Area | Direction |
|---------|------------------|-----------|
| Top     | Shrinks from top | −Y |
| Bottom  | Shrinks from bottom | +Y |
| Left    | Shrinks from left | +X |
| Right   | Shrinks from right | −X |

Plot area shrinks monotonically inward.

---

## Prohibited Behaviors

The GraphPresentationModel MUST NOT:

- Move title or subtitle geometry to accommodate legends
- Place legends within axis-title bands
- Allow legends to overlap immutable regions
- Shrink plot area more than once for the same legend placement
- Use rendering-specific assumptions to resolve layout conflicts

---

## Consequences

### Positive

- Deterministic layout behavior
- Elimination of legend/axis/title overlap bugs
- Clear separation between geometry, layout semantics, and rendering
- Simplified reasoning and debugging
- Stable foundation for future layout features

### Cost

- Requires auditing existing layout code against these rules
- May require refactoring to separate geometry calculation from semantic enforcement

---

## Relationship to Other ADRs

- [ADR-0002: Graph Presentation Model Layering and Lifecycle](ADR-0002-Graph-Presentation-Model-Layering-and-Lifecycle.md)
- [ADR-0003: GraphPresentationModel Abstract Geometry Plane](ADR-0003-Graph-Presentation-Model-Abstract-Geometry-Plane.md)

ADR-0004 builds upon ADR-0003 by defining *how space is owned and consumed* within the defined geometric plane.

---

## Notes

This ADR intentionally defines *layout invariants*, not specific implementation techniques.
No future feature or fix should violate these invariants, even if visual output appears acceptable.

This document represents a hard semantic contract, not a tuning guide.
