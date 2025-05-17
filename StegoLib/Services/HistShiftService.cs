// 單通道 Histogram Shifting 隱寫（Peak Pixel List 順序嵌入＆萃取，完全一致）
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using StegoLib.Models;
using StegoLib.Utilities;

namespace StegoLib.Services
{
    public class HistShiftStegoService_SingleChannel : IStegoService
    {
        private int _peakR;
        private int _zeroR;
        private const int METADATA_SIZE = 2;
        private const int PEAKLIST_HASH_SIZE = 4; // 4 bytes

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                int width = coverImage.Width;
                int height = coverImage.Height;
                LogManager.Instance.LogDebug($"開始嵌入，圖像大小: {width}x{height}");
                LogManager.Instance.LogDebug($"原始訊息長度: {message.Length}, 訊息內容: {message}");

                // 1. 建立 R 通道矩陣
                int[,] rCh = new int[height, width];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        rCh[y, x] = coverImage.GetPixel(x, y).R;

                // 2. 計算 R 通道直方圖並找 peak（避開邊界）
                var histR = HistogramHelper.ComputeChannelHistogram(coverImage, 'R');
                _peakR = ArgMax(histR);
                _zeroR = TryFindZeroPoint(histR, _peakR);
                if (_peakR <= 1 || _peakR >= 254 || _zeroR <= 1 || _zeroR >= 254)
                {
                    LogManager.Instance.LogError($"Peak/Zero 在邊界區間，嵌入失敗: Peak={_peakR}, Zero={_zeroR}");
                    return new StegoResult { Success = false, ErrorMessage = "Peak/Zero 不可為邊界值(0,1,254,255)" };
                }
                LogManager.Instance.LogDebug($"Peak/Zero - R:{_peakR}/{_zeroR}");

                // 3. 準備 metadata (2 bytes: peak, zero)
                byte[] metadata = new byte[] { (byte)_peakR, (byte)_zeroR };

                // 4. 準備 payload
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                byte[] lenBytes = BitConverter.GetBytes(msgBytes.Length);
                byte[] payload = new byte[lenBytes.Length + msgBytes.Length];
                Array.Copy(lenBytes, 0, payload, 0, lenBytes.Length);
                Array.Copy(msgBytes, 0, payload, lenBytes.Length, msgBytes.Length);
                int totalBits = payload.Length * 8;
                LogManager.Instance.LogDebug($"嵌入payload原始長度: {payload.Length} bytes, 須嵌入 bits: {totalBits}");
                LogManager.Instance.LogDebug($"payload前16位元內容(byte): " + BitConverter.ToString(payload, 0, Math.Min(16, payload.Length)));

                // 5. 先做直方圖位移
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        ShiftChannel(rCh, y, x, _peakR, _zeroR);
                progress?.Report(30);
                LogManager.Instance.LogDebug("直方圖位移完成，進度：30%");

                // 6. 僅收集「位移後 R==peak 或 R==peak+1」的座標list（固定掃描順序，先y後x）
                var peakList = new List<(int x, int y)>();
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        if (rCh[y, x] == _peakR || rCh[y, x] == _peakR + 1) peakList.Add((x, y));

                // log
                StringBuilder listLog = new StringBuilder();
                for (int i = 0; i < Math.Min(32, peakList.Count); i++)
                {
                    var (x, y) = peakList[i];
                    listLog.Append($"({x},{y}) ");
                }
                LogManager.Instance.LogDebug($"[PEAKLIST] 前32: {listLog.ToString()}");

                if (peakList.Count < totalBits)
                {
                    LogManager.Instance.LogError($"容量不足: 需要 {totalBits} 位元，僅有 {peakList.Count} 位元可用。");
                    return new StegoResult { Success = false, ErrorMessage = $"容量不足: 需要 {totalBits} 位元，僅有 {peakList.Count} 位元可用。" };
                }

                // 7. 嵌入 bits，只修改 peakList 裡的 pixel（這是唯一順序）
                for (int i = 0; i < totalBits; i++)
                {
                    var (x, y) = peakList[i];
                    int bytePos = i / 8;
                    int shift = 7 - (i % 8);
                    int bit = (payload[bytePos] >> shift) & 1;
                    rCh[y, x] = _peakR + bit;
                    if (i < 32) LogManager.Instance.LogDebug($"嵌入長度bit: idx={i} @({x},{y}) value={bit}");
                }

                // 8. 寫回 Bitmap
                var resultBmp = new Bitmap(coverImage);
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        var orig = resultBmp.GetPixel(x, y);
                        int nr = Clamp(rCh[y, x]);
                        resultBmp.SetPixel(x, y, Color.FromArgb(orig.A, nr, orig.G, orig.B));
                    }
                LogManager.Instance.Log("Bitmap寫回成功，進度：90%");
                progress?.Report(90);

                // 9. 計算 peakList hash 並寫進 metadata (後 4 bytes)
                byte[] peakListHash = ComputePeakListHash(peakList, 16); // 16 為保證 peakList 至少有 32
                LogManager.Instance.LogDebug($"PeakList hash: {BitConverter.ToString(peakListHash)}");
                byte[] allMeta = new byte[metadata.Length + peakListHash.Length];
                Array.Copy(metadata, 0, allMeta, 0, metadata.Length);
                Array.Copy(peakListHash, 0, allMeta, metadata.Length, peakListHash.Length);
                MetadataHelper.EmbedMetadataLSB(resultBmp, allMeta);

                progress?.Report(100);
                LogManager.Instance.LogSuccess($"藏密完成，嵌入 {allMeta.Length} bytes 元數據 + {totalBits} bits payload。");
                return new StegoResult { Success = true, Image = resultBmp };
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError($"嵌入過程中發生錯誤: {ex.Message}");
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

