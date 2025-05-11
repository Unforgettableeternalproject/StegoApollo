using System;
using System.Drawing;
using System.Text;
using StegoLib.Models;

namespace StegoLib.Services
{
    /// <summary>
    /// DCT 頻域藏密：使用 Quantization Index Modulation (QIM) 技術
    /// 在 8x8 Block 的特定中頻係數中嵌入／提取訊息，支援彩色通道
    /// </summary>
    public class DctStegoService : IStegoService
    {
        private readonly int _delta;    // 量化步階 Δ
        private readonly int _coefIndex; // 要操作的 DCT 係數索引 (1~63)

        public DctStegoService(int delta = 16, int coefIndex = 10)
        {
            if (delta <= 0) throw new ArgumentOutOfRangeException(nameof(delta));
            if (coefIndex < 1 || coefIndex > 63) throw new ArgumentOutOfRangeException(nameof(coefIndex));
            _delta = delta;
            _coefIndex = coefIndex;
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                // 使用一個固定的種子加鹽增強穩定性
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                byte[] lenBytes = BitConverter.GetBytes(msgBytes.Length);
                byte[] payload = new byte[lenBytes.Length + msgBytes.Length];
                Array.Copy(lenBytes, payload, lenBytes.Length);
                Array.Copy(msgBytes, 0, payload, lenBytes.Length, msgBytes.Length);
                int totalBits = payload.Length * 8;

                int width = coverImage.Width;
                int height = coverImage.Height;
                int blocksX = width / 8;
                int blocksY = height / 8;
                int u = _coefIndex / 8;
                int v = _coefIndex % 8;
                int bitIndex = 0;

                // 拷貝原始圖像，避免直接修改原圖
                Bitmap stegoImage = new Bitmap(coverImage);

                // 分別處理 R, G, B 三個通道
                for (int channel = 0; channel < 3 && bitIndex < totalBits; channel++)
                {
                    // 建立通道矩陣
                    double[,] mat = new double[height, width];
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            Color p = stegoImage.GetPixel(x, y);
                            mat[y, x] = channel == 0 ? p.R : channel == 1 ? p.G : p.B;
                        }

                    // 逐 8x8 Block 嵌入
                    for (int by = 0; by < blocksY && bitIndex < totalBits; by++)
                    {
                        for (int bx = 0; bx < blocksX && bitIndex < totalBits; bx++)
                        {
                            double[,] block = DctUtils.ForwardDct(mat, bx * 8, by * 8);
                            double cRaw = block[u, v];

                            // 獲取當前要嵌入的位元
                            int payloadBit = (payload[bitIndex / 8] >> (7 - (bitIndex % 8))) & 1;

                            // 計算最接近的量化值，使其最低位與待嵌入的位元相符
                            double quantized;
                            if (payloadBit == 0)
                            {
                                // 嵌入 0：找到最接近的偶數量化值
                                int q = (int)Math.Round(cRaw / _delta);
                                q = q % 2 == 0 ? q : q - 1;  // 確保為偶數
                                quantized = q * _delta;
                            }
                            else
                            {
                                // 嵌入 1：找到最接近的奇數量化值
                                int q = (int)Math.Round(cRaw / _delta);
                                q = q % 2 == 1 ? q : q + 1;  // 確保為奇數
                                quantized = q * _delta;
                            }

                            block[u, v] = quantized;
                            DctUtils.InverseDct(block, mat, bx * 8, by * 8);
                            bitIndex++;

                            // 更新進度
                            progress?.Report((int)(((double)bitIndex / totalBits) * 100));
                        }
                    }

                    // 寫回通道
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            Color orig = stegoImage.GetPixel(x, y);
                            int val = (int)Math.Round(mat[y, x]);
                            val = Math.Min(255, Math.Max(0, val));
                            Color updated = channel == 0
                                ? Color.FromArgb(orig.A, val, orig.G, orig.B)
                                : channel == 1
                                    ? Color.FromArgb(orig.A, orig.R, val, orig.B)
                                    : Color.FromArgb(orig.A, orig.R, orig.G, val);
                            stegoImage.SetPixel(x, y, updated);
                        }
                }

