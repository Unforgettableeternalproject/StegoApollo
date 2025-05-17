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
                        int gray = (int)Math.Round(0.299 * px.R + 0.587 * px.G + 0.114 * px.B);

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
                int width = stegoImage.Width;
                int height = stegoImage.Height;

                // 1. 灰階矩陣
                double[,] gray = ImageHelper.ToGrayscaleMatrix(stegoImage);

                // 2. 首先找到可能的候選峰值 (通過觀察直方圖)
                int[] hist = new int[256];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        hist[(int)gray[y, x]]++;

                // 找出直方圖中有明顯間隙的候選點 (peak, peak+1)
                int candidatePeak = -1;
                for (int i = 0; i < 255; i++)
                {
                    if (hist[i] > 0 && hist[i + 1] > 0)
                    {
                        candidatePeak = i;
                        break;
                    }
                }

                if (candidatePeak == -1)
                    return new StegoResult { Success = false, ErrorMessage = "無法找到候選峰值" };

                    Color px = stegoImage.GetPixel(x, y);
                    int gray = (int)Math.Round(0.299 * px.R + 0.587 * px.G + 0.114 * px.B);
                    // 判斷灰階值是在偶數格(0)還是奇數格(1)
                    int mod = gray % _quantStep;
                    int bit = (mod >= _quantStep / 4 && mod < _quantStep * 3 / 4) ? 1 : 0;

                // 只讀取足夠的像素來獲取元資料
                for (int y = 0; y < height && bitIndex < metaBits; y++)
                {
                    for (int x = 0; x < width && bitIndex < metaBits; x++)
                    {
                        int v = (int)gray[y, x];
                        if (v == _peak || v == _peak + 1)
                        {
                            int bit = v == _peak + 1 ? 1 : 0;
                            int bytePos = bitIndex / 8;
                            int shift = 7 - (bitIndex % 8);
                            metaBytes[bytePos] |= (byte)(bit << shift);
                            bitIndex++;
                        }
                    }
                }

                // 從元資料中獲取實際的峰值
                _peak = metaBytes[0];

                // 4. 現在使用正確的峰值重新提取所有資料（包括長度）
                bitIndex = 0;
                int headerBits = (2 + 4) * 8; // 2字節元資料 + 4字節長度
                byte[] header = new byte[6];

                for (int y = 0; y < height && bitIndex < headerBits; y++)
                {
                    for (int x = 0; x < width && bitIndex < headerBits; x++)
                    {
                        int v = (int)gray[y, x];
                        if (v == _peak || v == _peak + 1)
                        {
                            int bit = v == _peak + 1 ? 1 : 0;
                            int bytePos = bitIndex / 8;
                            int shift = 7 - (bitIndex % 8);
                            header[bytePos] |= (byte)(bit << shift);
                            bitIndex++;
                        }
                    }

                    // 更新進度
                    progress?.Report((int)(((double)bitIndex / headerBits) * 100 / 2)); // 提取header佔總進度的50%
                }

                // 5. 解析長度
                int msgLen = BitConverter.ToInt32(header, 2);
                if (msgLen < 0 || msgLen > width * height / 8) // 合理性檢查
                    return new StegoResult { Success = false, ErrorMessage = $"無效的訊息長度: {msgLen}" };

                // 6. 提取訊息bits
                int totalBits = (2 + 4 + msgLen) * 8;
                byte[] payload = new byte[2 + 4 + msgLen];
                Array.Copy(header, payload, header.Length); // 複製已讀取的header
                bitIndex = headerBits; // 從已讀取的位置繼續

                for (int y = 0; y < height && bitIndex < totalBits; y++)
                {
                    for (int x = 0; x < width && bitIndex < totalBits; x++)
                    {
                        int v = (int)gray[y, x];
                        if (v == _peak || v == _peak + 1)
                        {
                            int bit = v == _peak + 1 ? 1 : 0;
                            int bytePos = bitIndex / 8;
                            int shift = 7 - (bitIndex % 8);
                            payload[bytePos] |= (byte)(bit << shift);
                            bitIndex++;
                        }
                    }

                    // 更新進度
                    progress?.Report(50 + (int)(((double)bitIndex / totalBits) * 100 / 2));
                }

                // 7. 復原直方圖: 將所有大於peak+1的值減1
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        if (gray[y, x] > _peak + 1)
                            gray[y, x]--;
                    }

                // 8. 解析訊息
                byte[] msgBytes = new byte[msgLen];
                Array.Copy(payload, 6, msgBytes, 0, msgLen);
                string message = Encoding.UTF8.GetString(msgBytes);
                progress?.Report(100);
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