# Control Shared API Signatures (WinForms + WPF)

Status: Living contract for cross-framework parity
Date: 2026-07-01

## Purpose

This document defines the shared control-facing API contract exposed by both implementations:

- WinForms: `Graphing.Controls.EngineeringGraphControl` in `Graphing.Controls.WinForms`
- WPF: `Graphing.Controls.EngineeringGraphControl` in `Graphing.Controls.WPF`

The goal is behavioral parity while preserving framework-specific UI boundaries and host patterns.

## Scope

- Public surface that consumers depend on
- Shared interaction semantics (zoom and axis interaction)
- WPF binding contract for strict MVVM hosts
- Non-goals for this phase: options editor parity and framework-specific designer metadata parity

## Framework Boundary Rule

`Graphing.Controls.WPF` must remain strictly WPF and must not depend on WinForms types.

Examples of disallowed WinForms dependencies in WPF project:

- `System.Windows.Forms.Control`
- `System.Windows.Forms.MouseEventArgs`
- `System.Windows.Forms.MouseButtons`
- `System.Windows.Forms.Keys`

WPF implementation should use WPF-native equivalents (`UIElement`, `MouseEventArgs`, `MouseButtonEventArgs`, `ModifierKeys`, etc.).

## Shared Public Members (Contract)

Both WinForms and WPF controls expose equivalent control semantics, with framework-appropriate shapes at the host boundary.

WinForms control contract:

```csharp
public IGraphModel GraphModel { get; }
public IGraphSnapshot ActiveSnapshot { get; }
public GraphPresentationModel ActivePresentation { get; }
public GraphPresentationOptions ActiveOptions { get; }

public bool AnimationBarEnabled { get; set; }
public int AnimationBarXIndex { get; set; }
public Color AnimationBarColor { get; set; }

public bool ZoomEnabled { get; set; }

public void SetGraphSource(IGraphModel graphModel, GraphPresentationOptions options = null);
public void ZoomExtents();
```

WPF control contract (host-facing):

```csharp
public IGraphModel GraphModel { get; set; }
public GraphPresentationOptions GraphPresentationOptions { get; set; }
public IGraphSnapshot ActiveSnapshot { get; }
public GraphPresentationModel ActivePresentation { get; }
public GraphPresentationOptions ActiveOptions { get; }

public bool ZoomEnabled { get; set; }
public int ZoomExtentsRequestVersion { get; set; }

public void SetGraphSource(IGraphModel graphModel, GraphPresentationOptions options = null);
public void ZoomExtents();
```

Notes:

- WPF `GraphModel`, `GraphPresentationOptions`, `ZoomEnabled`, and `ZoomExtentsRequestVersion` are dependency-property backed to support strict binding-first hosts.
- `ZoomExtentsRequestVersion` is an idempotent trigger token for MVVM command flows where the host view model requests a zoom reset without invoking control methods from code-behind.
- Snapshot/presentation lifecycles must remain aligned with ADR-0002.

## Shared Interaction Events (Behavioral Contract)

Both controls should publish equivalent interaction semantics where implemented:

```csharp
public event EventHandler<AxisInteractionMouseEventArgs> AxisMouseDown;
public event EventHandler<AxisInteractionMouseEventArgs> AxisMouseUp;
public event EventHandler<AxisInteractionMouseEventArgs> AxisContextRequested;

public event EventHandler<AnimationBarIndexChangedEventArgs> AnimationBarXIndexChanged;
```

Event payloads should preserve these semantics:

- Axis descriptor identity and axis metadata
- Client coordinates in control-local space
- Graph coordinates in normalized abstract geometry space
- User-initiated vs programmatic animation index transitions

## ADR-Backed Behavioral Invariants

Both frameworks must preserve the same control behavior:

- Presentation geometry is renderer-agnostic (ADR-0003).
- Layout ownership and pressure resolution are model-level contracts (ADR-0004, ADR-0005).
- Interaction affordances are non-space-owning (ADR-0008).
- Control overlays may consume renderer-produced ephemeral geometry in a one-way dependency (ADR-0009).
- Zoom gesture semantics are identical across frameworks (ADR-0010).

## Current Baseline (as of 2026-07-01)

- WinForms control is fully implemented and is the behavioral reference implementation.
- WPF control implementation exists in `Graphing.Controls.WPF` with renderer + interaction support.
- WPF control supports strict binding-first integration via dependency-property-backed host contract.
- WPF rendering test harness exists in `Graphing.TestHarness.WPF` and uses a UI-free view-model assembly in `Graphing.TestHarness.WPF.Core`.
- Shared renderer abstraction in `Graphing.Controls` remains the primary extension seam for both UI frameworks.

## Parity Status

- Achieved in this phase:
	- Shared graph source lifecycle semantics
	- Shared zoom reset semantics
	- Shared axis interaction orchestration path
	- WPF binding-first host contract
- Deferred:
	- Options editor parity
	- Animation bar parity in WPF host

## Implementation Direction

Continue to preserve behavioral parity by extracting framework-agnostic interaction and control logic into `Graphing.Controls`, with both WinForms and WPF hosts consuming that shared logic.

Priority follow-ons:

- Complete remaining interaction parity surfaces (including animation-bar interactions where required)
- Keep WPF host APIs strictly binding-first and framework-native
- Preserve behavior in WinForms reference implementation as changes are extracted

All parity extractions must remain behavior-preserving for the WinForms reference while keeping WPF free of WinForms dependencies.
