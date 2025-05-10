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

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress=null)
        {
            try
            {
                // 1) 封裝訊息長度 + 內容，跟 LSB 程式類似
                var payload = PreparePayload(message);

                Bitmap bmp = new Bitmap(coverImage);
                int w = bmp.Width, h = bmp.Height, bitIdx = 0, totalBits = payload.Length * 8;

                for (int y = 0; y < h && bitIdx < totalBits; y++)
                {
                    for (int x = 0; x < w && bitIdx < totalBits; x++)
                    {
                        Color px = bmp.GetPixel(x, y);
                        int gray = (px.R + px.G + px.B) / 3;

                        int bit = (payload[bitIdx / 8] >> (7 - (bitIdx % 8))) & 1;
                        // QIM: 如果 bit=0，量化到最近的偶數格；bit=1，量化到最近的奇數格
                        int qBase = (gray / _quantStep) * _quantStep;
                        int qEven = qBase;
                        int qOdd = qBase + _quantStep / 2;

                        int newGray = (Math.Abs(gray - qEven) <= Math.Abs(gray - qOdd))
                                      ? (bit == 0 ? qEven : qOdd)
                                      : (bit == 0 ? qOdd : qEven);

                        var newColor = Color.FromArgb(px.A, newGray, newGray, newGray);
                        bmp.SetPixel(x, y, newColor);

                        bitIdx++;
                    }
                    // 更新進度
                    progress?.Report((int)((double)bitIdx / totalBits * 100));
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
                int bitIdx = 0;

                // 1) 提取訊息長度 (Header)
                byte[] header = new byte[4];
                for (int y = 0; y < h && bitIdx < header.Length * 8; y++)
                {
                    for (int x = 0; x < w && bitIdx < header.Length * 8; x++)
                    {
                        Color px = stegoImage.GetPixel(x, y);
                        int gray = (px.R + px.G + px.B) / 3;

                        // 判斷灰階值落在偶數格 (0) 還是奇數格 (1)
                        int bit = (gray % _quantStep) >= (_quantStep / 2) ? 1 : 0;

                        // 將提取的 bit 填入 header
                        header[bitIdx / 8] = (byte)((header[bitIdx / 8] << 1) | bit);
                        bitIdx++;
                    }
                }

                // 2) 解析訊息長度
                int msgLen = BitConverter.ToInt32(header, 0);
                if (msgLen <= 0)
                    throw new Exception("無效的訊息長度。");

                // 3) 提取訊息內容
                byte[] msgBytes = new byte[msgLen];
                bitIdx = 0;
                for (int y = 0; y < h && bitIdx < msgLen * 8; y++)
                {
                    for (int x = 0; x < w && bitIdx < msgLen * 8; x++)
                    {
                        // 跳過已經讀取 header 的像素
                        if (y * w + x < Math.Ceiling((double)(header.Length * 8) / (w * h)))
                            continue;

                        Color px = stegoImage.GetPixel(x, y);
                        int gray = (px.R + px.G + px.B) / 3;

                        // 判斷灰階值落在偶數格 (0) 還是奇數格 (1)
                        int bit = (gray % _quantStep) >= (_quantStep / 2) ? 1 : 0;

                        // 將提取的 bit 填入訊息內容
                        msgBytes[bitIdx / 8] = (byte)((msgBytes[bitIdx / 8] << 1) | bit);
                        bitIdx++;

                        // 更新進度
                        progress?.Report((int)((double)bitIdx / (msgLen * 8) * 100));
                    }
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