                // 1. metadata
                byte[] allMeta = MetadataHelper.ExtractMetadataLSB(stegoImage, METADATA_SIZE + PEAKLIST_HASH_SIZE);
                _peakR = allMeta[0]; _zeroR = allMeta[1];
                byte[] peakListHashRef = new byte[PEAKLIST_HASH_SIZE];
                Array.Copy(allMeta, METADATA_SIZE, peakListHashRef, 0, PEAKLIST_HASH_SIZE);
                LogManager.Instance.LogDebug($"已提取 metadata - Peak/Zero: R:{_peakR}/{_zeroR}");
                progress?.Report(20);

                // 2. R channel
                int[,] rCh = new int[h, w];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        rCh[y, x] = stegoImage.GetPixel(x, y).R;

                // 3. 生成 peakList（先y後x）
                var peakList = new List<(int x, int y)>();
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        if (rCh[y, x] == _peakR || rCh[y, x] == _peakR + 1)
                            peakList.Add((x, y));

                // log
                StringBuilder listLog = new StringBuilder();
                for (int i = 0; i < Math.Min(32, peakList.Count); i++)
                {
                    var (x, y) = peakList[i];
                    listLog.Append($"({x},{y}) ");
                }
                LogManager.Instance.LogDebug($"[PEAKLIST] 前32: {listLog.ToString()}");

                // 3-2. 驗證 peakList hash
                byte[] peakListHash = ComputePeakListHash(peakList, 16);
                LogManager.Instance.LogDebug($"PeakList hash (萃取): {BitConverter.ToString(peakListHash)}");
                
                if (!IsEqual(peakListHash, peakListHashRef))
                {
                    LogManager.Instance.LogError($"PeakList 不一致，萃取失敗");
                    return new StegoResult { Success = false, ErrorMessage = "PeakList 不一致，無法正確解碼。" };
                }
                // 只用和嵌入時同樣順序！

                // 4. 提取長度（只用 peakList 前 32bit/4byte）
                byte[] lenBytes = new byte[4];
                for (int i = 0; i < 32; i++)
                {
                    var (x, y) = peakList[i];
                    int v = rCh[y, x];
                    int bit = (v == _peakR + 1) ? 1 : 0;
                    int byteIdx = i / 8;
                    int bitIdx = 7 - (i % 8);
                    if (bit == 1) lenBytes[byteIdx] |= (byte)(1 << bitIdx);
                    if (i < 32) LogManager.Instance.LogDebug($"[Extract]長度bit: idx={i} @({x},{y}) value={bit}");
                }
                LogManager.Instance.LogDebug($"Extracted lenBytes: {BitConverter.ToString(lenBytes)}");
                int msgLen = BitConverter.ToInt32(lenBytes, 0);
                LogManager.Instance.LogDebug($"提取的訊息長度: {msgLen} bytes");
                if (msgLen < 0 || msgLen > 1024 * 1024)
                {
                    LogManager.Instance.LogError($"提取的訊息長度無效: {msgLen} bytes");
                    throw new Exception($"提取的訊息長度無效: {msgLen} bytes");
                }
                progress?.Report(40);

                // 5. 提取訊息
                int totalBits = msgLen * 8;
                byte[] msgBytes = new byte[msgLen];
                for (int i = 0; i < totalBits; i++)
                {
                    var (x, y) = peakList[32 + i];
                    int v = rCh[y, x];
                    int bit = (v == _peakR + 1) ? 1 : 0;
                    int byteIdx = i / 8;
                    int bitIdx = 7 - (i % 8);
                    if (bit == 1) msgBytes[byteIdx] |= (byte)(1 << bitIdx);
                }
                progress?.Report(80);
                string msgRaw = Encoding.UTF8.GetString(msgBytes);
                string msgTrimmed = msgRaw.TrimEnd('\0');
                LogManager.Instance.LogDebug($"msgBytes原始前32byte: {BitConverter.ToString(msgBytes, 0, Math.Min(32, msgBytes.Length))}");
                LogManager.Instance.LogDebug($"提取文字內容(原始): [{msgRaw}]");
                LogManager.Instance.LogDebug($"提取文字內容(去尾): [{msgTrimmed}]");

                // 6. 可選還原
                Bitmap recoveredImage = new Bitmap(stegoImage);
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        RecoverChannel(rCh, y, x, _peakR, _zeroR);
                        var orig = recoveredImage.GetPixel(x, y);
                        recoveredImage.SetPixel(x, y, Color.FromArgb(orig.A, Clamp(rCh[y, x]), orig.G, orig.B));
                    }
                progress?.Report(100);

                LogManager.Instance.LogSuccess("提取完成，進度：100%");
                return new StegoResult { Success = true, Message = msgTrimmed, Image = recoveredImage };
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError($"提取過程中發生錯誤: {ex.Message}");
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        // --------- 工具方法 -----------
        private int ArgMax(int[] arr)
        {
            int idx = 1;
            for (int i = 2; i < arr.Length - 1; i++)
                if (arr[i] > arr[idx]) idx = i;
            return idx;
        }
        private int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;
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
                if (up < hist.Length && hist[up] == 0 && up > 0 && up < 255) return up;
                if (dn >= 0 && hist[dn] == 0 && dn > 0 && dn < 255) return dn;
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

        private byte[] ComputePeakListHash(List<(int x, int y)> list, int takeN)
        {
            // 只取前 takeN 對座標進行 hash，可自行調整
            var sb = new StringBuilder();
            for (int i = 0; i < Math.Min(takeN, list.Count); i++)
                sb.Append($"{list[i].x},{list[i].y};");
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            // 只取前4 bytes
            byte[] out4 = new byte[4];
            Array.Copy(hash, 0, out4, 0, 4);
            return out4;
        }

        public static bool IsEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}
