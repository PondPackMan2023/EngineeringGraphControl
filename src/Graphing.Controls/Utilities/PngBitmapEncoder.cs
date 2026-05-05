using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Graphing.Controls.Utilities
{
    /// <summary>
    /// Encodes an existing <see cref="Bitmap"/> into PNG bytes.
    /// Rendering and encoding are intentionally separate concerns.
    /// </summary>
    internal static class PngBitmapEncoder
    {
        internal static byte[] Encode(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        internal static void EncodeToStream(Bitmap bitmap, Stream output)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            bitmap.Save(output, ImageFormat.Png);
        }
    }
}
