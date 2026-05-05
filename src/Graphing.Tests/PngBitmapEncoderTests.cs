using System;
using System.Drawing;
using System.IO;
using Graphing.Controls.Utilities;
using NUnit.Framework;

namespace Graphing.Tests
{
    [TestFixture]
    public class PngBitmapEncoderTests
    {
        private static readonly byte[] PngSignature = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
        };

        [Test]
        public void Encode_ProducesNonEmptyPngBytesWithPngSignature()
        {
            using (var bitmap = new Bitmap(32, 24))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.LightBlue);
                var bytes = PngBitmapEncoder.Encode(bitmap);

                Assert.That(bytes, Is.Not.Null);
                Assert.That(bytes.Length, Is.GreaterThan(PngSignature.Length));
                AssertHasPngSignature(bytes);
            }
        }

        [Test]
        public void EncodeToStream_WritesPngBytesWithPngSignature()
        {
            using (var bitmap = new Bitmap(10, 10))
            using (var g = Graphics.FromImage(bitmap))
            using (var stream = new MemoryStream())
            {
                g.Clear(Color.OrangeRed);
                PngBitmapEncoder.EncodeToStream(bitmap, stream);

                var bytes = stream.ToArray();
                Assert.That(bytes.Length, Is.GreaterThan(PngSignature.Length));
                AssertHasPngSignature(bytes);
            }
        }

        [Test]
        public void Encode_WithNullBitmap_ThrowsArgumentNullException()
        {
            Assert.That(() => PngBitmapEncoder.Encode(null), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void EncodeToStream_WithNullOutput_ThrowsArgumentNullException()
        {
            using (var bitmap = new Bitmap(2, 2))
            {
                Assert.That(() => PngBitmapEncoder.EncodeToStream(bitmap, null), Throws.InstanceOf<ArgumentNullException>());
            }
        }

        private static void AssertHasPngSignature(byte[] bytes)
        {
            for (var i = 0; i < PngSignature.Length; i++)
            {
                Assert.That(bytes[i], Is.EqualTo(PngSignature[i]), "PNG signature mismatch at byte index " + i + ".");
            }
        }
    }
}
