# EngineeringGraphControl

A .NET class library providing engineering graph control components for Windows
applications (WinForms / WPF), targeting both .NET Framework 4.8 and .NET 6+.

## Relationship to UnitRegistry

This repository uses
[UnitRegistry](https://github.com/PondPackMan2023/UnitRegistry)
(included as a Git submodule under `submodules/UnitRegistry`) to provide
unit-of-measure support for axis labels, data series, and other graph elements.

## Relationship to OpenFlowsGraphControl

**EngineeringGraphControl** serves as the upstream, general-purpose graphing
library. Project-specific customisation — such as pipe-flow diagrams, head-loss
curves, and pump operating-point overlays — lives in **OpenFlowsGraphControl**,
which takes a project reference to this library.

## Getting Started

```bash
# Clone UnitRegistry
git clone https://github.com/PondPackMan2023/UnitRegistry.git

# Clone EngineeringGraphControl
git clone https://github.com/PondPackMan2023/EngineeringGraphControl.git

# Restore & build
dotnet restore
dotnet build EngineeringGraphControl.slnx

# Run tests
dotnet test EngineeringGraphControl.slnx
```

## Repository layout

```plaintext
EngineeringGraphControl.slnx
Directory.Build.props          # Redirects all build output to /out
Directory.Packages.props       # Central NuGet version management
docs/                          # ADRs, API notes, and design docs
src/
  Graphing.Core/               # Core contracts and graphing model primitives
  Graphing.Controls/           # Shared control contracts/abstractions
  Graphing.Controls.WinForms/  # WinForms EngineeringGraphControl implementation
  Graphing.Controls.WPF/       # WPF EngineeringGraphControl implementation
  Graphing.Editors.WinForms/   # WinForms options/editor UI
  Graphing.TestScenarios/      # Test data/scenario generation utilities
  Graphing.Core.Tests/         # NUnit tests for core model/semantics
  Graphing.Tests/              # NUnit tests for control/presentation behavior
  Graphing.TestHarness/        # WinForms interactive harness
  Graphing.TestHarness.WPF/    # WPF interactive harness
  Graphing.TestHarness.WPF.Core/ # Shared non-UI WPF harness viewmodel layer
.github/workflows/
  ci.yml                       # PR / push CI workflow
```
