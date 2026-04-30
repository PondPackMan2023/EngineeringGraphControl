using System.ComponentModel;

namespace Graphing.Editors.EditorModels
{
    public class AxesEditorModel
    {
        public BindingList<AxisItemEditorModel> Axes { get; }

        public AxesEditorModel()
        {
            Axes = new BindingList<AxisItemEditorModel>();
        }
    }
}
