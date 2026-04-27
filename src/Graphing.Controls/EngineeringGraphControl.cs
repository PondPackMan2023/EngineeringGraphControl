using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Snapshot;
using System;
using System.Windows.Forms;

namespace Graphing.Controls
{
    public class EngineeringGraphControl : UserControl
    {
        private readonly object _snapshotSync = new object();
        private readonly WinFormsGraphRenderer _renderer = new WinFormsGraphRenderer();

        private IGraphModel _graphModel;
        private IGraphSnapshot _activeSnapshot;
        private GraphPresentationModel _activePresentation;

        public EngineeringGraphControl()
        {
            DoubleBuffered = true;
        }

        public IGraphSnapshot ActiveSnapshot
        {
            get
            {
                lock (_snapshotSync)
                {
                    return _activeSnapshot;
                }
            }
        }

        public GraphPresentationModel ActivePresentation
        {
            get
            {
                lock (_snapshotSync)
                {
                    return _activePresentation;
                }
            }
        }

        public IGraphModel GraphModel => _graphModel;

        public void SetGraphSource(IGraphModel graphModel, GraphPresentationOptions options = null)
        {
            lock (_snapshotSync)
            {
                var snapshotBuilder = new GraphSnapshotBuilder();
                _graphModel = graphModel;
                var nextSnapshot = graphModel == null
                    ? null
                    : snapshotBuilder.Build(graphModel);

                TryInstallSnapshotAndPresentation(nextSnapshot, options);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            GraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            if (presentation != null)
            {
                _renderer.Render(e.Graphics, ClientRectangle, presentation);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected virtual GraphPresentationModel CreatePresentationModel(IGraphSnapshot snapshot,
            GraphPresentationOptions options = null)
        {
            return new GraphPresentationModel(snapshot, options);
        }

        private void TryInstallSnapshotAndPresentation(IGraphSnapshot nextSnapshot,
            GraphPresentationOptions options = null)
        {
            if (ReferenceEquals(_activeSnapshot, nextSnapshot))
            {
                return;
            }

            if (nextSnapshot == null)
            {
                _activeSnapshot = null;
                _activePresentation = null;
                return;
            }

            GraphPresentationModel nextPresentation;
            try
            {
                nextPresentation = CreatePresentationModel(nextSnapshot, options);
            }
            catch
            {
                return;
            }

            _activeSnapshot = nextSnapshot;
            _activePresentation = nextPresentation;
            Invalidate();
        }
    }
}
