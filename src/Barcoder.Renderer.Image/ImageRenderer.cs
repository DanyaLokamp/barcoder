using System;
using System.IO;
using System.Numerics;
using Barcoder.Renderers;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ImageSharp = SixLabors.ImageSharp;

namespace Barcoder.Renderer.Image
{
    public sealed class ImageRenderer : IRenderer
    {
        private readonly PngEncoder _pngEncoder = new PngEncoder();
        private readonly int _pixelSize;
        private readonly int _barHeightFor1DBarcode;
        private readonly int _margin;

        public ImageRenderer(int pixelSize = 10, int barHeightFor1DBarcode = 40, int margin = 5)
        {
            if (pixelSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pixelSize), "Value must be larger than zero");
            if (barHeightFor1DBarcode <= 0)
                throw new ArgumentOutOfRangeException(nameof(barHeightFor1DBarcode), "Value must be larger than zero");
            _pixelSize = pixelSize;
            _barHeightFor1DBarcode = barHeightFor1DBarcode;
            _margin = margin;
        }

        public void Render(IBarcode barcode, Stream outputStream)
        {
            barcode = barcode ?? throw new ArgumentNullException(nameof(barcode));
            outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            if (barcode.Bounds.Y == 1)
                Render1D(barcode, outputStream);
            else if (barcode.Bounds.Y > 1)
                Render2D(barcode, outputStream);
            else
                throw new NotSupportedException($"Y value of {barcode.Bounds.Y} is invalid");
        }

        private void Render1D(IBarcode barcode, Stream outputStream)
        {
            int width = (barcode.Bounds.X + 2 * _margin) * _pixelSize;
            int height = (_barHeightFor1DBarcode + 2 * _margin) * _pixelSize;

            using (var image = new ImageSharp.Image<Gray8>(width, height))
            {
                image.Mutate(ctx =>
                {
                    var black = new Gray8(0);
                    ctx.Fill(new Gray8(255));
                    for (var x = 0; x < barcode.Bounds.X; x++)
                    {
                        if (!barcode.At(x, 0))
                            continue;
                        ctx.FillPolygon(
                            black,
                            new Vector2((_margin + x) * _pixelSize, _margin * _pixelSize),
                            new Vector2((_margin + x + 1) * _pixelSize, _margin * _pixelSize),
                            new Vector2((_margin + x + 1) * _pixelSize, (_barHeightFor1DBarcode + _margin) * _pixelSize),
                            new Vector2((_margin + x) * _pixelSize, (_barHeightFor1DBarcode + _margin) * _pixelSize));
                    }
                });

                image.Save(outputStream, _pngEncoder);
            }
        }

        private void Render2D(IBarcode barcode, Stream outputStream)
        {
            int width = (barcode.Bounds.X + 2 * _margin) * _pixelSize;
            int height = (barcode.Bounds.Y + 2 * _margin) * _pixelSize;

            using (var image = new ImageSharp.Image<Gray8>(width, height))
            {
                image.Mutate(ctx =>
                {
                    var black = new Gray8(0);
                    ctx.Fill(new Gray8(255));
                    for (var y = 0; y < barcode.Bounds.Y; y++)
                    {
                        for (var x = 0; x < barcode.Bounds.X; x++)
                        {
                            if (!barcode.At(x, y))
                                continue;
                            ctx.FillPolygon(
                                black,
                                new Vector2((_margin + x) * _pixelSize, (_margin + y) * _pixelSize),
                                new Vector2((_margin + x + 1) * _pixelSize, (_margin + y) * _pixelSize),
                                new Vector2((_margin + x + 1) * _pixelSize, (_margin + y + 1) * _pixelSize),
                                new Vector2((_margin + x) * _pixelSize, (_margin + y + 1) * _pixelSize));
                        }
                    }
                });

                image.Save(outputStream, _pngEncoder);
            }
        }
    }
}
