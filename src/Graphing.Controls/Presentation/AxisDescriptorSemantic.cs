namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Semantic axis descriptor/caption.
    /// Captures meaning only, without layout or styling.
    /// </summary>
    public sealed class AxisDescriptorSemantic
    {
        public AxisDescriptorSemantic(
            string axisIdentity,
            string axisId,
            string caption,
            string displayUnitLabel,
            string formatterName)
        {
            AxisIdentity = axisIdentity;
            AxisId = axisId;
            Caption = caption;
            DisplayUnitLabel = displayUnitLabel;
            FormatterName = formatterName;
        }

        public string AxisIdentity { get; }
        public string AxisId { get; }
        public string Caption { get; }
        public string DisplayUnitLabel { get; }
        public string FormatterName { get; }
    }
}
