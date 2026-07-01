using System;

namespace Graphing.Controls.Interaction
{
    /// <summary>
    /// Immutable payload for animation bar X-index changes.
    /// </summary>
    public sealed class AnimationBarIndexChangedEventArgs : EventArgs
    {
        public AnimationBarIndexChangedEventArgs(int xIndex, int? previousXIndex, bool isUserInitiated)
        {
            XIndex = xIndex;
            PreviousXIndex = previousXIndex;
            IsUserInitiated = isUserInitiated;
        }

        public int XIndex { get; }

        public int? PreviousXIndex { get; }

        public bool IsUserInitiated { get; }
    }
}
