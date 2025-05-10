using System;
using System.Drawing;
using System.Text;
using StegoLib.Models;
using StegoLib.Utilities;

namespace StegoLib.Services
{
    /// <summary>
    /// 量化索引調變 (QIM) Steganography 實作
    /// </summary>
    public class QimStegoService : IStegoService
    {
        private readonly int _quantStep;  // 量化步階 Δ

        public QimStegoService(int quantStep = 16)
        {
            if (quantStep <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantStep));
            _quantStep = quantStep;
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                // 1) 封裝訊息長度 + 內容
                var payload = PreparePayload(message);

                Bitmap bmp = new Bitmap(coverImage);
                int w = bmp.Width, h = bmp.Height, bitIdx = 0, totalBits = payload.Length * 8;

                for (int y = 0; y < h && bitIdx < totalBits; y++)
                {
                    for (int x = 0; x < w && bitIdx < totalBits; x++)
                    {
                        Color px = bmp.GetPixel(x, y);
                        int gray = (px.R + px.G + px.B) / 3;

                        int payloadByte = payload[bitIdx / 8];
                        int bit = (payloadByte >> (7 - (bitIdx % 8))) & 1;

                        // QIM: 如果 bit=0，量化到最近的偶數格；bit=1，量化到最近的奇數格
                        int qStep = _quantStep;
                        int qIndex = gray / qStep;

                        // 確保量化後會在0-255範圍內
                        int newGray;
                        if (bit == 0)
                        {
                            // 嵌入0：使用2k格
                            newGray = qIndex * qStep;
                        }
                        else
                        {
                            // 嵌入1：使用2k+1格
                            newGray = qIndex * qStep + qStep / 2;
                        }

                        // 確保灰階值在有效範圍內
                        newGray = Math.Max(0, Math.Min(255, newGray));

                        var newColor = Color.FromArgb(px.A, newGray, newGray, newGray);
                        bmp.SetPixel(x, y, newColor);

                        bitIdx++;
                    }
                    // 更新進度
                    if (progress != null)
                        progress.Report((int)((double)bitIdx / totalBits * 100));
                }

                Console.WriteLine($"嵌入完成，總共嵌入 {totalBits} bits 的訊息。");
                return new StegoResult { Success = true, Image = bmp };
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
                int w = stegoImage.Width, h = stegoImage.Height;

                // 1) 提取訊息長度 (Header)
                byte[] header = new byte[4];
                for (int i = 0; i < 32; i++) // 4 bytes = 32 bits
                {
                    int y = i / w;
                    int x = i % w;

                    if (y >= h)
                        throw new Exception("圖像尺寸不足，無法提取完整訊息");

                    Color px = stegoImage.GetPixel(x, y);
                    int gray = (px.R + px.G + px.B) / 3;

                    // 判斷灰階值是在偶數格(0)還是奇數格(1)
                    int mod = gray % _quantStep;
                    int bit = (mod >= _quantStep / 4 && mod < _quantStep * 3 / 4) ? 1 : 0;

                    // 將位元放入對應的header位置
                    int byteIdx = i / 8;
                    int bitPos = 7 - (i % 8);
                    header[byteIdx] |= (byte)(bit << bitPos);
                }

                // 2) 解析訊息長度
                int msgLen = BitConverter.ToInt32(header, 0);

                // 驗證長度合理性
                if (msgLen <= 0 || msgLen > (w * h - 4))
                    throw new Exception($"無效的訊息長度: {msgLen}");

                // 3) 提取訊息內容
                byte[] msgBytes = new byte[msgLen];
                for (int i = 0; i < msgLen * 8; i++)
                {
                    int fullIndex = i + 32; // 跳過32位元的header
                    int y = fullIndex / w;
                    int x = fullIndex % w;

                    if (y >= h)
                        throw new Exception("圖像尺寸不足，無法提取完整訊息");

                    Color px = stegoImage.GetPixel(x, y);
                    int gray = (px.R + px.G + px.B) / 3;

                    // 判斷灰階值是在偶數格(0)還是奇數格(1)
                    int mod = gray % _quantStep;
                    int bit = (mod >= _quantStep / 4 && mod < _quantStep * 3 / 4) ? 1 : 0;

                    // 將位元放入對應的訊息位置
                    int byteIdx = i / 8;
                    int bitPos = 7 - (i % 8);
                    msgBytes[byteIdx] |= (byte)(bit << bitPos);

                    // 更新進度
                    if (progress != null && i % 100 == 0)
                        progress.Report((int)((double)i / (msgLen * 8) * 100));
                }

                // 4) 將訊息內容轉換為字串
                string message = Encoding.UTF8.GetString(msgBytes);

                Console.WriteLine($"提取完成，總共提取 {msgLen * 8} bits 的訊息。");
                return new StegoResult { Success = true, Message = message };
            }
            catch (Exception ex)
            {
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private byte[] PreparePayload(string message)
        {
            var msgB = Encoding.UTF8.GetBytes(message);
            var lenB = BitConverter.GetBytes(msgB.Length);
            var buf = new byte[lenB.Length + msgB.Length];
            Array.Copy(lenB, buf, lenB.Length);
            Array.Copy(msgB, 0, buf, lenB.Length, msgB.Length);
            return buf;
        }
    }
}