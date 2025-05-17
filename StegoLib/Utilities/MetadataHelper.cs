using System;
using System.Drawing;

namespace StegoLib.Utilities
{
    /// <summary>
    /// 處理 LSB 存取 Metadata 的工具類
    /// 將 metadata 與主要的 histogram shifting 技術分離
    /// </summary>
    public static class MetadataHelper
    {
        /// <summary>
        /// 將 metadata 使用 LSB 技術嵌入到圖像左上角區域
        /// </summary>
        /// <param name="bmp">要嵌入 metadata 的圖像</param>
        /// <param name="metadata">要嵌入的 metadata 字節數組</param>
        public static void EmbedMetadataLSB(Bitmap bmp, byte[] metadata)
        {
            int width = bmp.Width, height = bmp.Height;
            int bitIndex = 0;
            int totalBits = metadata.Length * 8;

            // 記錄使用了多少像素作為日誌
            int pixelsUsed = 0;

            // 在圖像的左上角逐位嵌入
            for (int y = 0; y < height && bitIndex < totalBits; y++)
            {
                for (int x = 0; x < width && bitIndex < totalBits; x++)
                {
                    // 只使用 R 通道的 LSB，避免對圖像質量產生太大影響
                    Color pixel = bmp.GetPixel(x, y);

                    // 獲取當前要嵌入的位
                    int bytePos = bitIndex / 8;
                    int bitPos = 7 - (bitIndex % 8);
                    int bit = (metadata[bytePos] >> bitPos) & 1;

                    // 修改 R 通道的 LSB
                    byte newR = (byte)((pixel.R & 0xFE) | bit);

                    // 設置回像素
                    bmp.SetPixel(x, y, Color.FromArgb(pixel.A, newR, pixel.G, pixel.B));

                    bitIndex++;
                    pixelsUsed++;
                }
            }

            LogManager.Instance.Log($"Metadata 嵌入完成，使用了 {pixelsUsed} 個像素的 LSB。");
        }

        /// <summary>
        /// 從圖像的 LSB 中提取 metadata
        /// </summary>
        /// <param name="bmp">包含 metadata 的圖像</param>
        /// <param name="metadataLength">要提取的 metadata 長度（字節）</param>
        /// <returns>提取的 metadata 字節數組</returns>
        public static byte[] ExtractMetadataLSB(Bitmap bmp, int metadataLength)
        {
            int width = bmp.Width, height = bmp.Height;
            int bitIndex = 0;
            int totalBits = metadataLength * 8;

            byte[] metadata = new byte[metadataLength];
            Array.Clear(metadata, 0, metadata.Length);  // 初始化為 0

            // 從圖像左上角區域提取 metadata
            for (int y = 0; y < height && bitIndex < totalBits; y++)
            {
                for (int x = 0; x < width && bitIndex < totalBits; x++)
                {
                    // 從 R 通道讀取 LSB
                    Color pixel = bmp.GetPixel(x, y);
                    int bit = pixel.R & 1;

                    // 將 bit 設置到 metadata 中
                    int bytePos = bitIndex / 8;
                    int bitPos = 7 - (bitIndex % 8);

                    if (bit == 1)
                    {
                        metadata[bytePos] |= (byte)(1 << bitPos);
                    }

                    bitIndex++;
                }
            }

            LogManager.Instance.Log($"從 LSB 提取了 {metadataLength} 字節的 metadata。");
            return metadata;
        }
    }
}