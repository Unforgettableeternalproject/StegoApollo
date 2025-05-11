using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using StegoLib.Models;
using StegoLib.Utilities;

namespace StegoLib.Services
{
    /// <summary>
    /// 彩色圖片的DCT影像域藏密法：在RGB三個通道的8x8 Block的中頻DCT係數中嵌入訊息
    /// </summary>
    public class DctStegoService : IStegoService
    {
        private readonly int _coefIndex;
        private readonly bool _useAllChannels;
        private readonly int _channelIndex; // 0=R, 1=G, 2=B，當_useAllChannels=false時使用

        public DctStegoService(int coefIndex = 10, bool useAllChannels = true, int channelIndex = 2)
        {
            if (coefIndex < 1 || coefIndex > 63)
                throw new ArgumentOutOfRangeException(nameof(coefIndex), "係數索引必須在1-63間");

            if (!useAllChannels && (channelIndex < 0 || channelIndex > 2))
                throw new ArgumentOutOfRangeException(nameof(channelIndex), "通道索引必須為0(R)、1(G)或2(B)");

            _coefIndex = coefIndex; // 1~63 建議使用中頻係數
            _useAllChannels = useAllChannels;
            _channelIndex = channelIndex;
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                // 打包訊息：長度+內容
                var payload = PreparePayload(message);
                int w = coverImage.Width, h = coverImage.Height;

                // 計算可嵌入的容量
                int channelCount = _useAllChannels ? 3 : 1;
                int totalBlocks = (w / 8) * (h / 8) * channelCount;

                Console.WriteLine($"圖片大小: {w}x{h}，總區塊數: {(w / 8) * (h / 8)}，使用通道數: {channelCount}");
                Console.WriteLine($"總可用區塊: {totalBlocks}，訊息大小: {payload.Length} 字節，需要 {payload.Length * 8} 位元");

                if (totalBlocks < payload.Length * 8)
                    return new StegoResult { Success = false, ErrorMessage = $"圖片大小不足以嵌入完整訊息，需要{payload.Length * 8}位元，但只有{totalBlocks}可用區塊" };

                // 建立影像副本
                var bmp = new Bitmap(coverImage);

                // 鎖定圖片位元資料進行操作
                BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, w, h),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format24bppRgb); // 確保使用24位元RGB格式

                int stride = bmpData.Stride;
                int bytesPerPixel = 3; // RGB各佔一字節
                IntPtr scan0 = bmpData.Scan0;

                // 分配暫存 RGB 通道資料
                byte[][,] channels = new byte[3][,];
                for (int c = 0; c < 3; c++)
                {
                    channels[c] = new byte[h, w];
                }

