using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Snapshot;
using System;
using System.Collections.Generic;
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

        public void SetGraphSource(IGraphModel graphModel)
        {
            lock (_snapshotSync)
            {
                var snapshotBuilder = new GraphSnapshotBuilder();
                _graphModel = graphModel;
                var nextSnapshot = graphModel == null
                    ? null
                    : snapshotBuilder.Build(graphModel);

                TryInstallSnapshotAndPresentation(nextSnapshot);
            }
        }

        public void NotifyDomainChanged(int elementTypeId, int[] elementIds, int[] attributeIds)
        {
            if (attributeIds == null || attributeIds.Length == 0)
            {
                return;
            }

            lock (_snapshotSync)
            {
                if (_graphModel == null || _activeSnapshot == null)
                {
                    return;
                }

                var snapshotBuilder = new GraphSnapshotBuilder();
                var nextSnapshot = snapshotBuilder.Build(_graphModel);
                TryInstallSnapshotAndPresentation(nextSnapshot);
            }
        }

        public void NotifyFormattersChanged(IReadOnlyCollection<string> changedFormatterNames)
        {
            if (changedFormatterNames == null || changedFormatterNames.Count == 0)
            {
                return;
            }

            lock (_snapshotSync)
            {
                if (_graphModel == null || _activeSnapshot == null)
                {
                    return;
                }

                if (!SnapshotUsesFormatter(_activeSnapshot, changedFormatterNames))
                {
                    return;
                }

                var snapshotBuilder = new GraphSnapshotBuilder();
                var nextSnapshot = snapshotBuilder.Build(_graphModel);
                TryInstallSnapshotAndPresentation(nextSnapshot);
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

        protected virtual GraphPresentationModel CreatePresentationModel(IGraphSnapshot snapshot)
        {
            return new GraphPresentationModel(snapshot);
        }

        private void TryInstallSnapshotAndPresentation(IGraphSnapshot nextSnapshot)
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
                nextPresentation = CreatePresentationModel(nextSnapshot);
            }
            catch
            {
                return;
            }

            _activeSnapshot = nextSnapshot;
            _activePresentation = nextPresentation;
            Invalidate();
        }

        private static bool SnapshotUsesFormatter(
            IGraphSnapshot snapshot,
            IReadOnlyCollection<string> changedFormatterNames)
        {
            var formatterNameSet = new HashSet<string>(changedFormatterNames);

            for (var axisIndex = 0; axisIndex < snapshot.Axes.Count; axisIndex++)
            {
                var formatterName = snapshot.Axes[axisIndex].FormatterName;
                if (formatterName != null && formatterNameSet.Contains(formatterName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