                progress?.Report(100); // 確保進度條達到 100%
                return new StegoResult { Success = true, Image = stegoImage };
            }
            catch (Exception ex)
            {
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public StegoResult Extract(Bitmap stegoImage, IProgress<int> progress = null)
        {
            try
            {
                int width = stegoImage.Width;
                int height = stegoImage.Height;
                int blocksX = width / 8;
                int blocksY = height / 8;
                int u = _coefIndex / 8;
                int v = _coefIndex % 8;

                // 首先提取長度信息（32位/4字節）
                int headerBits = 32;
                byte[] lenBits = new byte[headerBits];
                int bitIndex = 0;

                // 分通道讀取長度信息
                for (int channel = 0; channel < 3 && bitIndex < headerBits; channel++)
                {
                    double[,] mat = new double[height, width];
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            Color p = stegoImage.GetPixel(x, y);
                            mat[y, x] = channel == 0 ? p.R : channel == 1 ? p.G : p.B;
                        }

                    for (int by = 0; by < blocksY && bitIndex < headerBits; by++)
                    {
                        for (int bx = 0; bx < blocksX && bitIndex < headerBits; bx++)
                        {
                            double[,] block = DctUtils.ForwardDct(mat, bx * 8, by * 8);
                            int q = (int)Math.Round(block[u, v] / _delta);
                            lenBits[bitIndex] = (byte)(q & 1);  // 直接存儲位元值
                            bitIndex++;

                            // 更新進度
                            progress?.Report((int)(((double)bitIndex / headerBits) * 10));  // 頭部佔總進度10%
                        }
                    }
                }

                // 從位元陣列重建長度值
                byte[] lenBytes = new byte[4];
                for (int i = 0; i < 32; i++)
                {
                    int byteIndex = i / 8;
                    int bitPos = 7 - (i % 8);
                    lenBytes[byteIndex] |= (byte)(lenBits[i] << bitPos);
                }

                int msgLen = BitConverter.ToInt32(lenBytes, 0);
                if (msgLen <= 0 || msgLen > (width * height * 3) / 64)  // 更合理的上限檢查
                {
                    return new StegoResult { Success = false, ErrorMessage = $"解析到無效的訊息長度: {msgLen}" };
                }

                // 提取消息內容
                byte[] msgBytes = new byte[msgLen];
                int totalBits = headerBits + (msgLen * 8);
                byte[] msgBits = new byte[msgLen * 8];
                int msgBitIndex = 0;

                // 繼續提取消息位元
                for (int channel = 0; channel < 3 && bitIndex < totalBits; channel++)
                {
                    double[,] mat = new double[height, width];
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            Color p = stegoImage.GetPixel(x, y);
                            mat[y, x] = channel == 0 ? p.R : channel == 1 ? p.G : p.B;
                        }

                    for (int by = 0; by < blocksY && bitIndex < totalBits; by++)
                    {
                        for (int bx = 0; bx < blocksX && bitIndex < totalBits; bx++)
                        {
                            double[,] block = DctUtils.ForwardDct(mat, bx * 8, by * 8);
                            int q = (int)Math.Round(block[u, v] / _delta);
                            int bit = q & 1;

                            if (bitIndex >= headerBits)
                            {
                                msgBits[msgBitIndex++] = (byte)bit;
                            }

                            bitIndex++;

                            // 更新進度
                            progress?.Report(10 + (int)(((double)(bitIndex - headerBits) / (msgLen * 8)) * 90));
                        }
                    }
                }

                // 從位元陣列重建消息
                for (int i = 0; i < msgLen * 8; i++)
                {
                    if (i < msgBits.Length)  // 確保不越界
                    {
                        int byteIndex = i / 8;
                        int bitPos = 7 - (i % 8);
                        msgBytes[byteIndex] |= (byte)(msgBits[i] << bitPos);
                    }
                }

                progress?.Report(100); // 確保進度條達到 100%
                string message = Encoding.UTF8.GetString(msgBytes);
                return new StegoResult { Success = true, Message = message };
            }
            catch (Exception ex)
            {
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}