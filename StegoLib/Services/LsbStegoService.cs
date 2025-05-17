using System;
using System.Text;
using System.Drawing;
using StegoLib.Models;
using StegoLib.Utilities;

namespace StegoLib.Services
{
    public class LsbStegoService : IStegoService
    {
        private readonly int _bitsToUse;

        /// <param name="bitsToUse">每個顏色通道使用多少低階位 (1-2 建議)</param>
        public LsbStegoService(int bitsToUse = 1)
        {
            if (bitsToUse < 1 || bitsToUse > 2)
                throw new ArgumentOutOfRangeException(nameof(bitsToUse), "bitsToUse 必須介於 1~2 之間");

            _bitsToUse = bitsToUse;
            LogManager.Instance.LogDebug($"LsbStegoService initialized with bitsToUse = {_bitsToUse}");
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                LogManager.Instance.LogDebug("Embed method called.");
                LogManager.Instance.LogDebug($"CoverImage Size: {coverImage.Width}x{coverImage.Height}");
                LogManager.Instance.LogDebug($"Message Length: {message.Length}");

                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                int msgLen = msgBytes.Length;

                byte[] lengthBytes = BitConverter.GetBytes(msgLen);
                byte[] payload = new byte[lengthBytes.Length + msgLen];
                Array.Copy(lengthBytes, payload, lengthBytes.Length);
                Array.Copy(msgBytes, 0, payload, lengthBytes.Length, msgLen);

                int totalBits = payload.Length * 8;
                int capacity = coverImage.Width * coverImage.Height * 3 * _bitsToUse;

                LogManager.Instance.LogDebug($"Payload Length: {payload.Length} bytes, Total Bits: {totalBits}, Capacity: {capacity}");

                if (totalBits > capacity)
                {
                    LogManager.Instance.LogDebug("Message too long for the image capacity.");
                    return new StegoResult { Success = false, ErrorMessage = "訊息太長，超過圖片容量。" };
                }

                int w = coverImage.Width, h = coverImage.Height;
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

                // 2. 嵌入訊息
                int bitIndex = 0;
                for (int y = 0; y < h && bitIndex < totalBits; y++)
                {
                    for (int x = 0; x < w && bitIndex < totalBits; x++)
                    {
                        for (int c = 0; c < 3 && bitIndex < totalBits; c++)
                        {
                            int newVal = rgbMat[y, x, c];
                            for (int b = 0; b < _bitsToUse && bitIndex < totalBits; b++)
                            {
                                int payloadByte = payload[bitIndex / 8];
                                int payloadBit = (payloadByte >> (7 - (bitIndex % 8))) & 1;
                                newVal = (newVal & ~(1 << b)) | (payloadBit << b);
                                bitIndex++;
                            }
                            rgbMat[y, x, c] = newVal;
                        }
                        if (bitIndex % 100 == 0)
                            progress?.Report((int)Math.Round(((bitIndex) * 100.0 / totalBits) * 0.4 + 40));
                    }
                }

                // 3. 寫回像素
                Bitmap resultBmp = new Bitmap(w, h);
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        resultBmp.SetPixel(x, y, Color.FromArgb(
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
                return new StegoResult { Success = true, Image = resultBmp };
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

                // 2. 先讀 4 bytes 的長度資訊
                byte[] lengthBytes = new byte[4];
                int bitIndex = 0;
                int totalBitsForLen = lengthBytes.Length * 8;
                for (int i = 0; i < totalBitsForLen; i++)
                {
                    int pixelIndex = i / (3 * _bitsToUse);
                    int x = pixelIndex % w;
                    int y = pixelIndex / w;
                    if (y >= h)
                        throw new Exception("圖像太小，無法提取完整資訊");

                    int channelIndex = (i % (3 * _bitsToUse)) / _bitsToUse;
                    int bitPosition = i % _bitsToUse;
                    int channelValue = rgbMat[y, x, channelIndex];
                    int bit = (channelValue >> bitPosition) & 1;

                    int byteIndex = i / 8;
                    int bitPos = 7 - (i % 8);
                    lengthBytes[byteIndex] |= (byte)(bit << bitPos);

                    if (i % 8 == 0)
                        progress?.Report((int)Math.Round((i * 100.0 / totalBitsForLen) * 0.2 + 40));
                }

                int msgLen = BitConverter.ToInt32(lengthBytes, 0);
                LogManager.Instance.LogDebug($"Extracted message length: {msgLen}");

                if (msgLen <= 0 || msgLen > (w * h * 3 * _bitsToUse - 32) / 8)
                    throw new Exception($"提取到的訊息長度無效: {msgLen}");

                // 3. 提取訊息內容
                byte[] msgBytes = new byte[msgLen];
                int totalBits = msgLen * 8;
                for (int i = 0; i < totalBits; i++)
                {
                    int fullBitIndex = i + 32;
                    int pixelIndex = fullBitIndex / (3 * _bitsToUse);
                    int x = pixelIndex % w;
                    int y = pixelIndex / w;
                    if (y >= h)
                        throw new Exception("圖像太小，無法提取完整訊息");

                    int channelIndex = (fullBitIndex % (3 * _bitsToUse)) / _bitsToUse;
                    int bitPosition = fullBitIndex % _bitsToUse;
                    int channelValue = rgbMat[y, x, channelIndex];
                    int bit = (channelValue >> bitPosition) & 1;

                    int byteIndex = i / 8;
                    int bitPos = 7 - (i % 8);
                    msgBytes[byteIndex] |= (byte)(bit << bitPos);

                    if (i % 100 == 0)
                        progress?.Report((int)Math.Round((i * 100.0 / totalBits) * 0.1 + 80));
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
    }
}