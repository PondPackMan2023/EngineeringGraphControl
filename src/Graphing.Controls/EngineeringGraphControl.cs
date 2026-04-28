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
        private GraphPresentationOptions _activePresentationOptions;

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
            GraphPresentationOptions options;
            IGraphSnapshot snapshot;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
                options = _activePresentationOptions;
                snapshot = _activeSnapshot;
            }

            if (snapshot != null)
            {
                var measurementInput = _renderer.CreateMeasurementInput(e.Graphics, ClientRectangle);
                presentation = CreatePresentationModel(snapshot, options, measurementInput);
                lock (_snapshotSync)
                {
                    _activePresentation = presentation;
                }
            }

            if (presentation != null)
            {
                _renderer.Render(e.Graphics, ClientRectangle, presentation, options);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected virtual GraphPresentationModel CreatePresentationModel(
            IGraphSnapshot snapshot,
            GraphPresentationOptions options = null,
            IGraphLayoutMeasurementInput measurementInput = null)
        {
            return new GraphPresentationModel(snapshot, options, measurementInput);
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
                _activePresentationOptions = null;
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
            _activePresentationOptions = options;
            Invalidate();
        }
    }
}
