
# Axis Scaling – As Implemented Design Notes

## Purpose

This document describes the **as-implemented design** of numeric axis scaling within EngineeringGraphControl (EGC), with specific focus on Y-axis behavior across different display units (e.g., meters, feet, inches).

It exists to record *intentional behavior* and *key invariants* discovered and validated during implementation, particularly where correctness depends on ordering, scale invariance, and responsibility boundaries rather than obvious arithmetic.

This document is **developer-facing** and is not an ADR. It complements **ADR-0003: Graph Presentation Model – Abstract Geometry Plane** by explaining how axis scaling is concretely realized in code.

---

## Scope and Non-Goals

### In Scope

- How axis min/max values are chosen
- How numeric intervals are selected
- How scaling remains invariant across units and magnitudes
- How responsibilities are split between policy, presentation, and renderer layers

### Explicitly Out of Scope

- Unit conversion or storage-unit concerns
- Renderer pixel mapping details
- End-user UX or product-level behavior

All logic described here operates **entirely in display units**.

---

## Architectural Context

Per ADR-0003:

- The **GraphPresentationModel** represents an *abstract geometry plane*
- All coordinates are semantic, renderer-agnostic values
- Renderers must not infer or reinterpret layout semantics

Axis scaling follows this same principle:

> Axis shape and semantics are fully determined before rendering begins.

---

## High-Level Axis Scaling Pipeline

The numeric Y-axis scaling pipeline proceeds as follows:

1. **FieldSnapshot**
   - Supplies numeric values already expressed in the selected display unit
   - No knowledge of storage or canonical units exists at this stage

2. **AxisRangeCalculator**
   - Computes raw numeric bounds (candidate min/max)
   - Performs no tick-density or increment selection

3. **AxisPolicyApplier**
   - Applies dense numeric policy and overrides
   - Selects minor increment and major tick stride
   - Anchors axis min/max to major interval boundaries

4. **GraphPresentationModel**
   - Emits only *major* ticks as presentation geometry
   - Gridlines, tick marks, and labels are 1:1:1

5. **Renderer**
   - Draws exactly the supplied geometry
   - Performs no density inference or cadence adjustment

---

## Dense Increment Selection (Scale-Invariant)

### Problem Addressed

A naive dense-increment algorithm that operates directly on raw numeric spans can behave incorrectly when the same data is expressed at different numeric scales (e.g., inches vs feet).

Correct behavior requires that the *shape* of the axis remain identical regardless of unit magnitude.

### Key Invariant

> Dense increment selection must be **scale-invariant** within the abstract geometry plane.

### As-Implemented Solution

Dense increment selection operates by factoring the numeric span into an order-of-magnitude representation:

1. Compute span:

   ```text
   span = axisMax - axisMin
   ```

2. Factor span into magnitude and mantissa:

   ```text
   exponent = floor(log10(span))
   mantissa = span / 10^exponent
   ```

3. Apply the standard `1–2–2.5–5–10` ladder **to the mantissa**
4. Select the best candidate based on existing density heuristics
5. Scale the chosen value back using the exponent

All operations remain in display units; no unit conversion occurs.

### Result

The same dataset produces identical axis geometry when expressed in:

- meters
- feet
- inches

Differing only by numeric scale, not by layout semantics.

---

## Minor vs Major Ticks

### Definitions

- **Minor Increment**: Internal numeric increment used only for policy calculation
- **Major Tick Interval**: Visual interval formed by:

  ```text
  majorInterval = minorIncrement × MajorTickStride
  ```

### Visual Semantics

For numeric Y-axes:

- Only **major ticks** are emitted downstream
- Gridlines, tick marks, and labels all appear at the major interval
- Minor ticks are never rendered

This ensures clear, uncluttered visuals and deterministic behavior.

---

## Axis Min/Max Anchoring

Once the major interval is known, axis bounds are anchored to **major interval boundaries**:

```text
axisMin = axisMax - floor((axisMax - actualMin) / majorInterval) × majorInterval
```

### Rationale

- Guarantees inclusion of the lowest labeled tick
- Prevents off-boundary starts such as `6915` inches instead of `6900`
- Required for correctness at large numeric scales

---

## Presentation Model Responsibilities

The presentation model is the **sole authority** for deciding:

- Which ticks exist
- Where gridlines exist
- Where labels are placed

It emits only the geometry that should appear visually.

---

## Renderer Responsibilities

Renderers:

- Convert abstract geometry to device space
- Draw exactly what is supplied
- Must not infer spacing, density, or cadence

This separation is critical to maintaining correctness.

---

## Invariants and Guardrails

The following invariants must always hold:

- Axis geometry is invariant under unit scaling
- No unit-specific branching exists in axis scaling logic
- Renderer performs no tick density inference
- Minor increments never directly drive visual output

Violating any of the above is considered a correctness regression.

---

## Relationship to ADR-0003

ADR-0003 defines *where* layout semantics live.

This document describes *how* that principle is implemented for numeric axis scaling.

It should be read as an implementation companion to ADR-0003.

---

## Closing Note

The axis scaling logic described here was validated against multiple units and magnitudes and is intentionally conservative. Behavior that may appear more complex than strictly necessary is the result of hard-learned correctness constraints, not accidental complexity.

Future changes to this area should be evaluated against the invariants listed above.
