// StegoLib/Services/HistShiftStegoService.cs
using System;
using System.Drawing;
using System.Text;
using StegoLib.Models;
using StegoLib.Utilities;

namespace StegoLib.Services
{
    /// <summary>
    /// 可逆直方圖位移 (Histogram Shifting) 隱寫
    /// 支援彩色三通道，並提供 Progress 回傳
    /// </summary>
    public class HistShiftStegoService : IStegoService
    {
        private int _peakR, _peakG, _peakB;
        private int _zeroR, _zeroG, _zeroB;

        public HistShiftStegoService() { }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                int width = coverImage.Width;
                int height = coverImage.Height;

                // 1. 建立三通道矩陣
                int[,] rCh = new int[height, width];
                int[,] gCh = new int[height, width];
                int[,] bCh = new int[height, width];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        var p = coverImage.GetPixel(x, y);
                        rCh[y, x] = p.R;
                        gCh[y, x] = p.G;
                        bCh[y, x] = p.B;
                    }

                // 2. 計算直方圖並找峰值
                var histR = HistogramHelper.ComputeChannelHistogram(coverImage, channel: 'R');
                var histG = HistogramHelper.ComputeChannelHistogram(coverImage, channel: 'G');
                var histB = HistogramHelper.ComputeChannelHistogram(coverImage, channel: 'B');
                _peakR = ArgMax(histR);
                _peakG = ArgMax(histG);
                _peakB = ArgMax(histB);

                // 3. 尋找 zero-point，有 zero 才用 zero，否則用最少頻率點
                _zeroR = TryFindZeroPoint(histR, _peakR);
                _zeroG = TryFindZeroPoint(histG, _peakG);
                _zeroB = TryFindZeroPoint(histB, _peakB);

                // 4. 準備 payload (6 bytes meta + 4 bytes length + data)
                var msgBytes = Encoding.UTF8.GetBytes(message);
                var lenBytes = BitConverter.GetBytes(msgBytes.Length);
                byte[] meta = new byte[] { (byte)_peakR, (byte)_zeroR,
                                           (byte)_peakG, (byte)_zeroG,
                                           (byte)_peakB, (byte)_zeroB };
                byte[] payload = new byte[meta.Length + lenBytes.Length + msgBytes.Length];
                Array.Copy(meta, 0, payload, 0, meta.Length);
                Array.Copy(lenBytes, 0, payload, meta.Length, lenBytes.Length);
                Array.Copy(msgBytes, 0, payload, meta.Length + lenBytes.Length, msgBytes.Length);
                int totalBits = payload.Length * 8;

                // 5. 容量檢查
                int capacity = histR[_peakR] + histG[_peakG] + histB[_peakB];
                if (capacity < totalBits)
                {
                    return new StegoResult
                    {
                        Success = false,
                        ErrorMessage = $"容量不足: 需要 {totalBits} 位元，僅有 {capacity} 位元可用。"
                    };
                }

