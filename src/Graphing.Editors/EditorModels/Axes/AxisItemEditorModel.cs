using System;
using Graphing.Controls.Models;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Editors.EditorModels
{
    public class AxisItemEditorModel
    {
        public AxisId AxisId { get; }
        public AxisSide Side { get; }

        public bool IsVisible { get; set; }

        public bool HasTitleOverride { get; set; }
        public string Title { get; set; }

        public bool HasFixedRange { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }

        public bool HasFixedIncrement { get; set; }
        public double Increment { get; set; }

        public Unit DisplayUnit { get; set; }

        public NumericFormatter NumericFormatter { get; set; }

        public AxisItemEditorModel(AxisId axisId, AxisSide side)
        {
            AxisId = axisId ?? throw new ArgumentNullException(nameof(axisId));
            Side = side;
        }
    }
}
