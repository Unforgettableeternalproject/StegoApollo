// StegoLib/Services/EnhancedHistShiftStegoService.cs
using System;
using System.Drawing;
using System.Text;
using StegoLib.Models;
using StegoLib.Utilities;
using StegoLib.Services;

namespace StegoLib.Services
{
    /// <summary>
    /// 增強版可逆直方圖位移 (Histogram Shifting) 隱寫
    /// 1. 使用 LSB 儲存 metadata
    /// 2. 支援 RGB + 灰階四通道
    /// 3. 提供 Progress 回傳
    /// </summary>
    public class HistShiftStegoService : IStegoService
    {
        // 每個通道的 peak 和 zero 點
        private int _peakR, _peakG, _peakB, _peakGray;
        private int _zeroR, _zeroG, _zeroB, _zeroGray;

        // Metadata 的大小（字節）
        private const int METADATA_SIZE = 8; // 4個通道，每個通道2個字節(peak + zero)

        public HistShiftStegoService() { }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                int width = coverImage.Width;
                int height = coverImage.Height;
                LogManager.Instance.Log($"開始嵌入，圖像大小: {width}x{height}");

                // 1. 建立四通道矩陣
                int[,] rCh = new int[height, width];
                int[,] gCh = new int[height, width];
                int[,] bCh = new int[height, width];
                int[,] grayCh = new int[height, width];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var p = coverImage.GetPixel(x, y);
                        rCh[y, x] = p.R;
                        gCh[y, x] = p.G;
                        bCh[y, x] = p.B;
                        grayCh[y, x] = (p.R + p.G + p.B) / 3; // 簡單平均作為灰階值
                    }
                }

                // 2. 計算直方圖並找峰值
                var histR = HistogramHelper.ComputeChannelHistogram(coverImage, channel: 'R');
                var histG = HistogramHelper.ComputeChannelHistogram(coverImage, channel: 'G');
                var histB = HistogramHelper.ComputeChannelHistogram(coverImage, channel: 'B');
                var histGray = HistogramHelper.ComputeGrayHistogram(coverImage);

                _peakR = ArgMax(histR);
                _peakG = ArgMax(histG);
                _peakB = ArgMax(histB);
                _peakGray = ArgMax(histGray);

                // 3. 尋找 zero-point，有 zero 才用 zero，否則用最少頻率點
                _zeroR = TryFindZeroPoint(histR, _peakR);
                _zeroG = TryFindZeroPoint(histG, _peakG);
                _zeroB = TryFindZeroPoint(histB, _peakB);
                _zeroGray = TryFindZeroPoint(histGray, _peakGray);

                LogManager.Instance.Log($"Peak/Zero - R:{_peakR}/{_zeroR}, G:{_peakG}/{_zeroG}, B:{_peakB}/{_zeroB}, Gray:{_peakGray}/{_zeroGray}");

                // 4. 準備 metadata (8 bytes = 4個通道 * 2個值)
                byte[] metadata = new byte[] {
                    (byte)_peakR, (byte)_zeroR,
                    (byte)_peakG, (byte)_zeroG,
                    (byte)_peakB, (byte)_zeroB,
                    (byte)_peakGray, (byte)_zeroGray
                };

                // 5. 準備 payload (只含長度 + 數據，不含 metadata)
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                byte[] lenBytes = BitConverter.GetBytes(msgBytes.Length);
                byte[] payload = new byte[lenBytes.Length + msgBytes.Length];
                Array.Copy(lenBytes, 0, payload, 0, lenBytes.Length);
                Array.Copy(msgBytes, 0, payload, lenBytes.Length, msgBytes.Length);
                int totalBits = payload.Length * 8;

                // 6. 容量檢查
                int capacity = histR[_peakR] + histG[_peakG] + histB[_peakB] + histGray[_peakGray];
                if (capacity < totalBits)
                {
                    return new StegoResult
                    {
                        Success = false,
                        ErrorMessage = $"容量不足: 需要 {totalBits} 位元，僅有 {capacity} 位元可用。"
                    };
                }

                // 7. 創建結果圖像
                var resultBmp = new Bitmap(coverImage);

                // 8. 直方圖位移 (peak→zero 區間)
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // 處理 RGB 通道
                        ShiftChannel(rCh, y, x, _peakR, _zeroR);
                        ShiftChannel(gCh, y, x, _peakG, _zeroG);
                        ShiftChannel(bCh, y, x, _peakB, _zeroB);

                        // 處理灰階通道 (灰階位移需要同步更新 RGB)
                        ShiftGrayChannel(grayCh, rCh, gCh, bCh, y, x, _peakGray, _zeroGray);
                    }
                }
                progress?.Report(30);
                LogManager.Instance.Log("直方圖位移完成，進度：30%");

                // 9. 嵌入 bits
                int bitIndex = 0;
                for (int y = 0; y < height && bitIndex < totalBits; y++)
                {
                    for (int x = 0; x < width && bitIndex < totalBits; x++)
                    {
                        // RGB 通道嵌入
                        WriteBit(rCh, payload, ref bitIndex, y, x, _peakR);
                        if (bitIndex >= totalBits) break;

                        WriteBit(gCh, payload, ref bitIndex, y, x, _peakG);
                        if (bitIndex >= totalBits) break;

                        WriteBit(bCh, payload, ref bitIndex, y, x, _peakB);
                        if (bitIndex >= totalBits) break;

                        // 灰階通道嵌入 (需同步更新 RGB)
                        WriteGrayBit(grayCh, rCh, gCh, bCh, payload, ref bitIndex, y, x, _peakGray);
                    }

                    // 更新進度
                    if (y % 10 == 0 || bitIndex >= totalBits)
                    {
                        int progressPercent = 30 + (int)(((double)bitIndex / totalBits) * 60);
                        progress?.Report(progressPercent);
                    }
                }

                // 10. 寫回 Bitmap
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var orig = resultBmp.GetPixel(x, y);
                        int nr = Clamp(rCh[y, x]);
                        int ng = Clamp(gCh[y, x]);
                        int nb = Clamp(bCh[y, x]);
                        resultBmp.SetPixel(x, y, Color.FromArgb(orig.A, nr, ng, nb));
                    }
                }
                LogManager.Instance.Log("Bitmap寫回成功，進度：90%");
                progress?.Report(90);

                // 11. 外部LSB
                MetadataHelper.EmbedMetadataLSB(resultBmp, metadata);
                
                progress?.Report(100);
                LogManager.Instance.Log($"藏密完成，總共嵌入 {metadata.Length} bytes 的元數據和 {totalBits} bits 的訊息。");
                return new StegoResult { Success = true, Image = resultBmp };
            }
            catch (Exception ex)
            {
                LogManager.Instance.Log($"嵌入過程中發生錯誤: {ex.Message}");
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public StegoResult Extract(Bitmap stegoImage, IProgress<int> progress = null)
        {
            try
            {
                LogManager.Instance.Log("開始提取…");
                int w = stegoImage.Width;
                int h = stegoImage.Height;

                // 1. 從 LSB 提取 metadata (8 bytes)
                byte[] metadata = MetadataHelper.ExtractMetadataLSB(stegoImage, METADATA_SIZE);

                // 2. 解析 metadata
                _peakR = metadata[0]; _zeroR = metadata[1];
                _peakG = metadata[2]; _zeroG = metadata[3];
                _peakB = metadata[4]; _zeroB = metadata[5];
                _peakGray = metadata[6]; _zeroGray = metadata[7];

                LogManager.Instance.Log($"已提取 metadata - Peak/Zero: R:{_peakR}/{_zeroR}, G:{_peakG}/{_zeroG}, B:{_peakB}/{_zeroB}, Gray:{_peakGray}/{_zeroGray}");

                // 3. 驗證元數據
                if (_peakR < 0 || _peakR > 255 || _zeroR < 0 || _zeroR > 255 ||
                    _peakG < 0 || _peakG > 255 || _zeroG < 0 || _zeroG > 255 ||
                    _peakB < 0 || _peakB > 255 || _zeroB < 0 || _zeroB > 255 ||
                    _peakGray < 0 || _peakGray > 255 || _zeroGray < 0 || _zeroGray > 255)
                {
                    throw new Exception("提取的元數據無效，peak 或 zero 值超出有效範圍 (0-255)");
                }

                progress?.Report(20);
                LogManager.Instance.Log("提取元數據完成，進度：20%");

                // 4. 建立通道矩陣
                int[,] rCh = new int[h, w];
                int[,] gCh = new int[h, w];
                int[,] bCh = new int[h, w];
                int[,] grayCh = new int[h, w];

                // 5. 讀取圖像數據
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        var p = stegoImage.GetPixel(x, y);
                        rCh[y, x] = p.R;
                        gCh[y, x] = p.G;
                        bCh[y, x] = p.B;
                        grayCh[y, x] = (p.R + p.G + p.B) / 3;
                    }
                }

                // 6. 提取長度資訊 (4 bytes)
                byte[] lenBytes = new byte[4];
                int bitIndex = 0;

                try
                {
                    ExtractBits(rCh, gCh, bCh, grayCh, 4 * 8, lenBytes);
                    int msgLen = BitConverter.ToInt32(lenBytes, 0);

                    LogManager.Instance.Log($"提取的訊息長度: {msgLen} bytes");

                    // 合理性檢查
                    if (msgLen < 0 || msgLen > 1 * 1024 * 1024) // 限制為最大1MB
                    {
                        throw new Exception($"提取的訊息長度無效: {msgLen} bytes");
                    }

                    progress?.Report(40);
                    LogManager.Instance.Log("提取長度資訊完成，進度：40%");

                    // 7. 提取實際訊息
                    byte[] msgBytes = new byte[msgLen];
                    try
                    {
                        ExtractBits(rCh, gCh, bCh, grayCh, msgLen * 8, msgBytes, 4 * 8);

                        progress?.Report(80);
                        LogManager.Instance.Log("提取訊息完成，進度：80%");

                        // 8. 復原直方圖
                        Bitmap recoveredImage = new Bitmap(stegoImage);
                        for (int y = 0; y < h; y++)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                // 復原 RGB 通道
                                RecoverChannel(rCh, y, x, _peakR, _zeroR);
                                RecoverChannel(gCh, y, x, _peakG, _zeroG);
                                RecoverChannel(bCh, y, x, _peakB, _zeroB);

                                // 復原灰階通道 (同步更新 RGB)
                                RecoverGrayChannel(grayCh, rCh, gCh, bCh, y, x, _peakGray, _zeroGray);

                                // 更新像素
                                var orig = recoveredImage.GetPixel(x, y);
                                recoveredImage.SetPixel(x, y, Color.FromArgb(orig.A,
                                    Clamp(rCh[y, x]), Clamp(gCh[y, x]), Clamp(bCh[y, x])));
                            }
                        }

                        // 完成處理
                        progress?.Report(100);
                        LogManager.Instance.Log("進度：100%");

                        string msg = Encoding.UTF8.GetString(msgBytes);
                        LogManager.Instance.Log($"提取完成，總共提取 {msg.Length} bytes 的訊息。");
                        return new StegoResult { Success = true, Message = msg, Image = recoveredImage };
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Log($"提取訊息失敗: {ex.Message}");
                        throw new Exception($"提取訊息失敗: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Log($"提取訊息長度失敗: {ex.Message}");
                    throw new Exception($"提取訊息長度失敗: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Log($"提取過程中發生錯誤: {ex.Message}");
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        // Enhanced Helper Methods

        /// <summary>
        /// 檢查像素值是否等於峰值或峰值+1
        /// </summary>
        private bool CheckPeak(int value, int peak)
        {
            return value == peak || value == peak + 1;
        }

        /// <summary>
        /// 在緩衝區的指定位置設置一個位元
        /// </summary>
        private void SetBit(byte[] buffer, int bitPosition, int bitValue)
        {
            int byteIndex = bitPosition / 8;
            int bitIndex = 7 - (bitPosition % 8);

            if (byteIndex < buffer.Length)
            {
                if (bitValue == 1)
                {
                    buffer[byteIndex] |= (byte)(1 << bitIndex);
                }
                else
                {
                    buffer[byteIndex] &= (byte)~(1 << bitIndex);
                }
            }
        }

        /// <summary>
        /// 找出直方圖中頻率最高的值
        /// </summary>
        private int ArgMax(int[] arr)
        {
            int idx = 0;
            for (int i = 1; i < arr.Length; i++)
                if (arr[i] > arr[idx]) idx = i;
            return idx;
        }

        /// <summary>
        /// 將值限制在 0 到 255 之間
        /// </summary>
        private int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

        /// <summary>
        /// 嘗試找到零點，若找不到則返回最小頻率點
        /// </summary>
        private int TryFindZeroPoint(int[] hist, int peak)
        {
            try { return FindZeroPoint(hist, peak); }
            catch { return FindMinPoint(hist, peak); }
        }

        /// <summary>
        /// 找到直方圖中的零點
        /// </summary>
        private int FindZeroPoint(int[] hist, int peak)
        {
            for (int d = 1; d < hist.Length; d++)
            {
                int up = peak + d;
                int dn = peak - d;
                if (up < hist.Length && hist[up] == 0) return up;
                if (dn >= 0 && hist[dn] == 0) return dn;
            }
            throw new InvalidOperationException("無法找到可用的零點");
        }

        /// <summary>
        /// 找到直方圖中頻率最小的點
        /// </summary>
        private int FindMinPoint(int[] hist, int peak)
        {
            int idx = -1;
            int min = int.MaxValue;
            for (int i = 0; i < hist.Length; i++)
            {
                if (i == peak) continue;
                if (hist[i] < min)
                {
                    min = hist[i];
                    idx = i;
                }
            }
            return idx >= 0 ? idx : peak;
        }

        /// <summary>
        /// 將通道值進行位移
        /// </summary>
        private void ShiftChannel(int[,] ch, int y, int x, int peak, int zero)
        {
            int v = ch[y, x];
            if (zero > peak && v > peak && v < zero) ch[y, x]++;
            else if (zero < peak && v < peak && v > zero) ch[y, x]--;
        }

        /// <summary>
        /// 復原通道值
        /// </summary>
        private void RecoverChannel(int[,] ch, int y, int x, int peak, int zero)
        {
            int v = ch[y, x];
            if (zero > peak && v > peak + 1 && v <= zero) ch[y, x]--;
            else if (zero < peak && v < peak - 1 && v >= zero) ch[y, x]++;
        }

        /// <summary>
        /// 從四個通道提取位元
        /// </summary>
        private void ExtractBits(int[,] rCh, int[,] gCh, int[,] bCh, int[,] grayCh,
                                int bitCount, byte[] buffer, int startBit = 0)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // 計算所需字節數
            int requiredBytes = (int)Math.Ceiling((startBit + bitCount) / 8.0)-4;

            // 檢查緩衝區長度
            if (buffer.Length < requiredBytes)
                throw new ArgumentException($"緩衝區長度不足，需要 {requiredBytes} bytes，但只有 {buffer.Length} bytes");

            // 初始化緩衝區
            Array.Clear(buffer, 0, buffer.Length);

            int h = rCh.GetLength(0), w = rCh.GetLength(1);
            int extractedBits = 0;

            // 掃描圖像並提取數據
            for (int y = 0; y < h && extractedBits < bitCount; y++)
            {
                for (int x = 0; x < w && extractedBits < bitCount; x++)
                {
                    // 處理 R 通道
                    if (extractedBits < bitCount)
                    {
                        if (CheckPeak(rCh[y, x], _peakR))
                        {
                            int bit = (rCh[y, x] == _peakR + 1) ? 1 : 0;
                            SetBit(buffer, startBit + extractedBits, bit);
                            extractedBits++;
                        }
                    }

                    // 處理 G 通道
                    if (extractedBits < bitCount)
                    {
                        if (CheckPeak(gCh[y, x], _peakG))
                        {
                            int bit = (gCh[y, x] == _peakG + 1) ? 1 : 0;
                            SetBit(buffer, startBit + extractedBits, bit);
                            extractedBits++;
                        }
                    }

                    // 處理 B 通道
                    if (extractedBits < bitCount)
                    {
                        if (CheckPeak(bCh[y, x], _peakB))
                        {
                            int bit = (bCh[y, x] == _peakB + 1) ? 1 : 0;
                            SetBit(buffer, startBit + extractedBits, bit);
                            extractedBits++;
                        }
                    }

                    // 處理灰階通道
                    if (extractedBits < bitCount)
                    {
                        if (CheckPeak(grayCh[y, x], _peakGray))
                        {
                            int bit = (grayCh[y, x] == _peakGray + 1) ? 1 : 0;
                            SetBit(buffer, startBit + extractedBits, bit);
                            extractedBits++;
                        }
                    }
                }
            }

            // 確認是否提取了足夠的位元
            if (extractedBits < bitCount)
            {
                throw new InvalidOperationException($"無法提取足夠的位元：需要 {bitCount} 位元，但只提取了 {extractedBits} 位元");
            }
        }
        /// <summary>
        /// 對灰階通道進行位移，並同步更新 RGB 值
        /// </summary>
        private void ShiftGrayChannel(int[,] grayCh, int[,] rCh, int[,] gCh, int[,] bCh,
                                     int y, int x, int peak, int zero)
        {
            int grayVal = grayCh[y, x];

            // 檢查是否需要位移
            if ((zero > peak && grayVal > peak && grayVal < zero) ||
                (zero < peak && grayVal < peak && grayVal > zero))
            {
                // 計算位移量
                int shift = (zero > peak) ? 1 : -1;

                // 更新灰階值
                grayCh[y, x] += shift;

                // 平均分配調整量到 RGB 三通道
                int rVal = rCh[y, x];
                int gVal = gCh[y, x];
                int bVal = bCh[y, x];

                // 根據各通道的值決定調整順序 (優先調整數值大的通道)
                int[] channels = new int[] { 0, 1, 2 }; // R, G, B
                int[] values = new int[] { rVal, gVal, bVal };

                // 簡單排序通道 (按值從大到小)
                for (int i = 0; i < 3; i++)
                {
                    for (int j = i + 1; j < 3; j++)
                    {
                        if (values[i] < values[j])
                        {
                            // 交換值
                            int tempVal = values[i];
                            values[i] = values[j];
                            values[j] = tempVal;
                            // 交換通道索引
                            int tempCh = channels[i];
                            channels[i] = channels[j];
                            channels[j] = tempCh;
                        }
                    }
                }

                // 根據排序後的通道順序進行調整
                for (int i = 0; i < 3; i++)
                {
                    switch (channels[i])
                    {
                        case 0: // R
                            if ((shift > 0 && rCh[y, x] < 255) || (shift < 0 && rCh[y, x] > 0))
                            {
                                rCh[y, x] += shift;
                                return;
                            }
                            break;
                        case 1: // G
                            if ((shift > 0 && gCh[y, x] < 255) || (shift < 0 && gCh[y, x] > 0))
                            {
                                gCh[y, x] += shift;
                                return;
                            }
                            break;
                        case 2: // B
                            if ((shift > 0 && bCh[y, x] < 255) || (shift < 0 && bCh[y, x] > 0))
                            {
                                bCh[y, x] += shift;
                                return;
                            }
                            break;
                    }
                }

                // 如果無法安全調整，記錄警告
                LogManager.Instance.Log($"警告：無法安全調整像素 ({x},{y}) 的灰度值");
            }
        }

        /// <summary>
        /// 在指定通道嵌入一個位元
        /// </summary>
        private void WriteBit(int[,] ch, byte[] payload, ref int bitIndex, int y, int x, int peak)
        {
            if (bitIndex >= payload.Length * 8) return;
            if (ch[y, x] == peak)
            {
                int bytePos = bitIndex / 8;
                int shift = 7 - (bitIndex % 8);
                int bit = (payload[bytePos] >> shift) & 1;
                ch[y, x] = peak + bit;
                bitIndex++;
            }
        }

        /// <summary>
        /// 在灰階通道寫入一個位元，並同步更新 RGB
        /// </summary>
        private void WriteGrayBit(int[,] grayCh, int[,] rCh, int[,] gCh, int[,] bCh,
                                 byte[] payload, ref int bitIndex, int y, int x, int peak)
        {
            if (bitIndex >= payload.Length * 8) return;
            if (grayCh[y, x] == peak)
            {
                int bytePos = bitIndex / 8;
                int shift = 7 - (bitIndex % 8);
                int bit = (payload[bytePos] >> shift) & 1;

                if (bit == 1)
                {
                    grayCh[y, x] = peak + 1;

                    // 平均分配增量到 RGB 通道
                    if (rCh[y, x] < 255)
                    {
                        rCh[y, x]++;
                    }
                    else if (gCh[y, x] < 255)
                    {
                        gCh[y, x]++;
                    }
                    else if (bCh[y, x] < 255)
                    {
                        bCh[y, x]++;
                    }
                    else
                    {
                        LogManager.Instance.Log($"警告：無法在像素 ({x},{y}) 寫入灰度位元 1，所有通道都達到最大值");
                    }
                }

                bitIndex++;
            }
        }


        /// <summary>
        /// 復原灰階通道，並同步更新 RGB
        /// </summary>
        private void RecoverGrayChannel(int[,] grayCh, int[,] rCh, int[,] gCh, int[,] bCh,
                                       int y, int x, int peak, int zero)
        {
            int grayVal = grayCh[y, x];

            // 檢查是否需要復原
            if ((zero > peak && grayVal > peak + 1 && grayVal <= zero) ||
                (zero < peak && grayVal < peak - 1 && grayVal >= zero))
            {
                int shift = (zero > peak) ? -1 : 1;

                // 更新灰階值
                grayCh[y, x] += shift;

                // 均勻分配調整量到 RGB 三通道
                if ((shift < 0 && rCh[y, x] > 0) || (shift > 0 && rCh[y, x] < 255))
                {
                    rCh[y, x] += shift;
                }
                else if ((shift < 0 && gCh[y, x] > 0) || (shift > 0 && gCh[y, x] < 255))
                {
                    gCh[y, x] += shift;
                }
                else if ((shift < 0 && bCh[y, x] > 0) || (shift > 0 && bCh[y, x] < 255))
                {
                    bCh[y, x] += shift;
                }
                else
                {
                    LogManager.Instance.Log($"警告：無法安全復原像素 ({x},{y}) 的灰度值");
                }
            }
        }


    }
}