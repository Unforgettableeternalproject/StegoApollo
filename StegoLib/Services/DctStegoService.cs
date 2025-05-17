using System;
using System.Drawing;
using System.Text;
using StegoLib.Models;
using StegoLib.Utilities;

namespace StegoLib.Services
{
    /// <summary>
    /// DCT-QIM 頻域藏密：在 8x8 Block 的指定 DCT 係數上進行 QIM 量化嵌入
    /// </summary>
    public class DctStegoService : IStegoService
    {
        private readonly int _delta;    // 量化步階 Δ
        private readonly int _coefIndex; // 要操作的 DCT 係數索引 (1~63)

        public DctStegoService(int delta = 16, int coefIndex = 10)
        {
            if (delta <= 0) throw new ArgumentOutOfRangeException(nameof(delta));
            if (coefIndex < 1 || coefIndex > 63) throw new ArgumentOutOfRangeException(nameof(coefIndex));
            _delta = delta;
            _coefIndex = coefIndex;
            LogManager.Instance.LogDebug($"DctStegoService initialized with delta = {_delta}, coefIndex = {_coefIndex}");
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null)
        {
            try
            {
                if (coverImage == null)
                    return new StegoResult { Success = false, ErrorMessage = "輸入圖片為空。" };

                // 自動裁切圖片為8的倍數
                int cropWidth = coverImage.Width - (coverImage.Width % 8);
                int cropHeight = coverImage.Height - (coverImage.Height % 8);

                if (cropWidth < 8 || cropHeight < 8)
                    return new StegoResult { Success = false, ErrorMessage = "圖片尺寸過小，無法進行DCT嵌入。" };

                Bitmap croppedImage = coverImage;
                if (coverImage.Width != cropWidth || coverImage.Height != cropHeight)
                {
                    croppedImage = new Bitmap(cropWidth, cropHeight);
                    using (Graphics g = Graphics.FromImage(croppedImage))
                    {
                        g.DrawImage(coverImage, 0, 0, cropWidth, cropHeight);
                    }
                    LogManager.Instance.LogDebug($"圖片已自動裁切為 {cropWidth}x{cropHeight} 以符合8的倍數。");
                }

                LogManager.Instance.LogDebug("Embed method called.");
                LogManager.Instance.LogDebug($"CoverImage Size: {croppedImage.Width}x{croppedImage.Height}");
                LogManager.Instance.LogDebug($"Message Length: {message.Length}");

                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                byte[] lenBytes = BitConverter.GetBytes(msgBytes.Length);
                byte[] payload = new byte[lenBytes.Length + msgBytes.Length];
                Array.Copy(lenBytes, payload, lenBytes.Length);
                Array.Copy(msgBytes, 0, payload, lenBytes.Length, msgBytes.Length);
                int totalBits = payload.Length * 8;

                int width = croppedImage.Width;
                int height = croppedImage.Height;
                int blocksX = width / 8;
                int blocksY = height / 8;
                int maxBits = blocksX * blocksY; // 每個block只能嵌入1 bit

                if (totalBits > maxBits)
                {
                    return new StegoResult
                    {
                        Success = false,
                        ErrorMessage = $"訊息過長，最多可嵌入 {maxBits / 8} bytes，實際欲嵌入 {payload.Length} bytes。"
                    };
                }

                LogManager.Instance.LogDebug($"Payload Length: {payload.Length} bytes, Total Bits: {totalBits}");

                int u = _coefIndex / 8;
                int v = _coefIndex % 8;
                int bitIndex = 0;

                Bitmap stegoImage = new Bitmap(croppedImage);

                progress?.Report(0);

                int channel = 0; // 0:R, 1:G, 2:B

                double[,] mat = new double[height, width];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        Color p = stegoImage.GetPixel(x, y);
                        mat[y, x] = channel == 0 ? p.R : channel == 1 ? p.G : p.B;
                        if ((y * width + x) % 1000 == 0)
                            progress?.Report((int)Math.Round(((y * width + x) * 100.0 / (width * height)) * 0.4));
                    }

                LogManager.Instance.LogDebug("Starting embedding process...");

                for (int by = 0; by < blocksY && bitIndex < totalBits; by++)
                {
                    for (int bx = 0; bx < blocksX && bitIndex < totalBits; bx++)
                    {
                        double[,] block = DctUtils.ForwardDct(mat, bx * 8, by * 8);
                        double cRaw = block[u, v];

                        int payloadBit = (payload[bitIndex / 8] >> (7 - (bitIndex % 8))) & 1;

                        int q = (int)Math.Round(cRaw / _delta);
                        if ((q & 1) != payloadBit)
                            q = (q & ~1) | payloadBit;
                        double quantized = q * _delta;

                        block[u, v] = quantized;
                        DctUtils.InverseDct(block, mat, bx * 8, by * 8);

                        bitIndex++;

                        if (bitIndex % 100 == 0)
                        {
                            LogManager.Instance.LogDebug($"Embedding bit {bitIndex}/{totalBits}: {quantized}");
                            progress?.Report((int)Math.Round(((by * blocksX + bx) * 100.0 / (blocksX * blocksY)) * 0.4 + 40));
                        }
                    }
                }

                LogManager.Instance.LogDebug($"Embedding completed. Total embedded bits: {bitIndex}");

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        Color orig = stegoImage.GetPixel(x, y);
                        int val = (int)Math.Round(mat[y, x]);
                        val = Math.Min(255, Math.Max(0, val));
                        Color updated = channel == 0
                            ? Color.FromArgb(orig.A, val, orig.G, orig.B)
                            : channel == 1
                                ? Color.FromArgb(orig.A, orig.R, val, orig.B)
                                : Color.FromArgb(orig.A, orig.R, orig.G, val);
                        stegoImage.SetPixel(x, y, updated);
                        if ((y * width + x) % 1000 == 0)
                        {
                            progress?.Report((int)Math.Round(((y * width + x) * 100.0 / (width * height)) * 0.2 + 80));
                        }
                    }

