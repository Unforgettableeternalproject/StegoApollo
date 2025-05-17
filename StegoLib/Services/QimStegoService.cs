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
            LogManager.Instance.LogDebug($"QimStegoService initialized with quantStep = {_quantStep}");
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                LogManager.Instance.LogDebug("Embed method called.");
                LogManager.Instance.LogDebug($"CoverImage Size: {coverImage.Width}x{coverImage.Height}");
                LogManager.Instance.LogDebug($"Message Length: {message.Length}");

                var payload = PreparePayload(message);
                LogManager.Instance.LogDebug($"Payload Length: {payload.Length} bytes");

                int w = coverImage.Width, h = coverImage.Height, totalBits = payload.Length * 8;
                if (totalBits > w * h)
                {
                    LogManager.Instance.LogError("Message too long for the image capacity.");
                    return new StegoResult { Success = false, ErrorMessage = "訊息太長，超過圖片容量。" };
                }

                // 1. 前處理：將像素讀入陣列
                int[,,] rgbMat = new int[h, w, 3];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        Color px = coverImage.GetPixel(x, y);
                        rgbMat[y, x, 0] = px.R;
                        rgbMat[y, x, 1] = px.G;
                        rgbMat[y, x, 2] = px.B;
                        if ((y * w + x) % 1000 == 0)
                            progress?.Report((int)Math.Round(((y * w + x) * 100.0 / (w * h)) * 0.4));
                    }

                // 2. 嵌入訊息（以 R 通道為例）
                int bitIdx = 0;
                int channel = 0; // 0:R, 1:G, 2:B
                for (int y = 0; y < h && bitIdx < totalBits; y++)
                {
                    for (int x = 0; x < w && bitIdx < totalBits; x++)
                    {
                        int orig = rgbMat[y, x, channel];
                        int payloadByte = payload[bitIdx / 8];
                        int bit = (payloadByte >> (7 - (bitIdx % 8))) & 1;

                        int qStep = _quantStep;
                        int qIndex = orig / qStep;
                        int newVal = bit == 0 ? qIndex * qStep : qIndex * qStep + qStep / 2;
                        newVal = Math.Max(0, Math.Min(255, newVal));
                        rgbMat[y, x, channel] = newVal;

                        bitIdx++;
                        if (bitIdx % 100 == 0)
                            progress?.Report((int)Math.Round(((bitIdx) * 100.0 / totalBits) * 0.4 + 40));
                    }
                }

                // 3. 寫回像素
                Bitmap bmp = new Bitmap(w, h);
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        bmp.SetPixel(x, y, Color.FromArgb(
                            255,
                            rgbMat[y, x, 0],
                            rgbMat[y, x, 1],
                            rgbMat[y, x, 2]
                        ));
                        if ((y * w + x) % 1000 == 0)
                            progress?.Report((int)Math.Round(((y * w + x) * 100.0 / (w * h)) * 0.2 + 80));
                    }

                progress?.Report(100);
                LogManager.Instance.LogSuccess($"藏密完成，總共嵌入 {totalBits} bits 的訊息。");
                return new StegoResult { Success = true, Image = bmp };
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError($"藏密失敗: {ex.Message}");
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public StegoResult Extract(Bitmap stegoImage, IProgress<int> progress = null)
        {
            try
            {
                LogManager.Instance.LogDebug("Extract method called.");
                LogManager.Instance.LogDebug($"StegoImage Size: {stegoImage.Width}x{stegoImage.Height}");

                int w = stegoImage.Width, h = stegoImage.Height;

                // 1. 前處理：將像素讀入陣列
                int[,,] rgbMat = new int[h, w, 3];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        Color px = stegoImage.GetPixel(x, y);
                        rgbMat[y, x, 0] = px.R;
                        rgbMat[y, x, 1] = px.G;
                        rgbMat[y, x, 2] = px.B;
                        if ((y * w + x) % 1000 == 0)
                            progress?.Report((int)Math.Round(((y * w + x) * 100.0 / (w * h)) * 0.4));
                    }

                // 2. 萃取訊息長度
                int channel = 0; // 0:R, 1:G, 2:B
                byte[] header = new byte[4];
                for (int i = 0; i < 32; i++)
                {
                    int y = i / w;
                    int x = i % w;
                    if (y >= h)
                        throw new Exception("圖像尺寸不足，無法提取完整訊息");

                    int val = rgbMat[y, x, channel];
                    int mod = val % _quantStep;
                    int bit = (mod >= _quantStep / 4 && mod < _quantStep * 3 / 4) ? 1 : 0;

                    int byteIdx = i / 8;
                    int bitPos = 7 - (i % 8);
                    header[byteIdx] |= (byte)(bit << bitPos);

                    if (i % 8 == 0)
                        progress?.Report((int)Math.Round((i * 100.0 / 32) * 0.2 + 40));
                }

                int msgLen = BitConverter.ToInt32(header, 0);
                LogManager.Instance.LogDebug($"Extracted message length: {msgLen}");

                if (msgLen <= 0 || msgLen > (w * h - 4))
                    throw new Exception($"無效的訊息長度: {msgLen}");

                // 3. 萃取訊息
                byte[] msgBytes = new byte[msgLen];
                for (int i = 0; i < msgLen * 8; i++)
                {
                    int fullIndex = i + 32;
                    int y = fullIndex / w;
                    int x = fullIndex % w;
                    if (y >= h)
                        throw new Exception("圖像尺寸不足，無法提取完整訊息");

                    int val = rgbMat[y, x, channel];
                    int mod = val % _quantStep;
                    int bit = (mod >= _quantStep / 4 && mod < _quantStep * 3 / 4) ? 1 : 0;

                    int byteIdx = i / 8;
                    int bitPos = 7 - (i % 8);
                    msgBytes[byteIdx] |= (byte)(bit << bitPos);

                    if (i % 100 == 0)
                        progress?.Report((int)Math.Round((i * 100.0 / (msgLen * 8)) * 0.1 + 80));
                }
                progress?.Report(100);

                string message = Encoding.UTF8.GetString(msgBytes);
                LogManager.Instance.LogSuccess($"萃取完成，總共提取 {msgLen * 8} bits 的訊息。");
                return new StegoResult { Success = true, Message = message };
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError($"萃取失敗: {ex.Message}");
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
            LogManager.Instance.LogDebug($"Prepared payload with length: {buf.Length} bytes");
            return buf;
        }
    }
}