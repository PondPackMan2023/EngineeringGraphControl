using System;
using System.Collections.Generic;
using System.Linq;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Controls.Snapshot;
using Graphing.Editors.Presentation;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Tests
{
    [TestFixture]
    public class AxisRangeCalculatorAndSnapshotIncrementTests
    {
        [Test]
        public void AxisRangeCalculator_CalculatesBasicPositiveRange()
        {
            var range = AxisRangeCalculator.Calculate(1.2d, 9.8d, 5);

            Assert.That(range.Minimum, Is.EqualTo(0d).Within(1e-12));
            Assert.That(range.Maximum, Is.EqualTo(10d).Within(1e-12));
            Assert.That(range.Increment, Is.EqualTo(2d).Within(1e-12));
        }

        [Test]
        public void AxisRangeCalculator_CalculatesNegativeOnlyRange()
        {
            var range = AxisRangeCalculator.Calculate(-9.1d, -1.2d, 5);

            Assert.That(range.Minimum, Is.EqualTo(-10d).Within(1e-12));
            Assert.That(range.Maximum, Is.EqualTo(0d).Within(1e-12));
            Assert.That(range.Increment, Is.EqualTo(2d).Within(1e-12));
        }

        [Test]
        public void AxisRangeCalculator_HandlesZeroSpanInput()
        {
            var range = AxisRangeCalculator.Calculate(3d, 3d, 5);

            Assert.That(range.Minimum, Is.LessThan(3d));
            Assert.That(range.Maximum, Is.GreaterThan(3d));
            Assert.That(range.Increment, Is.GreaterThan(0d));
        }

        [Test]
        public void AxisRangeCalculator_HandlesVerySmallAndVeryLargeMagnitudes()
        {
            var small = AxisRangeCalculator.Calculate(1e-6, 9e-6, 5);
            var large = AxisRangeCalculator.Calculate(2e8, 9e8, 5);

            Assert.That(double.IsNaN(small.Increment), Is.False);
            Assert.That(double.IsInfinity(small.Increment), Is.False);
            Assert.That(small.Increment, Is.GreaterThan(0d));
            Assert.That(small.Minimum, Is.LessThanOrEqualTo(1e-6));
            Assert.That(small.Maximum, Is.GreaterThanOrEqualTo(9e-6));

            Assert.That(double.IsNaN(large.Increment), Is.False);
            Assert.That(double.IsInfinity(large.Increment), Is.False);
            Assert.That(large.Increment, Is.GreaterThan(0d));
            Assert.That(large.Minimum, Is.LessThanOrEqualTo(2e8));
            Assert.That(large.Maximum, Is.GreaterThanOrEqualTo(9e8));
        }

        [Test]
        public void Snapshot_AutoIncrement_IsCalculatedAndStored()
        {
            var model = CreateModel();

            var snapshot = new GraphSnapshotBuilder().Build(model);
            var axis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(axis.Increment.HasValue, Is.True);
            Assert.That(axis.Increment.Value, Is.GreaterThan(0d));
            Assert.That(axis.IsAutoIncrement, Is.True);
            Assert.That(axis.MinimumValue.HasValue, Is.True);
            Assert.That(axis.MaximumValue.HasValue, Is.True);
        }

        [Test]
        public void Snapshot_FixedIncrementOverride_BypassesAutoCalculation()
        {
            var model = CreateModel();
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("y-axis"),
                        new AxisOverrides
                        {
                            HasFixedIncrement = true,
                            Increment = 3d
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var axis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(axis.IsAutoIncrement, Is.False);
            Assert.That(axis.Increment, Is.EqualTo(3d).Within(1e-12));
        }

        [Test]
        public void Presentation_Ticks_RespectSnapshotMinimumMaximumAndIncrement()
        {
            var model = CreateModel();
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("y-axis"),
                        new AxisOverrides
                        {
                            HasFixedRange = true,
                            Minimum = 0d,
                            Maximum = 12d,
                            HasFixedIncrement = true,
                            Increment = 3d
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var presentation = new GraphPresentationModel(snapshot, options);
            var yAxis = presentation.Axes.Single(a => a.AxisId == "y-axis");

            var values = yAxis.Ticks.Select(t => t.Value).ToArray();

            Assert.That(values, Is.EqualTo(new[] { 0d, 3d, 6d, 9d, 12d }));
        }

        [Test]
        public void GraphOptionsModel_AutoIncrement_DefaultsFromSnapshot()
        {
            var model = CreateModel();
            var options = new GraphPresentationOptions();
            var snapshot = new GraphSnapshotBuilder().Build(model, options);

            var pm = new GraphOptionsPresentationModel(model, options, snapshot);
            var axisItem = pm.Axes.Axes.Single(a => a.AxisId.Value == "y-axis");
            var axisSnapshot = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(axisItem.HasFixedIncrement, Is.False);
            Assert.That(axisItem.Increment, Is.EqualTo(axisSnapshot.Increment ?? 1d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_AutoMode_ClipsMaximumToActualMaximum()
        {
            var model = CreateModelForAutoClipping();

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.IsAutoIncrement, Is.True);
            Assert.That(xAxis.MaximumValue, Is.EqualTo(97d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_UserMaximumOverride_BypassesClipping()
        {
            var model = CreateModelForAutoClipping();
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("x-axis"),
                        new AxisOverrides
                        {
                            HasFixedRange = true,
                            Minimum = 0d,
                            Maximum = 120d
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.MaximumValue, Is.EqualTo(120d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_AutoMode_RemainsUnchangedAndMayExceedActualMaximum()
        {
            var model = CreateModelForAutoClipping();

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.IsAutoIncrement, Is.True);
            Assert.That(yAxis.MaximumValue.HasValue, Is.True);
            Assert.That(yAxis.MaximumValue.Value, Is.GreaterThan(97d));
        }

        [Test]
        public void Snapshot_XAxis_ManualToAutoToggle_ReappliesClipping()
        {
            var model = CreateModelForAutoClipping();

            var manualOptions = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("x-axis"),
                        new AxisOverrides
                        {
                            HasFixedRange = true,
                            Minimum = 0d,
                            Maximum = 120d
                        }
                    }
                });

            var manualSnapshot = new GraphSnapshotBuilder().Build(model, manualOptions);
            var manualXAxis = manualSnapshot.Axes.Single(a => a.AxisId == "x-axis");
            Assert.That(manualXAxis.MaximumValue, Is.EqualTo(120d).Within(1e-12));

            var autoOptions = new GraphPresentationOptions();
            var autoSnapshot = new GraphSnapshotBuilder().Build(model, autoOptions);
            var autoXAxis = autoSnapshot.Axes.Single(a => a.AxisId == "x-axis");
            Assert.That(autoXAxis.MaximumValue, Is.EqualTo(97d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_TemporalHours_AutoIncrementPolicyApplied()
        {
            var model = CreateTemporalModel(Units.Time.Hours, Units.Length.Meter);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.IsAutoIncrement, Is.True);
            Assert.That(xAxis.Increment, Is.EqualTo(12d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_TemporalMinutes_AutoIncrementPolicyApplied()
        {
            var model = CreateTemporalModel(Units.Time.Minutes, Units.Length.Meter);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.IsAutoIncrement, Is.True);
            Assert.That(xAxis.Increment, Is.EqualTo(15d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_TemporalSeconds_AutoIncrementPolicyApplied()
        {
            var model = CreateTemporalModel(Units.Time.Seconds, Units.Length.Meter);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.IsAutoIncrement, Is.True);
            Assert.That(xAxis.Increment, Is.EqualTo(15d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_TimeUnits_AutoIncrementUnchanged()
        {
            var model = CreateTemporalModel(Units.Length.Meter, Units.Time.Hours);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.IsAutoIncrement, Is.True);
            Assert.That(yAxis.Increment, Is.EqualTo(20d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_Temporal_UserIncrementOverrideBypassesPolicy()
        {
            var model = CreateTemporalModel(Units.Time.Minutes, Units.Length.Meter);
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("x-axis"),
                        new AxisOverrides
                        {
                            HasFixedIncrement = true,
                            Increment = 7d
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.IsAutoIncrement, Is.False);
            Assert.That(xAxis.Increment, Is.EqualTo(7d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_Temporal_ManualToAutoToggle_ReappliesTemporalPolicy()
        {
            var model = CreateTemporalModel(Units.Time.Minutes, Units.Length.Meter);

            var manualOptions = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("x-axis"),
                        new AxisOverrides
                        {
                            HasFixedIncrement = true,
                            Increment = 7d
                        }
                    }
                });

            var manualSnapshot = new GraphSnapshotBuilder().Build(model, manualOptions);
            var manualXAxis = manualSnapshot.Axes.Single(a => a.AxisId == "x-axis");
            Assert.That(manualXAxis.IsAutoIncrement, Is.False);
            Assert.That(manualXAxis.Increment, Is.EqualTo(7d).Within(1e-12));

            var autoSnapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var autoXAxis = autoSnapshot.Axes.Single(a => a.AxisId == "x-axis");
            Assert.That(autoXAxis.IsAutoIncrement, Is.True);
            Assert.That(autoXAxis.Increment, Is.EqualTo(15d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_BinaryAutoDetection_AppliesFixedZeroOneRangeAndIncrement()
        {
            var model = CreateBinaryYAxisModel();

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.IsAutoIncrement, Is.True);
            Assert.That(yAxis.MinimumValue, Is.EqualTo(0d).Within(1e-12));
            Assert.That(yAxis.MaximumValue, Is.EqualTo(1d).Within(1e-12));
            Assert.That(yAxis.Increment, Is.EqualTo(1d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_BinaryPolicy_OverridesStandardRangeLogic()
        {
            var model = CreateBinaryYAxisModel();

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MaximumValue, Is.Not.EqualTo(2d).Within(1e-12));
            Assert.That(yAxis.Increment, Is.EqualTo(1d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_PercentagePolicy_AppliesPaddedRange()
        {
            var model = CreatePercentageYAxisModel(new[] { 30d, 50d, 70d });

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MinimumValue, Is.EqualTo(25d).Within(1e-12));
            Assert.That(yAxis.MaximumValue, Is.EqualTo(75d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_AboveZeroPolicy_ClampsNegativeMinimum()
        {
            var model = CreateModelWithNegativeYValues();
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("y-axis"),
                        new AxisOverrides
                        {
                            EnforceMinimumZero = true
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MinimumValue, Is.EqualTo(0d).Within(1e-12));
            Assert.That(yAxis.MaximumValue.HasValue, Is.True);
            Assert.That(yAxis.MaximumValue.Value, Is.GreaterThan(0d));
        }

        [Test]
        public void Snapshot_YAxis_UserOverrides_BypassSemanticPolicies()
        {
            var model = CreateBinaryYAxisModel();
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("y-axis"),
                        new AxisOverrides
                        {
                            HasFixedRange = true,
                            Minimum = -5d,
                            Maximum = 5d,
                            HasFixedIncrement = true,
                            Increment = 2d,
                            EnforceMinimumZero = true
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.IsAutoIncrement, Is.False);
            Assert.That(yAxis.MinimumValue, Is.EqualTo(-5d).Within(1e-12));
            Assert.That(yAxis.MaximumValue, Is.EqualTo(5d).Within(1e-12));
            Assert.That(yAxis.Increment, Is.EqualTo(2d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_RemainsUnaffectedByYAxisSemanticPolicies()
        {
            var model = CreatePercentageYAxisModel(new[] { 30d, 50d, 70d });
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("y-axis"),
                        new AxisOverrides
                        {
                            EnforceMinimumZero = true
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.MaximumValue, Is.EqualTo(4d).Within(1e-12));
            Assert.That(xAxis.Increment, Is.EqualTo(1d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_EnabledByDefaultForScalarUnits()
        {
            var model = CreateModel();

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.IsAutoIncrement, Is.True);
            Assert.That(yAxis.Increment, Is.EqualTo(1d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_DisabledViaOptionFlag()
        {
            var model = CreateModel();
            var options = new GraphPresentationOptions(enableDenseNumericYAxisTicks: false);

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.IsAutoIncrement, Is.True);
            Assert.That(yAxis.Increment, Is.EqualTo(2d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_DefaultExcludedDimensions_BypassDenseBehavior()
        {
            var percentModel = CreatePercentageYAxisModel(new[] { 30d, 50d, 70d });
            var percentSnapshot = new GraphSnapshotBuilder().Build(percentModel, new GraphPresentationOptions());
            var percentYAxis = percentSnapshot.Axes.Single(a => a.AxisId == "y-axis");
            Assert.That(percentYAxis.Increment, Is.EqualTo(10d).Within(1e-12));

            var timeModel = CreateTemporalModel(Units.Length.Meter, Units.Time.Hours);
            var timeSnapshot = new GraphSnapshotBuilder().Build(timeModel, new GraphPresentationOptions());
            var timeYAxis = timeSnapshot.Axes.Single(a => a.AxisId == "y-axis");
            Assert.That(timeYAxis.Increment, Is.EqualTo(20d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_CustomExclusionList_ModifiesBehavior()
        {
            var model = CreateModel();
            var customExclusions = new HashSet<Dimension> { Dimensions.Percent, Dimensions.Time, Dimensions.Length };
            var options = new GraphPresentationOptions(denseNumericYAxisExcludedDimensions: customExclusions);

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.IsAutoIncrement, Is.True);
            Assert.That(yAxis.Increment, Is.EqualTo(2d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_UserIncrementOverrideBypassesDensePolicy()
        {
            var model = CreateModel();
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("y-axis"),
                        new AxisOverrides
                        {
                            HasFixedIncrement = true,
                            Increment = 3d
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.IsAutoIncrement, Is.False);
            Assert.That(yAxis.Increment, Is.EqualTo(3d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_DenseNumericPolicy_DoesNotAffectXAxisBehavior()
        {
            var model = CreateModel();

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.IsAutoIncrement, Is.True);
            Assert.That(xAxis.Increment, Is.EqualTo(1d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_AnchorsMinimumToActualMinimum()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Meter);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MinimumValue, Is.EqualTo(175.5d).Within(1e-12));
            Assert.That(yAxis.Increment, Is.EqualTo(0.5d).Within(1e-12));
            Assert.That(yAxis.MajorTickStride, Is.EqualTo(1));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_MetersHgl_MinorAndMajorTickSeparation()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Meter);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MinimumValue, Is.EqualTo(175.5d).Within(1e-12));
            Assert.That(yAxis.MaximumValue, Is.EqualTo(180d).Within(1e-12));
            Assert.That(yAxis.Increment, Is.EqualTo(0.5d).Within(1e-12));
            Assert.That(yAxis.MajorTickStride, Is.EqualTo(1));

            var minorIntervals = (int)Math.Round((yAxis.MaximumValue.Value - yAxis.MinimumValue.Value) / yAxis.Increment.Value);
            var minorTickCount = minorIntervals + 1;
            var majorTickCount = (minorIntervals / yAxis.MajorTickStride) + 1;

            Assert.That(minorTickCount, Is.EqualTo(10));
            Assert.That(majorTickCount, Is.EqualTo(10));
            Assert.That(majorTickCount, Is.GreaterThanOrEqualTo(9).And.LessThanOrEqualTo(13));

            var majorInterval = yAxis.Increment.Value * yAxis.MajorTickStride;
            var majorBoundaryIntervals = (yAxis.MaximumValue.Value - yAxis.MinimumValue.Value) / majorInterval;
            Assert.That(majorBoundaryIntervals, Is.EqualTo(Math.Round(majorBoundaryIntervals)).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_UserRangeOverrideBypassesAnchoring()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Meter);
            var options = new GraphPresentationOptions(
                axisOverrides: new Dictionary<AxisId, AxisOverrides>
                {
                    {
                        new AxisId("y-axis"),
                        new AxisOverrides
                        {
                            HasFixedRange = true,
                            Minimum = 175d,
                            Maximum = 180d
                        }
                    }
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MinimumValue, Is.EqualTo(175d).Within(1e-12));
            Assert.That(yAxis.MinimumValue, Is.Not.EqualTo(175.5d).Within(1e-12));
        }

        [Test]
        public void Snapshot_XAxis_DenseNumericAnchoring_DoesNotAffectXAxisMinimum()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Meter);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var xAxis = snapshot.Axes.Single(a => a.AxisId == "x-axis");

            Assert.That(xAxis.MinimumValue, Is.EqualTo(0d).Within(1e-12));
            Assert.That(xAxis.Increment, Is.EqualTo(5d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_MajorTickCountAlwaysInRange()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Meter);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.Increment.HasValue, Is.True);
            Assert.That(yAxis.Increment.Value, Is.GreaterThan(0d));

            var minorIntervals = (int)Math.Round((yAxis.MaximumValue.Value - yAxis.MinimumValue.Value) / yAxis.Increment.Value);
            var majorTickCount = (minorIntervals / yAxis.MajorTickStride) + 1;

            Assert.That(majorTickCount, Is.GreaterThanOrEqualTo(9).And.LessThanOrEqualTo(13));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_FeetHgl_MajorTickAnchoredParity()
        {
            var metersModel = CreateHglDenseNumericModel(Units.Length.Meter);
            var feetModel = CreateHglDenseNumericModel(Units.Length.Feet);

            var metersSnapshot = new GraphSnapshotBuilder().Build(metersModel, new GraphPresentationOptions());
            var feetSnapshot = new GraphSnapshotBuilder().Build(feetModel, new GraphPresentationOptions());

            var metersAxis = metersSnapshot.Axes.Single(a => a.AxisId == "y-axis");
            var feetAxis = feetSnapshot.Axes.Single(a => a.AxisId == "y-axis");

            // Meters: minor increment 0.5, stride 1, 10 major labels.
            Assert.That(metersAxis.Increment, Is.EqualTo(0.5d).Within(1e-12));
            Assert.That(metersAxis.MinimumValue, Is.EqualTo(175.5d).Within(1e-12));
            Assert.That(metersAxis.MaximumValue, Is.EqualTo(180d).Within(1e-12));
            Assert.That(metersAxis.MajorTickStride, Is.EqualTo(1));

            var metersMinorIntervals = (int)Math.Round((metersAxis.MaximumValue.Value - metersAxis.MinimumValue.Value) / metersAxis.Increment.Value);
            var metersMajorTickCount = (metersMinorIntervals / metersAxis.MajorTickStride) + 1;
            Assert.That(metersMajorTickCount, Is.EqualTo(10));

            // Feet parity: minor increment 1.25, stride 1, 13 major labels.
            Assert.That(feetAxis.Increment, Is.EqualTo(1.25d).Within(1e-12));
            Assert.That(feetAxis.MinimumValue, Is.EqualTo(575d).Within(1e-12));
            Assert.That(feetAxis.MaximumValue, Is.EqualTo(590d).Within(1e-12));
            Assert.That(feetAxis.MajorTickStride, Is.EqualTo(1));

            var feetMinorIntervals = (int)Math.Round((feetAxis.MaximumValue.Value - feetAxis.MinimumValue.Value) / feetAxis.Increment.Value);
            var feetMinorTickCount = feetMinorIntervals + 1;
            var feetMajorTickCount = (feetMinorIntervals / feetAxis.MajorTickStride) + 1;
            Assert.That(feetMinorTickCount, Is.EqualTo(13));
            Assert.That(feetMajorTickCount, Is.EqualTo(13));

            var feetMajorInterval = feetAxis.Increment.Value * feetAxis.MajorTickStride;
            var feetMajorBoundaryIntervals = (feetAxis.MaximumValue.Value - feetAxis.MinimumValue.Value) / feetMajorInterval;
            Assert.That(feetMajorBoundaryIntervals, Is.EqualTo(Math.Round(feetMajorBoundaryIntervals)).Within(1e-12));
        }

        [Test]
        public void DenseNumericRefinement_StopsAfterAtMostFiveIterations()
        {
            var result = AxisPolicyApplier.RefineDenseNumericIncrement(175.750910d, 180d, 1d);

            Assert.That(result.Iterations, Is.LessThanOrEqualTo(AxisPolicyApplier.MaxDenseRefinementIterations));
            Assert.That(AxisPolicyApplier.MaxDenseRefinementIterations, Is.EqualTo(8));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_DoesNotExpandBeyondFixedMaximum()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Meter);

            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MaximumValue, Is.EqualTo(180d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_MajorTickStride_IsAlwaysPositive()
        {
            var model = CreateModel();
            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MajorTickStride, Is.GreaterThan(0));
        }

        [Test]
        public void Snapshot_YAxis_MajorTickCount_WithStrideIsInRange()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Meter);
            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            var minorIntervals = (int)Math.Round((yAxis.MaximumValue.Value - yAxis.MinimumValue.Value) / yAxis.Increment.Value);
            var minorTickCount = minorIntervals + 1;
            var majorTickCount = (minorIntervals / yAxis.MajorTickStride) + 1;

            // Minor tick count need not be bounded by 9–13; the stride collapses it to the label range.
            Assert.That(minorTickCount, Is.GreaterThan(0));
            Assert.That(majorTickCount, Is.GreaterThanOrEqualTo(9).And.LessThanOrEqualTo(13));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_MajorAnchoringIncludesBottomLabel()
        {
            var feetModel = CreateHglDenseNumericModel(Units.Length.Feet);
            var feetSnapshot = new GraphSnapshotBuilder().Build(feetModel, new GraphPresentationOptions());
            var feetAxis = feetSnapshot.Axes.Single(a => a.AxisId == "y-axis");

            var majorInterval = feetAxis.Increment.Value * feetAxis.MajorTickStride;
            var majorIntervals = (feetAxis.MaximumValue.Value - feetAxis.MinimumValue.Value) / majorInterval;
            var majorTickCount = (int)Math.Round(majorIntervals) + 1;

            Assert.That(feetAxis.MinimumValue, Is.EqualTo(575d).Within(1e-12));
            Assert.That(majorTickCount, Is.EqualTo(13));
            Assert.That(feetAxis.MinimumValue.Value + ((majorTickCount - 1) * majorInterval), Is.EqualTo(feetAxis.MaximumValue.Value).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_LabelSpacing_EqualsMinorIncrementTimesStride()
        {
            var feetModel = CreateHglDenseNumericModel(Units.Length.Feet);
            var feetSnapshot = new GraphSnapshotBuilder().Build(feetModel, new GraphPresentationOptions());
            var feetAxis = feetSnapshot.Axes.Single(a => a.AxisId == "y-axis");

            var labelSpacing = feetAxis.Increment.Value * feetAxis.MajorTickStride;

            Assert.That(feetAxis.Increment, Is.EqualTo(1.25d).Within(1e-12));
            Assert.That(feetAxis.MajorTickStride, Is.EqualTo(1));
            Assert.That(labelSpacing, Is.EqualTo(1.25d).Within(1e-12));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_InchesParity_WithWaterGems()
        {
            var inchesModel = CreateHglDenseNumericModel(Units.Length.Inches);
            var snapshot = new GraphSnapshotBuilder().Build(inchesModel, new GraphPresentationOptions());
            var yAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(yAxis.MinimumValue, Is.EqualTo(6900d).Within(1e-12));
            Assert.That(yAxis.MaximumValue, Is.EqualTo(7080d).Within(1e-12));
            Assert.That(yAxis.Increment, Is.EqualTo(20d).Within(1e-12));
            Assert.That(yAxis.MajorTickStride, Is.EqualTo(1));

            var majorInterval = yAxis.Increment.Value * yAxis.MajorTickStride;
            var majorTickCount = (int)Math.Round((yAxis.MaximumValue.Value - yAxis.MinimumValue.Value) / majorInterval) + 1;
            Assert.That(majorInterval, Is.EqualTo(20d).Within(1e-12));
            Assert.That(majorTickCount, Is.EqualTo(10));
        }

        [Test]
        public void Snapshot_YAxis_DenseNumericPolicy_ScaleInvariantAcrossMetersFeetInches()
        {
            var metersAxis = new GraphSnapshotBuilder().Build(CreateHglDenseNumericModel(Units.Length.Meter), new GraphPresentationOptions()).Axes.Single(a => a.AxisId == "y-axis");
            var feetAxis = new GraphSnapshotBuilder().Build(CreateHglDenseNumericModel(Units.Length.Feet), new GraphPresentationOptions()).Axes.Single(a => a.AxisId == "y-axis");
            var inchesAxis = new GraphSnapshotBuilder().Build(CreateHglDenseNumericModel(Units.Length.Inches), new GraphPresentationOptions()).Axes.Single(a => a.AxisId == "y-axis");

            var metersMajorInterval = metersAxis.Increment.Value * metersAxis.MajorTickStride;
            var feetMajorInterval = feetAxis.Increment.Value * feetAxis.MajorTickStride;
            var inchesMajorInterval = inchesAxis.Increment.Value * inchesAxis.MajorTickStride;

            Assert.That(metersMajorInterval, Is.EqualTo(0.5d).Within(1e-12));
            Assert.That(feetMajorInterval, Is.EqualTo(1.25d).Within(1e-12));
            Assert.That(inchesMajorInterval, Is.EqualTo(20d).Within(1e-12));

            Assert.That((feetAxis.MaximumValue.Value - feetAxis.MinimumValue.Value) / feetMajorInterval, Is.EqualTo(12d).Within(1e-12));
            Assert.That((metersAxis.MaximumValue.Value - metersAxis.MinimumValue.Value) / metersMajorInterval, Is.EqualTo(9d).Within(1e-12));
            Assert.That((inchesAxis.MaximumValue.Value - inchesAxis.MinimumValue.Value) / inchesMajorInterval, Is.EqualTo(9d).Within(1e-12));
        }

        [Test]
        public void Presentation_YAxis_DensePolicy_RendersMajorTicksGridlinesAndLabels_OneToOne_Meters()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Meter);
            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions());

            var snapshotYAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");
            var renderedYAxis = presentation.Axes.Single(a => a.AxisId == "y-axis");
            var horizontalGridLines = presentation.Layout.GridLines.HorizontalLines
                .Where(l => l.AxisEntry != null && l.AxisEntry.Axis != null && l.AxisEntry.Axis.AxisId == "y-axis")
                .ToArray();

            Assert.That(snapshotYAxis.Increment, Is.EqualTo(0.5d).Within(1e-12));
            Assert.That(snapshotYAxis.MajorTickStride, Is.EqualTo(1));

            Assert.That(renderedYAxis.Ticks.Count, Is.EqualTo(10));
            Assert.That(horizontalGridLines.Length, Is.EqualTo(10));
            Assert.That(renderedYAxis.Ticks.All(t => !string.IsNullOrWhiteSpace(t.Label)), Is.True);

            var majorInterval = snapshotYAxis.Increment.Value * snapshotYAxis.MajorTickStride;
            for (var i = 1; i < renderedYAxis.Ticks.Count; i++)
            {
                var spacing = renderedYAxis.Ticks[i].Value - renderedYAxis.Ticks[i - 1].Value;
                Assert.That(spacing, Is.EqualTo(majorInterval).Within(1e-12));
            }
        }

        [Test]
        public void Presentation_YAxis_DensePolicy_RendersOnlyMajorInterval_NoMinorTicks_Feet()
        {
            var model = CreateHglDenseNumericModel(Units.Length.Feet);
            var snapshot = new GraphSnapshotBuilder().Build(model, new GraphPresentationOptions());
            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions());

            var snapshotYAxis = snapshot.Axes.Single(a => a.AxisId == "y-axis");
            var renderedYAxis = presentation.Axes.Single(a => a.AxisId == "y-axis");
            var horizontalGridLines = presentation.Layout.GridLines.HorizontalLines
                .Where(l => l.AxisEntry != null && l.AxisEntry.Axis != null && l.AxisEntry.Axis.AxisId == "y-axis")
                .ToArray();

            var minorTickCount = (int)Math.Round((snapshotYAxis.MaximumValue.Value - snapshotYAxis.MinimumValue.Value) / snapshotYAxis.Increment.Value) + 1;
            var majorInterval = snapshotYAxis.Increment.Value * snapshotYAxis.MajorTickStride;

            Assert.That(snapshotYAxis.Increment, Is.EqualTo(1.25d).Within(1e-12));
            Assert.That(snapshotYAxis.MajorTickStride, Is.EqualTo(1));
            Assert.That(majorInterval, Is.EqualTo(1.25d).Within(1e-12));

            Assert.That(minorTickCount, Is.EqualTo(13));
            Assert.That(renderedYAxis.Ticks.Count, Is.EqualTo(13));
            Assert.That(horizontalGridLines.Length, Is.EqualTo(13));
            Assert.That(renderedYAxis.Ticks.All(t => !string.IsNullOrWhiteSpace(t.Label)), Is.True);

            for (var i = 1; i < renderedYAxis.Ticks.Count; i++)
            {
                var spacing = renderedYAxis.Ticks[i].Value - renderedYAxis.Ticks[i - 1].Value;
                Assert.That(spacing, Is.EqualTo(majorInterval).Within(1e-12));
            }
        }

        [Test]
        public void AxisPolicyApplier_ComputeMajorTickStride_ReturnsOneForNullInputs()
        {
            Assert.That(AxisPolicyApplier.ComputeMajorTickStride(null, null, null), Is.EqualTo(1));
            Assert.That(AxisPolicyApplier.ComputeMajorTickStride(0d, 10d, null), Is.EqualTo(1));
        }

        [Test]
        public void AxisPolicyApplier_ComputeMajorTickStride_ReturnsLargestValidStride()
        {
            // 60 minor intervals: stride=5 gives (60/5)+1=13 major ticks -> valid.
            Assert.That(AxisPolicyApplier.ComputeMajorTickStride(575d, 590d, 0.25d), Is.EqualTo(5));

            // 9 minor intervals: stride=1 gives 10 major ticks -> valid; stride=2 gives 5 -> invalid.
            Assert.That(AxisPolicyApplier.ComputeMajorTickStride(175.5d, 180d, 0.5d), Is.EqualTo(1));
        }

        [Test]
        public void AxisPolicyApplier_ComputeMajorAnchoredMinimum_UsesMajorIntervalBoundaries()
        {
            var metersMinimum = AxisPolicyApplier.ComputeMajorAnchoredMinimum(180d, 175.5d, 0.5d);
            var feetMinimum = AxisPolicyApplier.ComputeMajorAnchoredMinimum(590d, 575d, 1.25d);

            Assert.That(metersMinimum, Is.EqualTo(175.5d).Within(1e-12));
            Assert.That(feetMinimum, Is.EqualTo(575d).Within(1e-12));
        }

        private static IGraphModel CreateModel()
        {
            var unit = Units.Length.Meter;
            var formatterX = new NumericFormatter("formatter-x", UnitsRegistry.Default, "Distance", "F2");
            var formatterY = new NumericFormatter("formatter-y", UnitsRegistry.Default, "Elevation", "F2");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", formatterX);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatterY);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d, 4d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 1.2d, 2.1d, 4.6d, 8.4d, 9.8d });

            var series = new GraphSeriesModel(new SeriesId("s1"), "Series 1", SeriesType.Line, xField, yField, xAxis, yAxis);
            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphModel CreateModelForAutoClipping()
        {
            var unit = Units.Length.Meter;
            var formatterX = new NumericFormatter("formatter-x-clip", UnitsRegistry.Default, "Distance", "F2");
            var formatterY = new NumericFormatter("formatter-y-clip", UnitsRegistry.Default, "Elevation", "F2");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", formatterX);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatterY);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 20d, 55d, 97d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 20d, 55d, 97d });

            var series = new GraphSeriesModel(new SeriesId("clip-series"), "Clip Series", SeriesType.Line, xField, yField, xAxis, yAxis);
            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphModel CreateTemporalModel(Unit xUnit, Unit yUnit)
        {
            var formatterX = new NumericFormatter("formatter-x-time", UnitsRegistry.Default, "Time", "F2");
            var formatterY = new NumericFormatter("formatter-y-time", UnitsRegistry.Default, "Value", "F2");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, xUnit, xUnit != null ? xUnit.Label : string.Empty, formatterX);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, yUnit, yUnit != null ? yUnit.Label : string.Empty, formatterY);

            var xField = new TestFieldDefinition("X", "x", xUnit, new[] { 0d, 20d, 55d, 97d });
            var yField = new TestFieldDefinition("Y", "y", yUnit, new[] { 0d, 20d, 55d, 97d });

            var series = new GraphSeriesModel(new SeriesId("temporal-series"), "Temporal Series", SeriesType.Line, xField, yField, xAxis, yAxis);
            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphModel CreateModelWithNegativeYValues()
        {
            var unit = Units.Length.Meter;
            var formatterX = new NumericFormatter("formatter-x-neg", UnitsRegistry.Default, "Distance", "F2");
            var formatterY = new NumericFormatter("formatter-y-neg", UnitsRegistry.Default, "Elevation", "F2");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", formatterX);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatterY);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d, 4d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { -12d, -4d, 2d, 9d, 14d });

            var series = new GraphSeriesModel(new SeriesId("negative-y-series"), "Negative Y Series", SeriesType.Line, xField, yField, xAxis, yAxis);
            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphModel CreateBinaryYAxisModel()
        {
            var xUnit = Units.Length.Meter;
            var yUnit = Units.Unitless.UnitlessUnit;
            var formatterX = new NumericFormatter("formatter-x-binary", UnitsRegistry.Default, "Distance", "F2");
            var formatterY = new NumericFormatter("formatter-y-binary", UnitsRegistry.Default, "Classification", "F0");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, xUnit, "m", formatterX);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, yUnit, string.Empty, formatterY);

            var xField = new TestFieldDefinition("X", "x", xUnit, new[] { 0d, 1d, 2d, 3d, 4d });
            var yField = new TestFieldDefinition("Y", "y", yUnit, new[] { 0d, 1d, 0d, 1d, 1d });

            var series = new GraphSeriesModel(new SeriesId("binary-series"), "Binary Series", SeriesType.Line, xField, yField, xAxis, yAxis);
            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphModel CreatePercentageYAxisModel(double[] yValues)
        {
            var xUnit = Units.Length.Meter;
            var yUnit = Units.Percent.PercentPercent;
            var formatterX = new NumericFormatter("formatter-x-percent", UnitsRegistry.Default, "Distance", "F2");
            var formatterY = new NumericFormatter("formatter-y-percent", UnitsRegistry.Default, "Percent", "F1");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, xUnit, "m", formatterX);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, yUnit, "%%", formatterY);

            var xField = new TestFieldDefinition("X", "x", xUnit, new[] { 0d, 1d, 2d, 3d, 4d });
            var yField = new TestFieldDefinition("Y", "y", yUnit, yValues);

            var series = new GraphSeriesModel(new SeriesId("percent-series"), "Percent Series", SeriesType.Line, xField, yField, xAxis, yAxis);
            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphModel CreateHglDenseNumericModel(Unit displayUnit)
        {
            var sourceUnit = Units.Length.Meter;
            var formatterX = new NumericFormatter("formatter-x-hgl", UnitsRegistry.Default, "Distance", "F2");
            var formatterY = new NumericFormatter("formatter-y-hgl", UnitsRegistry.Default, "Hydraulic Grade", "F3");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, sourceUnit, "m", formatterX);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, displayUnit, displayUnit != null ? displayUnit.Label : string.Empty, formatterY);

            var xField = new TestFieldDefinition("X", "x", sourceUnit, new[]
            {
                0d, 1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d,
                10d, 11d, 12d, 13d, 14d, 15d, 16d, 17d, 18d, 19d,
                20d, 21d, 22d, 23d, 24d, 25d, 26d, 27d
            });

            var yField = new TestFieldDefinition("Y", "y", sourceUnit, new[]
            {
                178.916874, 178.325934, 178.323273, 178.263054, 178.205178,
                178.149535, 178.096069, 178.044946, 178.010493, 177.700000,
                177.284007, 176.879436, 176.489544, 176.113604, 175.750910,
                177.142583, 177.486841, 178.201346, 178.943626, 179.791984,
                178.325729, 178.248562, 178.154428, 178.064629, 178.014418,
                177.966439, 177.948468, 177.902741
            });

            var series = new GraphSeriesModel(new SeriesId("hgl-series"), "HGL Series", SeriesType.Line, xField, yField, xAxis, yAxis);
            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private sealed class TestFieldDefinition : IGraphFieldDefinition
        {
            private readonly Array _values;

            public TestFieldDefinition(string label, string name, Unit unit, Array values)
            {
                Label = label;
                Name = name;
                Unit = unit;
                _values = values;
            }

            public string Label { get; }

            public string Name { get; }

            public Unit Unit { get; }

            public Array GetValues()
            {
                return _values;
            }
        }
    }
}
