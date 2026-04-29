## ADR-0006: Axis Identity, Insets, and Renderer Mapping Invariants

### Status

Accepted

### Context

ADR-0003, ADR-0004, and ADR-0005 together define the geometric plane, layout semantics,
and layout-pressure behavior of the GraphPresentationModel. These decisions correctly
separate abstract geometry, space ownership, and rendering concerns.

During the implementation of multi–Y-axis support (including stacked left Y-axes and
a single right Y-axis), several difficult rendering defects were encountered that could
not be consistently explained or corrected through renderer-local fixes alone.

Root-cause analysis revealed that these defects stemmed from **previously implicit
invariants** related to:

- Axis identity resolution
- Axis inset (padding) semantics
- The interaction between axis stacking and renderer coordinate mapping

Because these invariants were not formally documented, violations produced symptoms
that closely resembled renderer bugs despite being model- or contract-level errors.

This ADR records those invariants explicitly to prevent future regressions.

---

### Decision

The following invariants are now **architecturally required**.

---

#### 1. Axis Identity Must Be Unique Within a Graph

Each axis within a graph MUST have a unique `AxisId`.

- Axis resolution for series binding relies on `AxisId` matching.
- If multiple axes share an `AxisId`, series binding becomes ambiguous and may silently
  resolve to an unintended axis.
- Such ambiguity can manifest as incorrect normalization, apparent stacking leakage,
  or incorrect right-axis behavior at render time.

**Enforcement**

- AxisId uniqueness SHALL be validated during graph snapshot construction.
- GraphSnapshotBuilder MUST fail fast if duplicate AxisIds are detected.

This is a model integrity requirement, not a renderer concern.

---

#### 2. Axis Insets Are Orthogonal to Axis Stacking

Axis inset (padding for tick labels, titles, and visual clarity) and axis stacking serve
different purposes and MUST be applied independently.

- **Axis Insets**
  - Apply to all axes (left, right, top, bottom)
  - Reserve space for tick labels and titles
  - Reduce the usable axis rectangle uniformly

- **Axis Stacking**
  - Applies ONLY to left Y-axes
  - Splits an already-inset axis rectangle into vertical bands
  - Must never influence right-axis geometry

These concerns MUST NOT be combined into a single conditional.

---

#### 3. Renderer Mapping Pipeline Is Uniform

All renderer geometry (series, grid lines, ticks) MUST use the same mapping pipeline:
```
Domain value
→ axis-specific domain normalization
→ axis rectangle (after inset, after stacking as applicable)
→ device-space transform
```

Specifically:

- No geometry may embed pre-positioned offsets (e.g., anchor points)
- Axis side affects:
  - tick direction
  - stacking eligibility
- Axis side does NOT affect:
  - inset application
  - domain normalization logic

---

### Consequences

#### Positive

- Prevents ambiguous axis resolution at the model boundary
- Eliminates an entire class of “phantom renderer bugs”
- Guarantees correct behavior for:
  - stacked left Y-axes
  - independent right Y-axes
  - multi-axis grid alignment
- Aligns renderer implementation strictly with ADR‑0003/0004 contracts

#### Costs

- Early validation may reject previously tolerated (but invalid) graph models
- Requires careful separation of inset and stacking logic in renderer code

These costs are accepted as necessary to preserve correctness and architectural clarity.

---

### Relationship to Other ADRs

- **[ADR-0003](ADR-0003-Graph-Presentation-Model-Abstract-Geometry-Plane.md)** defines the abstract geometry plane this ADR operates within
- **[ADR-0004](ADR-0004-Graph-Presentation-Model-Layout-Semantics-and-Space-Ownership.md)** defines space ownership rules that make axis insets mandatory
- **[ADR-0005](ADR-0005-Layout-Pressure-Resolution-and-Degradation-Policy.md)** ensures inset behavior degrades correctly under layout pressure

ADR‑0006 does not override these decisions; it makes their hidden assumptions explicit.

---

### Notes

This ADR intentionally documents *invariants*, not implementation strategies. Any future
renderer or PresentationModel refactor MUST preserve these invariants, even if visual
behavior appears acceptable without them.