                // 讀取原始RGB資料
                unsafe
                {
                    byte* p = (byte*)(void*)scan0;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int idx = y * stride + x * bytesPerPixel;
                            channels[2][y, x] = p[idx];     // B
                            channels[1][y, x] = p[idx + 1]; // G
                            channels[0][y, x] = p[idx + 2]; // R
                        }
                    }
                }

                int bitIdx = 0, totalBits = payload.Length * 8;
                int maxBlocksX = w / 8;
                int maxBlocksY = h / 8;

                // 偵錯訊息
                Console.WriteLine($"開始嵌入訊息，長度: {BitConverter.ToInt32(payload, 0)} 字節");
                Console.WriteLine($"前32位元組: {BitConverter.ToString(payload.Take(32).ToArray())}");

                // 嵌入資料
                for (int channel = 0; channel < 3 && bitIdx < totalBits; channel++)
                {
                    // 如果不使用所有通道，則只處理指定通道
                    if (!_useAllChannels && channel != _channelIndex)
                        continue;

                    for (int by = 0; by < maxBlocksY && bitIdx < totalBits; by++)
                    {
                        for (int bx = 0; bx < maxBlocksX && bitIdx < totalBits; bx++)
                        {
                            try
                            {
                                // 獲取當前8x8區塊
                                double[,] block = GetDctBlock(channels[channel], bx, by);

                                // 計算要嵌入的係數位置
                                int u = _coefIndex / 8;
                                int v = _coefIndex % 8;

                                // 讀取當前位元
                                int bytePos = bitIdx / 8;
                                int bitPos = 7 - (bitIdx % 8);
                                int payloadBit = (payload[bytePos] >> bitPos) & 1;

                                // 調整係數
                                double coef = block[u, v];
                                int coefInt = (int)Math.Round(coef);

                                // 根據嵌入位元調整係數的奇偶性
                                if ((coefInt & 1) != payloadBit)
                                {
                                    coefInt = (coefInt % 2 == 0) ? coefInt + 1 : coefInt - 1;
                                }

                                block[u, v] = coefInt;

                                // 寫回圖像資料
                                WriteDctBlock(block, channels[channel], bx, by);
                                bitIdx++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"處理區塊時出錯 (通道 {channel}, bx={bx}, by={by}): {ex.Message}");
                                continue;
                            }
                        }

                        // 更新進度
                        if (progress != null)
                            progress.Report((int)((double)bitIdx / totalBits * 100));
                    }
                }

                // 將修改後的通道資料寫回原圖
                unsafe
                {
                    byte* p = (byte*)(void*)scan0;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int idx = y * stride + x * bytesPerPixel;
                            p[idx] = channels[2][y, x];     // B
                            p[idx + 1] = channels[1][y, x]; // G
                            p[idx + 2] = channels[0][y, x]; // R
                        }
                    }
                }

                // 解鎖圖片資料
                bmp.UnlockBits(bmpData);

                Console.WriteLine($"嵌入完成，總共嵌入 {bitIdx} / {totalBits} bits 的訊息。");

                if (bitIdx < totalBits)
                    return new StegoResult { Success = false, ErrorMessage = $"僅成功嵌入部分訊息: {bitIdx}/{totalBits} bits" };

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
                int maxBlocksX = w / 8;
                int maxBlocksY = h / 8;
                int channelCount = _useAllChannels ? 3 : 1;

                // 鎖定圖片位元資料
                BitmapData bmpData = stegoImage.LockBits(
                    new Rectangle(0, 0, w, h),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);

                int stride = bmpData.Stride;
                int bytesPerPixel = 3;
                IntPtr scan0 = bmpData.Scan0;

                // 讀取RGB通道資料
                byte[][,] channels = new byte[3][,];
                for (int c = 0; c < 3; c++)
                {
                    channels[c] = new byte[h, w];
                }

                unsafe
                {
                    byte* p = (byte*)(void*)scan0;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int idx = y * stride + x * bytesPerPixel;
                            channels[2][y, x] = p[idx];     // B
                            channels[1][y, x] = p[idx + 1]; // G
                            channels[0][y, x] = p[idx + 2]; // R
                        }
                    }
                }

                // 解鎖圖片
                stegoImage.UnlockBits(bmpData);

                // 先讀取訊息長度 (4位元組) - 需要32位元
                byte[] lenBytes = new byte[4];
                int bitIdx = 0;
                StringBuilder debugHeader = new StringBuilder();

                // 處理每個通道
                for (int channel = 0; channel < 3 && bitIdx < 32; channel++)
                {
                    // 如果不使用所有通道，則只處理指定通道
                    if (!_useAllChannels && channel != _channelIndex)
                        continue;

                    for (int by = 0; by < maxBlocksY && bitIdx < 32; by++)
                    {
                        for (int bx = 0; bx < maxBlocksX && bitIdx < 32; bx++)
                        {
                            try
                            {
                                // 計算DCT變換
                                double[,] block = GetDctBlock(channels[channel], bx, by);

                                // 讀取嵌入的係數
                                int u = _coefIndex / 8;
                                int v = _coefIndex % 8;
                                int coefInt = (int)Math.Round(block[u, v]);
                                int bit = coefInt & 1; // 取最低位

                                // 記錄位元用於debug
                                debugHeader.Append(bit);

                                // 寫入對應位置
                                int byteIdx = bitIdx / 8;
                                int bitPos = 7 - (bitIdx % 8);
                                lenBytes[byteIdx] |= (byte)(bit << bitPos);

                                bitIdx++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"讀取長度時出錯 (通道 {channel}, bx={bx}, by={by}): {ex.Message}");
                                continue;
                            }
                        }
                    }
                }

                // 確認讀取到完整的長度資訊
                if (bitIdx < 32)
                    throw new Exception("無法讀取完整的訊息長度資訊");

                // 解析訊息長度
                int msgLen = BitConverter.ToInt32(lenBytes, 0);
                Console.WriteLine($"讀取到的header位元: {debugHeader}");
                Console.WriteLine($"解析出的訊息長度: {msgLen} 字節");

                // 檢查長度合理性
                int maxPossibleMsgLen = (maxBlocksX * maxBlocksY * channelCount - 4) / 8;
                if (msgLen <= 0 || msgLen > maxPossibleMsgLen)
                    throw new Exception($"解析到無效的訊息長度: {msgLen}，最大可能長度: {maxPossibleMsgLen}");

                // 讀取訊息內容
                byte[] msgBytes = new byte[msgLen];
                int msgBitIdx = 0;
                int totalMsgBits = msgLen * 8;

                // 繼續從剛才讀取位置開始抽取訊息
                for (int channel = 0; channel < 3 && msgBitIdx < totalMsgBits; channel++)
                {
                    if (!_useAllChannels && channel != _channelIndex)
                        continue;

                    for (int by = 0; by < maxBlocksY && msgBitIdx < totalMsgBits; by++)
                    {
                        for (int bx = 0; bx < maxBlocksX && msgBitIdx < totalMsgBits; bx++)
                        {
                            // 跳過用於讀取長度的前32位元
                            int globalBitIdx = (channel * maxBlocksX * maxBlocksY) + (by * maxBlocksX) + bx;

                            if (globalBitIdx < 32)
                                continue;

                            try
                            {
                                double[,] block = GetDctBlock(channels[channel], bx, by);

                                int u = _coefIndex / 8;
                                int v = _coefIndex % 8;
                                int coefInt = (int)Math.Round(block[u, v]);
                                int bit = coefInt & 1;

                                int byteIdx = msgBitIdx / 8;
                                int bitPos = 7 - (msgBitIdx % 8);
                                msgBytes[byteIdx] |= (byte)(bit << bitPos);

                                msgBitIdx++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"讀取訊息時出錯 (通道 {channel}, bx={bx}, by={by}): {ex.Message}");
                                continue;
                            }
                        }

                        // 更新進度
                        progress.Report((int)((double)msgBitIdx / totalMsgBits * 100));
                    }
                }
                progress.Report(100);
                if (msgBitIdx < totalMsgBits)
                    throw new Exception($"無法提取完整訊息，僅獲取 {msgBitIdx}/{totalMsgBits} bits");

                string msg = Encoding.UTF8.GetString(msgBytes);
                Console.WriteLine($"提取完成，總共提取 {totalMsgBits} bits 的訊息。");

                return new StegoResult { Success = true, Message = msg };
            }
            catch (Exception ex)
            {
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        // 從圖像區塊計算DCT係數
        private double[,] GetDctBlock(byte[,] channel, int blockX, int blockY)
        {
            double[,] block = new double[8, 8];

            // 複製8x8區塊資料
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int y = blockY * 8 + i;
                    int x = blockX * 8 + j;
                    block[i, j] = channel[y, x];
                }
            }

            // 進行DCT變換
            return FastDct.ForwardDct(block);
        }

        // 將DCT係數寫回圖像區塊
        private void WriteDctBlock(double[,] dctBlock, byte[,] channel, int blockX, int blockY)
        {
            // 進行反DCT變換
            double[,] block = FastDct.InverseDct(dctBlock);

            // 寫回區塊資料，並確保值在0-255範圍內
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int y = blockY * 8 + i;
                    int x = blockX * 8 + j;
                    int value = (int)Math.Round(block[i, j]);
                    channel[y, x] = (byte)Math.Max(0, Math.Min(255, value));
                }
            }
        }

        private byte[] PreparePayload(string message)
        {
            var msgB = Encoding.UTF8.GetBytes(message);
            var lenB = BitConverter.GetBytes(msgB.Length);
            var buf = new byte[lenB.Length + msgB.Length];

            // 先寫入長度，再寫入訊息內容
            Array.Copy(lenB, buf, lenB.Length);
            Array.Copy(msgB, 0, buf, lenB.Length, msgB.Length);
            return buf;
        }
    }

    // 快速DCT變換實作
    public static class FastDct
    {
        // 預先計算的余弦值表
        private static readonly double[,] cosTable;

        static FastDct()
        {
            // 初始化余弦表
            cosTable = new double[8, 8];
            for (int u = 0; u < 8; u++)
            {
                for (int x = 0; x < 8; x++)
                {
                    cosTable[u, x] = Math.Cos((2 * x + 1) * u * Math.PI / 16.0);
                }
            }
        }

        // 前向DCT變換
        public static double[,] ForwardDct(double[,] block)
        {
            // 檢查輸入大小
            if (block.GetLength(0) != 8 || block.GetLength(1) != 8)
                throw new ArgumentException("DCT區塊必須是8x8大小");

            double[,] result = new double[8, 8];

            for (int u = 0; u < 8; u++)
            {
                for (int v = 0; v < 8; v++)
                {
                    double sum = 0;
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            sum += block[x, y] *
                                   cosTable[u, x] *
                                   cosTable[v, y];
                        }
                    }

                    // 套用係數
                    double cu = (u == 0) ? 1.0 / Math.Sqrt(2) : 1.0;
                    double cv = (v == 0) ? 1.0 / Math.Sqrt(2) : 1.0;
                    result[u, v] = 0.25 * cu * cv * sum;
                }
            }

            return result;
        }

        // 反向DCT變換
        public static double[,] InverseDct(double[,] dctBlock)
        {
            // 檢查輸入大小
            if (dctBlock.GetLength(0) != 8 || dctBlock.GetLength(1) != 8)
                throw new ArgumentException("DCT區塊必須是8x8大小");

            double[,] result = new double[8, 8];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    double sum = 0;
                    for (int u = 0; u < 8; u++)
                    {
                        for (int v = 0; v < 8; v++)
                        {
                            double cu = (u == 0) ? 1.0 / Math.Sqrt(2) : 1.0;
                            double cv = (v == 0) ? 1.0 / Math.Sqrt(2) : 1.0;
                            sum += cu * cv * dctBlock[u, v] *
                                   cosTable[u, x] *
                                   cosTable[v, y];
                        }
                    }

                    result[x, y] = 0.25 * sum;
                }
            }

            return result;
        }
    }
}