using System;
using System.Drawing.Imaging;
using System.Drawing;

namespace StegoLib.Utilities
{
    public class ImageHelper
    {
        public static Bitmap Load(string path)
        {
            return new Bitmap(path);
        }

        public static void Save(Bitmap bmp, string path, ImageFormat format = null)
        {
            format = format ?? ImageFormat.Png;
            bmp.Save(path, format);
        }

        public static double[,] ToGrayscaleMatrix(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            var m = new double[h, w];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var p = bmp.GetPixel(x, y);
                    m[y, x] = (p.R + p.G + p.B) / 3.0;
                }
            return m;
        }

        public static void WriteGrayscaleMatrix(Bitmap bmp, double[,] m)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int v = (int)Math.Round(m[y, x]);
                    v = Math.Min(255, Math.Max(0, v));
                    bmp.SetPixel(x, y, Color.FromArgb(v, v, v));
                }
        }
    }
}
