using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Graphing.Core.Pie.Presentation
{
    public static class PiePalette
    {
        private static readonly IReadOnlyList<PieColor> _default = new ReadOnlyCollection<PieColor>(
            new List<PieColor>
            {
                PieColor.FromArgb(255, 31, 119, 180),
                PieColor.FromArgb(255, 255, 127, 14),
                PieColor.FromArgb(255, 44, 160, 44),
                PieColor.FromArgb(255, 214, 39, 40),
                PieColor.FromArgb(255, 148, 103, 189),
                PieColor.FromArgb(255, 140, 86, 75),
                PieColor.FromArgb(255, 227, 119, 194),
                PieColor.FromArgb(255, 127, 127, 127),
                PieColor.FromArgb(255, 188, 189, 34),
                PieColor.FromArgb(255, 23, 190, 207),
                PieColor.FromArgb(255, 57, 59, 121),
                PieColor.FromArgb(255, 99, 121, 57),
                PieColor.FromArgb(255, 140, 109, 49),
                PieColor.FromArgb(255, 181, 207, 107),
                PieColor.FromArgb(255, 173, 73, 74),
                PieColor.FromArgb(255, 107, 76, 154)
            });

        public static IReadOnlyList<PieColor> Default => _default;

        public static PieColor GetColorForIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (_default.Count == 0)
            {
                return PieColor.Empty;
            }

            return _default[index % _default.Count];
        }
    }
}