                progress?.Report(100);
                LogManager.Instance.LogSuccess($"藏密完成，總共嵌入 {totalBits} bits 的訊息。");
                return new StegoResult { Success = true, Image = stegoImage };
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

                int width = stegoImage.Width;
                int height = stegoImage.Height;
                int blocksX = width / 8;
                int blocksY = height / 8;
                int u = _coefIndex / 8;
                int v = _coefIndex % 8;

                int channel = 0; // 和嵌入必須一致

                double[,] mat = new double[height, width];

                progress?.Report(0);

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        Color p = stegoImage.GetPixel(x, y);
                        mat[y, x] = channel == 0 ? p.R : channel == 1 ? p.G : p.B;
                        // 只在每 1000 個像素回報一次
                        if ((y * width + x) % 1000 == 0)
                            progress?.Report((int)Math.Round(((y * width + x) * 100.0 / (width * height)) * 0.4));
                    }

                LogManager.Instance.LogDebug("Starting extraction process...");

                // 先取出長度（32位元/4byte）
                byte[] lenBits = new byte[32];
                int totalBlocks = blocksX * blocksY;
                int blockIndex = 0;
                for (; blockIndex < 32 && blockIndex < totalBlocks; blockIndex++)
                {
                    int by = blockIndex / blocksX;
                    int bx = blockIndex % blocksX;
                    double[,] block = DctUtils.ForwardDct(mat, bx * 8, by * 8);
                    int q = (int)Math.Round(block[u, v] / _delta);
                    lenBits[blockIndex] = (byte)(q & 1);
                    // 僅每 10 個 bit 回報一次進度
                    if (blockIndex % 10 == 0)
                    {
                        LogManager.Instance.LogDebug($"Extracting bit {blockIndex}/32: {lenBits[blockIndex]}"); // Log the extracted bits
                        progress?.Report((int)Math.Round(((blockIndex) * 100.0 / totalBlocks) * 0.2 + 40));
                    }
                }

                byte[] lenBytes = new byte[4];
                for (int i = 0; i < 32; i++)
                {
                    int byteIndex = i / 8;
                    int bitPos = 7 - (i % 8);
                    lenBytes[byteIndex] |= (byte)(lenBits[i] << bitPos);
                }
                int msgLen = BitConverter.ToInt32(lenBytes, 0);

                LogManager.Instance.LogDebug($"Extracted message length: {msgLen}");

                if (msgLen <= 0 || msgLen > (width * height) / 64)
                {
                    LogManager.Instance.LogDebug($"Invalid message length: {msgLen}");
                    return new StegoResult { Success = false, ErrorMessage = $"解析到無效的訊息長度: {msgLen}" };
                }

                // 取出 message
                int totalBits = msgLen * 8;
                byte[] msgBits = new byte[totalBits];
                for (int i = 0; i < totalBits && blockIndex < totalBlocks; i++, blockIndex++)
                {
                    int by = blockIndex / blocksX;
                    int bx = blockIndex % blocksX;
                    double[,] block = DctUtils.ForwardDct(mat, bx * 8, by * 8);
                    int q = (int)Math.Round(block[u, v] / _delta);
                    msgBits[i] = (byte)(q & 1);
                    // 僅每 100 個 bit 回報一次進度
                    if (i % 100 == 0)
                    {
                        LogManager.Instance.LogDebug($"Extracting bit {i}/{totalBits}: {msgBits[i]}"); // Log the extracted bits
                        progress?.Report((int)Math.Round(((i) * 100.0 / totalBits) * 0.1 + 80));
                    }
                }

                byte[] msgBytes = new byte[msgLen];
                for (int i = 0; i < totalBits; i++)
                {
                    int byteIndex = i / 8;
                    int bitPos = 7 - (i % 8);
                    msgBytes[byteIndex] |= (byte)(msgBits[i] << bitPos);
                    // 僅每 100 個 bit 回報一次進度
                    if (i % 100 == 0)
                        progress?.Report((int)Math.Round(((i) * 100.0 / totalBits) * 0.1 + 90));
                }
                progress?.Report(100);

                string message = Encoding.UTF8.GetString(msgBytes);
                LogManager.Instance.LogDebug($"萃取完成，提取到的訊息: {message}");
                return new StegoResult { Success = true, Message = message };
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogDebug($"萃取失敗: {ex.Message}");
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
