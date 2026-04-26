using System;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Immutable snapshot of a single field within a series.
    /// Contains the field identifier, name, and assigned axis type.
    /// </summary>
    internal sealed class FieldSnapshot : IFieldSnapshot
    {
        private readonly string _label;
        private readonly string _name;
        private readonly Array _values;
        private readonly Unit _unit;
        private readonly string _displayUnitLabel;
        private readonly string _formatterName;
        private readonly NumericFormatter _formatter;

        /// <summary>
        /// The human-readable label of the field.
        /// </summary>
        public string Label
        {
            get { return _label; }
        }

        /// <summary>
        /// The display name of the field.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Raw values returned by the graph field definition.
        /// </summary>
        public Array Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Unit used by this numeric field.
        /// </summary>
        public Unit Unit
        {
            get { return _unit; }
        }

        /// <summary>
        /// Optional display unit label for unitized fields.
        /// </summary>
        public string DisplayUnitLabel
        {
            get { return _displayUnitLabel; }
        }

        /// <summary>
        /// Optional formatter identity for unitized fields.
        /// </summary>
        public string FormatterName
        {
            get { return _formatterName; }
        }

        /// <summary>
        /// Optional defensive formatter copy for unitized fields.
        /// </summary>
        public NumericFormatter Formatter
        {
            get { return _formatter; }
        }

        /// <summary>
        /// Creates an immutable snapshot of a field.
        /// </summary>
        /// <param name="label">The human-readable label of the field.</param>
        /// <param name="name">The display name of the field.</param>
        /// <param name="values">Raw values returned by the graph field definition.</param>
        /// <param name="unit">Unit used by this numeric field.</param>
        /// <param name="displayUnitLabel">Optional display unit label for unitized fields.</param>
        /// <param name="formatterName">Optional formatter identity for unitized fields.</param>
        /// <param name="formatter">Optional defensive formatter copy for unitized fields.</param>
        public FieldSnapshot(
            string label,
            string name,
            Array values,
            Unit unit,
            string displayUnitLabel,
            string formatterName,
            NumericFormatter formatter)
        {
            _label = label;
            _name = name;
            _values = values;
            _unit = unit;
            _displayUnitLabel = displayUnitLabel;
            _formatterName = formatterName;
            _formatter = formatter;
        }
    }
}
