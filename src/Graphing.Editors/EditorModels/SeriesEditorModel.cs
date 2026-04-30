using System.ComponentModel;

namespace Graphing.Editors.EditorModels
{
    public class SeriesEditorModel
    {
        public BindingList<SeriesItemEditorModel> Series { get; }

        public SeriesEditorModel()
        {
            Series = new BindingList<SeriesItemEditorModel>();
        }

        public void MoveUp(SeriesItemEditorModel item)
        {
            int index = Series.IndexOf(item);
            if (index <= 0)
            {
                return;
            }

            Series.RemoveAt(index);
            Series.Insert(index - 1, item);
        }

        public void MoveDown(SeriesItemEditorModel item)
        {
            int index = Series.IndexOf(item);
            if (index < 0 || index >= Series.Count - 1)
            {
                return;
            }

            Series.RemoveAt(index);
            Series.Insert(index + 1, item);
        }
    }
}
