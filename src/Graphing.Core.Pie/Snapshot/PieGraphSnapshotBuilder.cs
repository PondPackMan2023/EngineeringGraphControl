using System;
using System.Collections.Generic;
using Graphing.Core.Pie.Models;

namespace Graphing.Core.Pie.Snapshot
{
    public sealed class PieGraphSnapshotBuilder
    {
        public PieGraphSnapshot Build(IPieGraphModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var slices = model.Slices;
            var totalValue = 0d;

            if (slices != null)
            {
                for (var sliceIndex = 0; sliceIndex < slices.Count; sliceIndex++)
                {
                    var slice = slices[sliceIndex];
                    if (slice == null)
                    {
                        continue;
                    }

                    ValidateValue(slice.Value, slice.Label, sliceIndex);
                    totalValue += slice.Value;
                }
            }

            var snapshotSlices = new List<PieSliceSnapshot>();
            var runningAngle = 0d;

            if (slices != null)
            {
                for (var sliceIndex = 0; sliceIndex < slices.Count; sliceIndex++)
                {
                    var slice = slices[sliceIndex];
                    if (slice == null)
                    {
                        continue;
                    }

                    var percentage = totalValue > 0d ? slice.Value / totalValue : 0d;
                    var sweepAngle = totalValue > 0d ? percentage * 360d : 0d;
                    var startAngle = runningAngle;
                    var formattedValue = model.Formatter != null
                        ? model.Formatter.Format(slice.Value, null)
                        : string.Empty;

                    snapshotSlices.Add(
                        new PieSliceSnapshot(
                            slice.Id,
                            slice.Label,
                            slice.Value,
                            formattedValue,
                            percentage,
                            startAngle,
                            sweepAngle));

                    runningAngle += sweepAngle;
                }
            }

            return new PieGraphSnapshot(
                model.Title,
                model.Unit,
                model.Formatter,
                totalValue,
                snapshotSlices);
        }

        private static void ValidateValue(double value, string label, int sliceIndex)
        {
            if (double.IsNaN(value))
            {
                throw new InvalidOperationException($"Slice at index {sliceIndex} ('{label}') has NaN value.");
            }

            if (double.IsInfinity(value))
            {
                throw new InvalidOperationException($"Slice at index {sliceIndex} ('{label}') has infinite value.");
            }

            if (value < 0d)
            {
                throw new InvalidOperationException($"Slice at index {sliceIndex} ('{label}') has negative value '{value}'.");
            }
        }
    }
}
