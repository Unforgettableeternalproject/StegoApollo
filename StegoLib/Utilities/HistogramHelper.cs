using System.Drawing;

namespace StegoLib.Utilities
{
    public static class HistogramHelper
    {
        /// <summary>
        /// 計算一張 Bitmap 的灰階直方圖 (0~255)
        /// </summary>
        public static int[] ComputeGrayHistogram(Bitmap bmp)
        {
            var hist = new int[256];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color p = bmp.GetPixel(x, y);
                    // 取平均作為灰階值
                    int gray = (p.R + p.G + p.B) / 3;
                    hist[gray]++;
                }
            }
            return hist;
        }
    }
}
