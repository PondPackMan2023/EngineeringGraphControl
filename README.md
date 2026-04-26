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
# Clone including submodules
git clone --recurse-submodules https://github.com/PondPackMan2023/EngineeringGraphControl.git

# Restore & build
dotnet restore
dotnet build EngineeringGraphControl.slnx

# Run tests
dotnet test EngineeringGraphControl.slnx
```

## Repository layout

```
EngineeringGraphControl.slnx
Directory.Build.props          # Redirects all build output to /out
Directory.Packages.props       # Central NuGet version management
src/
  Graphing.Controls/           # Class library (net48 + net6.0-windows)
  Graphing.Tests/              # NUnit test project (net10.0)
submodules/
  UnitRegistry/                # Git submodule
.github/workflows/
  ci.yml                       # PR / push CI workflow
```
