using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;

namespace WhiteBoard
{
    class CanvasSaver
    {
     public void SaveUsingEncoder(Canvas myCanvas, string fileName)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                (int)myCanvas.ActualWidth,
                (int)myCanvas.ActualHeight,
                96,
                96,
                PixelFormats.Pbgra32);

            Rect bounds = VisualTreeHelper.GetDescendantBounds(myCanvas);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(myCanvas);
                ctx.DrawRectangle(vb, null, new Rect(bounds.Location, bounds.Size));
            }
            bitmap.Render(dv);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }
    }
}
