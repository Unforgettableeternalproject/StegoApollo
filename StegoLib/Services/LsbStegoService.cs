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
        public StegoResult Embed(Bitmap cover, string message)
        {
            // 嵌入邏輯
            return new StegoResult();
        }
        public StegoResult Extract(Bitmap stegoImage)
        {
            // 提取邏輯
            return new StegoResult();
        }
    }
}
