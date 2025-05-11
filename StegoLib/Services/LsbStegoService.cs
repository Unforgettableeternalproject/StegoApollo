using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using StegoLib.Services;
using StegoLib.Models;

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
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                // 1. 將訊息轉成 bytes(UTF8)
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                int msgLen = msgBytes.Length;

                // 2. 前置 4 bytes 記錄訊息長度
                byte[] lengthBytes = BitConverter.GetBytes(msgLen);
                byte[] payload = new byte[lengthBytes.Length + msgLen];
                Array.Copy(lengthBytes, payload, lengthBytes.Length);
                Array.Copy(msgBytes, 0, payload, lengthBytes.Length, msgLen);

                int totalBits = payload.Length * 8;
                int capacity = coverImage.Width * coverImage.Height * 3 * _bitsToUse;

                if (totalBits > capacity)
                    return new StegoResult { Success = false, ErrorMessage = "訊息太長，超過圖片容量。" };

                Bitmap resultBmp = new Bitmap(coverImage);
                int bitIndex = 0;

                for (int y = 0; y < resultBmp.Height && bitIndex < totalBits; y++)
                {
                    for (int x = 0; x < resultBmp.Width && bitIndex < totalBits; x++)
                    {
                        Color px = resultBmp.GetPixel(x, y);
                        int[] channels = { px.R, px.G, px.B };

                        for (int c = 0; c < 3 && bitIndex < totalBits; c++)
                        {
                            int newVal = channels[c];

                            for (int b = 0; b < _bitsToUse && bitIndex < totalBits; b++)
                            {
                                int payloadByte = payload[bitIndex / 8];
                                int payloadBit = (payloadByte >> (7 - (bitIndex % 8))) & 1;
                                // 清除低階位後加入 payloadBit
                                newVal = (newVal & ~(1 << b)) | (payloadBit << b);
                                bitIndex++;
                            }

                            channels[c] = newVal;
                        }

                        Color newPx = Color.FromArgb(px.A, channels[0], channels[1], channels[2]);
                        resultBmp.SetPixel(x, y, newPx);
                    }

                    // 更新進度
                    if (progress != null)
                        progress.Report((int)(((double)bitIndex / totalBits) * 100));
                }

                Console.WriteLine($"嵌入完成，總共嵌入 {totalBits} bits 的訊息。");
                return new StegoResult { Success = true, Image = resultBmp };
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
                // 1. 先讀 4 bytes 的長度資訊
                byte[] lengthBytes = new byte[4];
                int bitIndex = 0;

                // 用位元位址計算像素位置
                for (int i = 0; i < lengthBytes.Length * 8; i++)
                {
                    int pixelIndex = i / (3 * _bitsToUse);
                    int x = pixelIndex % stegoImage.Width;
                    int y = pixelIndex / stegoImage.Width;

                    if (y >= stegoImage.Height)
                        throw new Exception("圖像太小，無法提取完整資訊");

                    Color px = stegoImage.GetPixel(x, y);

                    // 計算目前位元位於哪個通道和位元位置
                    int channelIndex = (i % (3 * _bitsToUse)) / _bitsToUse;
                    int bitPosition = i % _bitsToUse;

                    // 取得對應通道
                    int channelValue = 0;
                    switch (channelIndex)
                    {
                        case 0: channelValue = px.R; break;
                        case 1: channelValue = px.G; break;
                        case 2: channelValue = px.B; break;
                    }

                    // 讀取對應位元
                    int bit = (channelValue >> bitPosition) & 1;

                    // 將位元寫入對應位置
                    int byteIndex = i / 8;
                    int bitPos = 7 - (i % 8);
                    lengthBytes[byteIndex] |= (byte)(bit << bitPos);
                }

                int msgLen = BitConverter.ToInt32(lengthBytes, 0);
                if (msgLen <= 0 || msgLen > (stegoImage.Width * stegoImage.Height * 3 * _bitsToUse - 32) / 8)
                    throw new Exception($"提取到的訊息長度無效: {msgLen}");

                byte[] msgBytes = new byte[msgLen];

                // 提取訊息內容，從第33個位元開始
                for (int i = 0; i < msgLen * 8; i++)
                {
                    int fullBitIndex = i + 32; // 跳過前32位元的長度資訊
                    int pixelIndex = fullBitIndex / (3 * _bitsToUse);
                    int x = pixelIndex % stegoImage.Width;
                    int y = pixelIndex / stegoImage.Width;

                    if (y >= stegoImage.Height)
                        throw new Exception("圖像太小，無法提取完整訊息");

                    Color px = stegoImage.GetPixel(x, y);

                    // 計算目前位元位於哪個通道和位元位置
                    int channelIndex = (fullBitIndex % (3 * _bitsToUse)) / _bitsToUse;
                    int bitPosition = fullBitIndex % _bitsToUse;

                    // 取得對應通道
                    int channelValue = 0;
                    switch (channelIndex)
                    {
                        case 0: channelValue = px.R; break;
                        case 1: channelValue = px.G; break;
                        case 2: channelValue = px.B; break;
                    }

                    // 讀取對應位元
                    int bit = (channelValue >> bitPosition) & 1;

                    // 將位元寫入對應位置
                    int byteIndex = i / 8;
                    int bitPos = 7 - (i % 8);
                    msgBytes[byteIndex] |= (byte)(bit << bitPos);

                    // 更新進度
                    progress.Report((int)(((double)i / (msgLen * 8)) * 100));
                }
                progress.Report(100);
                string message = Encoding.UTF8.GetString(msgBytes);

                Console.WriteLine($"提取完成，總共提取 {msgLen * 8} bits 的訊息。");
                return new StegoResult { Success = true, Message = message };
            }
            catch (Exception ex)
            {
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}