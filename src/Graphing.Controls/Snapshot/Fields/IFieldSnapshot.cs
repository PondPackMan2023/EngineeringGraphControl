using System;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Read-only interface for consuming a field snapshot.
    /// </summary>
    public interface IFieldSnapshot
    {
        /// <summary>
        /// The human-readable label of the field.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// The display name of the field.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Raw values returned by the graph field definition.
        /// </summary>
        Array Values { get; }

        /// <summary>
        /// Unit used by this numeric field.
        /// </summary>
        Unit Unit { get; }

        /// <summary>
        /// Optional display unit label for unitized fields.
        /// </summary>
        string DisplayUnitLabel { get; }

        /// <summary>
        /// Optional formatter identity for unitized fields.
        /// </summary>
        string FormatterName { get; }

        /// <summary>
        /// Optional defensive formatter copy for unitized fields.
        /// </summary>
        NumericFormatter Formatter { get; }
    }
}
