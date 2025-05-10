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
            _coefIndex = coefIndex; // 1~63 建議
        }

        public StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress=null)
        {
            try
            {
                var payload = PreparePayload(message);
                int w = coverImage.Width, h = coverImage.Height;
                var bmp = new Bitmap(coverImage);

                // 將影像轉灰階
                var gray = ImageHelper.ToGrayscaleMatrix(bmp);
                int bitIdx = 0, totalBits = payload.Length * 8;

                // 逐 8x8 Block 處理
                for (int by = 0; by < h && bitIdx < totalBits; by += 8)
                {
                    for (int bx = 0; bx < w && bitIdx < totalBits; bx += 8)
                    {
                        double[,] block = DctUtils.ForwardDct(gray, bx, by);
                        // 中頻係數嵌入 (省略直流與高頻)
                        int u = _coefIndex / 8, v = _coefIndex % 8;
                        int coef = (int)Math.Round(block[u, v]);

                        int payloadBit = (payload[bitIdx / 8] >> (7 - (bitIdx % 8))) & 1;
                        // 調整係數奇偶性
                        coef = (coef & 1) == payloadBit ? coef : coef ^ 1;
                        block[u, v] = coef;

                        // 反 DCT 寫回 gray
                        DctUtils.InverseDct(block, gray, bx, by);
                        bitIdx++;
                    }
                    // 更新進度
                    progress?.Report((int)((double)bitIdx / totalBits * 100));
                }

                // 將 gray 寫回 Bitmap
                ImageHelper.WriteGrayscaleMatrix(bmp, gray);

                Console.WriteLine($"嵌入完成，總共嵌入 {totalBits} bits 的訊息。");
                return new StegoResult { Success = true, Image = bmp };
            }
            catch (Exception ex)
            {
                return new StegoResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public StegoResult Extract(Bitmap stegoImage, IProgress<int> progress=null)
        {
            try
            {
                var bmp = new Bitmap(stegoImage);
                var gray = ImageHelper.ToGrayscaleMatrix(bmp);
                byte[] header = new byte[4];
                // 讀取長度 header (同前)
                int msgLen = BitConverter.ToInt32(header, 0);
                var payload = new byte[msgLen];
                int bitIdx = 32;
                int totalBits = (4 + msgLen) * 8;

                for (int by = 0; by < bmp.Height && bitIdx < totalBits; by += 8)
                {
                    for (int bx = 0; bx < bmp.Width && bitIdx < totalBits; bx += 8)
                    {
                        var block = DctUtils.ForwardDct(gray, bx, by);
                        int u = _coefIndex / 8, v = _coefIndex % 8;
                        int coef = (int)Math.Round(block[u, v]);
                        int bit = coef & 1;
                        int byteIdx = bitIdx / 8;
                        payload[byteIdx] = (byte)((payload[byteIdx] << 1) | bit);
                        bitIdx++;
                    }
                    // 更新進度
                    progress?.Report((int)((double)bitIdx / totalBits * 100));
                }

                string msg = Encoding.UTF8.GetString(payload);
                Console.WriteLine($"提取完成，總共提取 {totalBits} bits 的訊息。");

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