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
    /// DCT 影像域藏密法：在 8x8 Block 的中頻 DCT Coefficient 中嵌入訊息
    /// </summary>
    public class DctStegoService : IStegoService
    {
        private readonly int _coefIndex;

        public DctStegoService(int coefIndex = 10)
        {
            if (coefIndex < 1 || coefIndex > 63)
                throw new ArgumentOutOfRangeException(nameof(coefIndex), "係數索引必須在1-63間");

            _coefIndex = coefIndex; // 1~63 建議
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                var payload = PreparePayload(message);
                int w = coverImage.Width, h = coverImage.Height;

                // 檢查圖像尺寸是否足夠
                int totalBlocks = (w / 8) * (h / 8);
                if (totalBlocks * 1 < payload.Length * 8)
                    return new StegoResult { Success = false, ErrorMessage = "圖片大小不足以嵌入完整訊息" };

                var bmp = new Bitmap(coverImage);

                // 將影像轉灰階
                var gray = ImageHelper.ToGrayscaleMatrix(bmp);
                int bitIdx = 0, totalBits = payload.Length * 8;

                // 確保只處理對齊到8x8的圖像區域
                int maxBlocksX = w / 8;
                int maxBlocksY = h / 8;

                // 輸出訊息長度到控制台以便除錯
                Console.WriteLine($"嵌入的訊息長度: {BitConverter.ToInt32(payload, 0)} 字節");

                // 逐 8x8 Block 處理
                for (int by = 0; by < maxBlocksY && bitIdx < totalBits; by++)
                {
                    for (int bx = 0; bx < maxBlocksX && bitIdx < totalBits; bx++)
                    {
                        int blockX = bx * 8;
                        int blockY = by * 8;

                        try
                        {
                            // 對當前8x8區塊進行DCT變換
                            double[,] block = DctUtils.ForwardDct(gray, blockX, blockY);

                            // 中頻係數嵌入 (省略直流與高頻)
                            int u = _coefIndex / 8;
                            int v = _coefIndex % 8;

                            // 從payload取出要嵌入的位元
                            int bytePos = bitIdx / 8;
                            int bitPos = 7 - (bitIdx % 8);
                            int payloadBit = (payload[bytePos] >> bitPos) & 1;

                            // 調整係數奇偶性
                            double coef = block[u, v];
                            int coefInt = (int)Math.Round(coef);

                            // 根據要嵌入的位元調整係數值
                            if ((coefInt & 1) != payloadBit)
                            {
                                // 如果係數奇偶性不符合需要嵌入的位元，則調整
                                coefInt = (coefInt % 2 == 0) ? coefInt + 1 : coefInt - 1;
                            }

                            block[u, v] = coefInt;

                            // 反 DCT 寫回 gray
                            DctUtils.InverseDct(block, gray, blockX, blockY);
                            bitIdx++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"處理區塊 ({bx}, {by}) 時出錯: {ex.Message}");
                            // 跳過有問題的區塊，繼續處理
                            continue;
                        }
                    }

                    // 更新進度
                    if (progress != null)
                        progress.Report((int)((double)bitIdx / totalBits * 100));
                }

                // 將 gray 寫回 Bitmap
                ImageHelper.WriteGrayscaleMatrix(bmp, gray);

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
                var bmp = new Bitmap(stegoImage);
                var gray = ImageHelper.ToGrayscaleMatrix(bmp);
                int w = bmp.Width, h = bmp.Height;

                // 確保只處理對齊到8x8的圖像區域
                int maxBlocksX = w / 8;
                int maxBlocksY = h / 8;

                // 1. 先讀取長度資訊 (4 bytes = 32 bits)
                byte[] header = new byte[4];
                int headerBitIdx = 0;

                // 除錯資訊
                StringBuilder headerBits = new StringBuilder();

                for (int by = 0; by < maxBlocksY && headerBitIdx < 32; by++)
                {
                    for (int bx = 0; bx < maxBlocksX && headerBitIdx < 32; bx++)
                    {
                        int blockX = bx * 8;
                        int blockY = by * 8;

                        try
                        {
                            // 對當前8x8區塊進行DCT變換
                            double[,] block = DctUtils.ForwardDct(gray, blockX, blockY);

                            // 讀取中頻係數
                            int u = _coefIndex / 8;
                            int v = _coefIndex % 8;

                            int coefInt = (int)Math.Round(block[u, v]);
                            int bit = coefInt & 1; // 獲取最低位的奇偶性

                            // 將讀取的位元放入header中
                            int byteIdx = headerBitIdx / 8;
                            int bitPos = 7 - (headerBitIdx % 8);
                            header[byteIdx] |= (byte)(bit << bitPos);

                            // 除錯：記錄讀取的位元
                            headerBits.Append(bit);

                            headerBitIdx++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"處理header區塊 ({bx}, {by}) 時出錯: {ex.Message}");
                            // 跳過有問題的區塊，繼續處理
                            continue;
                        }
                    }
                }

                if (headerBitIdx < 32)
                    throw new Exception("無法讀取完整的訊息長度資訊");

                // 除錯：輸出header位元
                Console.WriteLine($"讀取到的header位元: {headerBits}");
                Console.WriteLine($"解析出的消息長度: {BitConverter.ToInt32(header, 0)}");

                // 2. 解析訊息長度
                int msgLen = BitConverter.ToInt32(header, 0);

                // 檢查長度合理性
                int maxPossibleMsgLen = (maxBlocksX * maxBlocksY - 4) / 8;
                if (msgLen <= 0 || msgLen > maxPossibleMsgLen)
                    throw new Exception($"解析到無效的訊息長度: {msgLen}，最大可能長度: {maxPossibleMsgLen}");

                // 3. 提取訊息內容
                byte[] msgBytes = new byte[msgLen];
                int msgBitIdx = 0;
                int totalMsgBits = msgLen * 8;

                // 從header後的區塊開始讀取
                int totalBlocksProcessed = 0;
                bool headerProcessed = false;

                for (int by = 0; by < maxBlocksY; by++)
                {
                    for (int bx = 0; bx < maxBlocksX; bx++)
                    {
                        totalBlocksProcessed++;

                        // 跳過用於header的區塊
                        if (!headerProcessed && totalBlocksProcessed <= 32)
                            continue;

                        if (!headerProcessed)
                            headerProcessed = true;

                        if (msgBitIdx >= totalMsgBits)
                            break;

                        int blockX = bx * 8;
                        int blockY = by * 8;

                        try
                        {
                            // 對當前8x8區塊進行DCT變換
                            double[,] block = DctUtils.ForwardDct(gray, blockX, blockY);

                            // 讀取中頻係數
                            int u = _coefIndex / 8;
                            int v = _coefIndex % 8;

                            int coefInt = (int)Math.Round(block[u, v]);
                            int bit = coefInt & 1; // 獲取最低位的奇偶性

                            // 將讀取的位元放入訊息中
                            int byteIdx = msgBitIdx / 8;
                            int bitPos = 7 - (msgBitIdx % 8);
                            msgBytes[byteIdx] |= (byte)(bit << bitPos);

                            msgBitIdx++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"處理訊息區塊 ({bx}, {by}) 時出錯: {ex.Message}");
                            // 跳過有問題的區塊，繼續處理
                            continue;
                        }
                    }

                    // 更新進度
                    if (progress != null)
                        progress.Report((int)((double)msgBitIdx / totalMsgBits * 100));

                    if (msgBitIdx >= totalMsgBits)
                        break;
                }

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