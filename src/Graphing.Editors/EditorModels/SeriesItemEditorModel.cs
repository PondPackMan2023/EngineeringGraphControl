using System;
using System.Drawing;
using Graphing.Controls.Models;

namespace Graphing.Editors.EditorModels
{
    public class SeriesItemEditorModel
    {
        public SeriesId SeriesId { get; }

        public bool IsVisible { get; set; }

        public bool HasLabelOverride { get; set; }
        public string Label { get; set; }

        public bool HasColorOverride { get; set; }
        public Color Color { get; set; }

        public SeriesItemEditorModel(SeriesId seriesId)
        {
            SeriesId = seriesId ?? throw new ArgumentNullException(nameof(seriesId));
        }
    }
}