                // 6. 直方圖位移 (peak→zero 區間)
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        ShiftChannel(rCh, y, x, _peakR, _zeroR);
                        ShiftChannel(gCh, y, x, _peakG, _zeroG);
                        ShiftChannel(bCh, y, x, _peakB, _zeroB);
                    }
                }


                // 7. 嵌入 bits
                int bitIndex = 0;
                for (int y = 0; y < height && bitIndex < totalBits; y++)
                {
                    for (int x = 0; x < width && bitIndex < totalBits; x++)
                    {
                        WriteBit(rCh, payload, ref bitIndex, y, x, _peakR);
                        WriteBit(gCh, payload, ref bitIndex, y, x, _peakG);
                        WriteBit(bCh, payload, ref bitIndex, y, x, _peakB);
                    }
                    progress?.Report((int)(((double)bitIndex / totalBits) * 100));
                }

                // 8. 寫回 Bitmap
                var resultBmp = new Bitmap(coverImage);
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        var orig = coverImage.GetPixel(x, y);
                        int nr = Clamp(rCh[y, x]);
                        int ng = Clamp(gCh[y, x]);
                        int nb = Clamp(bCh[y, x]);
                        resultBmp.SetPixel(x, y, Color.FromArgb(orig.A, nr, ng, nb));
                    }

                progress?.Report(100);
                LogManager.Instance.Log($"藏密完成，總共嵌入 {totalBits} bits 的訊息。");
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
                LogManager.Instance.Log("開始提取…");
                int w = stegoImage.Width;
                int h = stegoImage.Height;
                int[,] rCh = new int[h, w];
                int[,] gCh = new int[h, w];
                int[,] bCh = new int[h, w];

                // 讀取圖像數據
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var p = stegoImage.GetPixel(x, y);
                        rCh[y, x] = p.R;
                        gCh[y, x] = p.G;
                        bCh[y, x] = p.B;
                    }

                // 第一步：提取元數據 (6 bytes)
                byte[] meta = new byte[6];
                try
                {
                    ExtractBits(rCh, gCh, bCh, 6 * 8, meta);
                    _peakR = meta[0]; _zeroR = meta[1];
                    _peakG = meta[2]; _zeroG = meta[3];
                    _peakB = meta[4]; _zeroB = meta[5];

                    // 打印日誌，方便調試
                    LogManager.Instance.Log($"提取元數據: Peak/Zero - R:{_peakR}/{_zeroR}, G:{_peakG}/{_zeroG}, B:{_peakB}/{_zeroB}");

                    // 驗證元數據
                    if (_peakR < 0 || _peakR > 255 || _zeroR < 0 || _zeroR > 255 ||
                        _peakG < 0 || _peakG > 255 || _zeroG < 0 || _zeroG > 255 ||
                        _peakB < 0 || _peakB > 255 || _zeroB < 0 || _zeroB > 255)
                    {
                        throw new Exception("提取的元數據無效，peak 或 zero 值超出有效範圍 (0-255)");
                    }

                    progress?.Report(20);
                    LogManager.Instance.Log("進度：20%");
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Log($"提取元數據失敗: {ex.Message}");
                    throw new Exception($"提取元數據失敗: {ex.Message}");
                }

                // 第二步：提取長度資訊 (4 bytes)
                byte[] lenB = new byte[4];
                try
                {
                    // 確保準備足夠空間
                    if (lenB.Length < 4)
                    {
                        throw new Exception("長度緩衝區大小不足");
                    }

                    ExtractBits(rCh, gCh, bCh, 4 * 8, lenB, 6 * 8);
                    int msgLen = BitConverter.ToInt32(lenB, 0);

                    // 打印長度資訊進行調試
                    LogManager.Instance.Log($"提取的訊息長度: {msgLen} bytes");

                    // 合理性檢查
                    if (msgLen < 0 || msgLen > 1 * 1024 * 1024) // 限制為最大1MB
                    {
                        throw new Exception($"提取的訊息長度無效: {msgLen} bytes");
                    }

                    progress?.Report(40);
                    LogManager.Instance.Log("進度：40%");

                    // 第三步：提取實際訊息
                    byte[] msgB = new byte[msgLen];
                    try
                    {
                        ExtractBits(rCh, gCh, bCh, msgLen * 8, msgB, (6 + 4) * 8);

                        progress?.Report(80);
                        LogManager.Instance.Log("進度：80%");

                        // 第四步：復原直方圖
                        for (int y = 0; y < h; y++)
                            for (int x = 0; x < w; x++)
                            {
                                RecoverChannel(rCh, y, x, _peakR, _zeroR);
                                RecoverChannel(gCh, y, x, _peakG, _zeroG);
                                RecoverChannel(bCh, y, x, _peakB, _zeroB);
                            }

                        // 完成處理
                        progress?.Report(100);
                        LogManager.Instance.Log("進度：100%");

                        string msg = Encoding.UTF8.GetString(msgB);
                        LogManager.Instance.Log($"提取完成，總共提取 {msg.Length} bytes 的訊息。");
                        return new StegoResult { Success = true, Message = msg };
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

        // Helpers
        private int TryFindZeroPoint(int[] hist, int peak)
        {
            try { return FindZeroPoint(hist, peak); }
            catch { return FindMinPoint(hist, peak); }
        }

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

        private void ShiftChannel(int[,] ch, int y, int x, int peak, int zero)
        {
            int v = ch[y, x];
            if (zero > peak && v > peak && v < zero) ch[y, x]++;
            else if (zero < peak && v < peak && v > zero) ch[y, x]--;
        }

        private void RecoverChannel(int[,] ch, int y, int x, int peak, int zero)
        {
            int v = ch[y, x];
            if (zero > peak && v > peak + 1 && v <= zero) ch[y, x]--;
            else if (zero < peak && v < peak - 1 && v >= zero) ch[y, x]++;
        }

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

        private void ExtractBits(int[,] rCh, int[,] gCh, int[,] bCh,
                                 int bitCount, byte[] buffer, int startBit = 0)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // 修正計算所需字節數的方式
            int requiredBytes = (int)Math.Ceiling((startBit + bitCount) / 8.0);

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
                    // 處理R通道
                    if (extractedBits < bitCount)
                    {
                        if (CheckPeak(rCh[y, x], _peakR))
                        {
                            int bit = (rCh[y, x] == _peakR + 1) ? 1 : 0;
                            SetBit(buffer, startBit + extractedBits, bit);
                            extractedBits++;
                        }
                    }

                    // 處理G通道
                    if (extractedBits < bitCount)
                    {
                        if (CheckPeak(gCh[y, x], _peakG))
                        {
                            int bit = (gCh[y, x] == _peakG + 1) ? 1 : 0;
                            SetBit(buffer, startBit + extractedBits, bit);
                            extractedBits++;
                        }
                    }

                    // 處理B通道
                    if (extractedBits < bitCount)
                    {
                        if (CheckPeak(bCh[y, x], _peakB))
                        {
                            int bit = (bCh[y, x] == _peakB + 1) ? 1 : 0;
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

        // 檢查像素值是否等於峰值或峰值+1
        private bool CheckPeak(int value, int peak)
        {
            return value == peak || value == peak + 1;
        }

        // 在緩衝區的指定位置設置一個位元
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

        private int ArgMax(int[] arr)
        {
            int idx = 0;
            for (int i = 1; i < arr.Length; i++)
                if (arr[i] > arr[idx]) idx = i;
            return idx;
        }

        private int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;
    }
}