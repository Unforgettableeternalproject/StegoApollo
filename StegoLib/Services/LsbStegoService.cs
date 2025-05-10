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

                        progress?.Report((int)(((double)bitIndex / totalBits) * 100)); // 更新進度
                    }
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
                int headerBits = 4 * 8;
                byte[] lengthBytes = new byte[4];
                int bitIndex = 0;

                // 用同一流程讀取前 32 bits
                for (int y = 0; y < stegoImage.Height && bitIndex < headerBits; y++)
                {
                    for (int x = 0; x < stegoImage.Width && bitIndex < headerBits; x++)
                    {
                        Color px = stegoImage.GetPixel(x, y);
                        int[] channels = { px.R, px.G, px.B };

                        for (int c = 0; c < 3 && bitIndex < headerBits; c++)
                        {
                            for (int b = 0; b < _bitsToUse && bitIndex < headerBits; b++)
                            {
                                int bit = (channels[c] >> b) & 1;
                                int byteIndex = bitIndex / 8;
                                lengthBytes[byteIndex] = (byte)((lengthBytes[byteIndex] << 1) | bit);
                                bitIndex++;
                            }
                        }
                    }
                }

                int msgLen = BitConverter.ToInt32(lengthBytes, 0);
                int totalMsgBits = msgLen * 8;
                byte[] msgBytes = new byte[msgLen];
                bitIndex = 0;
                int dataBitCounter = 0;

                // 繼續讀取訊息 bits
                for (int y = 0; y < stegoImage.Height && dataBitCounter < totalMsgBits; y++)
                {
                    for (int x = 0; x < stegoImage.Width && dataBitCounter < totalMsgBits; x++)
                    {
                        // 跳過已讀 header
                        if (y * stegoImage.Width + x < Math.Ceiling((double)headerBits / (_bitsToUse * 3)))
                            continue;

                        Color px = stegoImage.GetPixel(x, y);
                        int[] channels = { px.R, px.G, px.B };

                        for (int c = 0; c < 3 && dataBitCounter < totalMsgBits; c++)
                        {
                            for (int b = 0; b < _bitsToUse && dataBitCounter < totalMsgBits; b++)
                            {
                                int bit = (channels[c] >> b) & 1;
                                int byteIndex = dataBitCounter / 8;
                                msgBytes[byteIndex] = (byte)((msgBytes[byteIndex] << 1) | bit);
                                dataBitCounter++;
                            }

                            progress?.Report((int)(((double)dataBitCounter / totalMsgBits) * 100)); // 更新進度
                        }
                    }
                }

                string message = Encoding.UTF8.GetString(msgBytes);

                Console.WriteLine($"提取完成，總共提取 {totalMsgBits} bits 的訊息。");
                return new StegoResult { Success = true, Message = message };
            }
            catch (Exception ex)
            {
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
