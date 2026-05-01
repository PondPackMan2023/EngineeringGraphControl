namespace Graphing.Editors.EditorModels
{
    public class TitlesEditorModel
    {
        public bool HasTitleTextOverride { get; set; }
        public string TitleText { get; set; }

        public bool HasSubtitleTextOverride { get; set; }
        public string SubtitleText { get; set; }

        public bool HasTitleVisibilityOverride { get; set; }
        public bool IsTitleVisible { get; set; }

        public bool HasSubtitleVisibilityOverride { get; set; }
        public bool IsSubtitleVisible { get; set; }
    }
}
