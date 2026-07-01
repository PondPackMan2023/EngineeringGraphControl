# ADR-0001: Introduction of EngineeringGraphControl

## Status

Accepted

---

## Context

Historically, graphing infrastructure in this codebase was tightly coupled to **OpenFlowsGraphControl**. Core responsibilities such as graph modeling, axis semantics, field definitions, formatting behavior, and rendering assumptions were implicitly encoded through OpenFlows-specific abstractions, inference, and factory patterns.

This coupling made it difficult to:

- Reuse graphing functionality outside of OpenFlows
- Reason about graph behavior without deep product context
- Test and validate rendering independently
- Introduce new consumers such as reporting, tooling, or test harnesses

As the graphing code evolved, it became clear that a clean separation was required between:

- **Graphing infrastructure** (models, snapshots, rendering primitives)
- **Product-specific controls** (OpenFlowsChartControl)

To address this, the graphing model and control architecture were re-evaluated with the goal of producing a reusable, explicit, and extensible graphing control.

---

## Decision

We introduce **EngineeringGraphControl** as a first-class, standalone WinForms control built on top of the **Graphing.Controls** library.

Key aspects of this decision:

1. **Explicit Graph Model**
   
   Graphing is now based on explicit, first-class model abstractions:
   
   - `IGraphModel`
   - `IGraphSeriesModel`
   - `IAxisModel`
   - `IGraphFieldDefinition`

   Ownership rules are explicit:
   
   - Graph owns axes and series
   - Series own exactly one X field and one Y field
   - Fields no longer declare axis membership

2. **Default Implementations with Clear Extension Points**
   
   Graphing.Controls provides default, policy-free implementations for:
   
   - GraphModel
   - GraphSeriesModel
   - AxisModel

   Data access remains explicit via an abstract base class:
   
   - GraphFieldDefinitionBase, requiring consumers to implement value retrieval

3. **Removal of Implicit Inference and Factories**
   
   The following legacy abstractions were removed:
   
   - AxisType
   - IGraphModelDependencyInfo
   - INumericFormatterFactory

   Numeric formatting is now configured explicitly at the axis boundary via IAxisModel.

4. **Clear Layering**
   
   Responsibilities are cleanly separated:
   
   - Models define semantics
   - Snapshot and presentation layers transform models
   - Rendering owns geometry and drawing primitives

   Rendering primitives such as GeometryPoint3D live under Rendering/Geometry.

5. **Consumer Validation**
   
   OpenFlowsGraphControl has been copied and renamed to **EngineeringGraphControl** and now builds successfully against the updated Graphing.Controls API.

   This validates the new architecture with a real, non-trivial consumer.

6. **Non-Goals**
   
   EngineeringGraphControl explicitly does **not**:
   
   - Emit telemetry or diagnostics
   - Manage logging
   - Perform dependency injection
   - Acquire or compute data values
   - Contain OpenFlows-specific behavior

---

## Consequences

### Positive

- Graphing functionality is reusable across products and tools
- Architecture is significantly easier to reason about and test
- Explicit models remove hidden coupling and inference
- Consumers can opt into defaults or supply custom implementations
- Rendering behavior can be validated independently of OpenFlows

### Trade-offs

- Consumers must now construct graph models explicitly
- Slightly more upfront configuration is required compared to legacy OpenFlowsChartControl behavior

These trade-offs are intentional and favor clarity, correctness, and extensibility over implicit convenience.

---

## Future Work (Out of Scope)

- WinForms TestHarness for EngineeringGraphControl rendering validation
- Migration of OpenFlowsChartControl to consume EngineeringGraphControl
- Interaction, performance, and UX tuning

---

## Notes

This ADR establishes EngineeringGraphControl as the canonical graphing control moving forward and documents the architectural intent behind its introduction.

---

## Addendum A (2026-07-01): WPF Implementation and Project Structure Update

### Status

Accepted

### Context

After the initial WinForms-centric introduction, a WPF implementation was added to support framework-native hosts that require strict WPF patterns, including binding-first integration and minimal code-behind.

During implementation, project structure was expanded to preserve UI boundaries and to support future host reuse.

### Decision

1. **WPF Control Added as First-Class Implementation**

   `Graphing.Controls.WPF` now contains a WPF-native `EngineeringGraphControl` implementation that preserves the same core model/snapshot/presentation semantics as the WinForms control.

2. **Binding-First Host Contract for WPF**

   The WPF control exposes dependency-property-backed host inputs so consuming applications can bind from `DataContext` in strict MVVM workflows:

   - `GraphModel`
   - `GraphPresentationOptions`
   - `ZoomEnabled`
   - `ZoomExtentsRequestVersion` (trigger token for zoom reset requests)

   Method-based APIs remain available for compatibility, but binding-based integration is the preferred host pattern.

3. **Preserve Framework Boundaries**

   `Graphing.Controls.WPF` remains strictly WPF with no WinForms dependencies.

4. **UI-Free ViewModel Layer for WPF Harness**

   A non-UI assembly, `Graphing.TestHarness.WPF.Core` (`net10.0`), was introduced to host view models and command orchestration. This avoids UI leakage into reusable harness logic and preserves clean layering.

5. **Solution Structure Updated**

   The solution now includes:

   - `Graphing.Controls.WPF`
   - `Graphing.TestHarness.WPF`
   - `Graphing.TestHarness.WPF.Core`

   This structure reflects explicit separation between control implementation, UI host, and non-UI host logic.

### Consequences

#### Positive

- EngineeringGraphControl now has both WinForms and WPF implementations under a shared architectural model.
- WPF consumers can integrate via bindings without relying on code-behind method invocations.
- View-model orchestration is testable and reusable in a UI-free project.
- Framework boundaries are explicit and enforceable.

#### Trade-offs

- WPF host integration introduces additional contract surface (dependency properties and trigger-token semantics).
- Solution and documentation footprint increased to capture the expanded implementation.

### Follow-on Work

- Continue parity work for deferred interactions (for example, animation bar parity where required).
- Keep options editor parity deferred until explicitly prioritized.
- Maintain shared API documentation as a living contract as parity evolves.
