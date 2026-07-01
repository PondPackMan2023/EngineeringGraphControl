using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using System.Drawing;
using System.IO;

namespace Graphing.Controls.Utilities
{
    public static class GraphExport
    {
        public static void ExportPng(
            Size size,
            GraphPresentationModel model,
            Stream output,
            GraphPresentationOptions options = null)
        {
            var renderer = new BitmapGraphRenderer();
            using (var bitmap = renderer.RenderToBitmap(size.Width, size.Height, model, options))
            {
                PngBitmapEncoder.EncodeToStream(bitmap, output);
            }
        }

        public static void ExportMetafile(
            Size size,
            GraphPresentationModel model,
            Stream output,
            GraphPresentationOptions options = null)
        {
            var renderer = new MetafileGraphRenderer();
            renderer.RenderToMetafile(size.Width, size.Height, output, model, options);
        }

        public static void ExportMetafile(
            Size size,
            GraphPresentationModel model,
            string filePath,
            GraphPresentationOptions options = null)
        {
            using (var stream = File.Create(filePath))
            {
                ExportMetafile(size, model, stream, options);
            }
        }
    }
}
