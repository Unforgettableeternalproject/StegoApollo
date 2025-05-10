using StegoLib.Models;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoLib.Services
{
    public interface IStegoService
    {
        StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null);
        StegoResult Extract(Bitmap stegoImage, IProgress<int> progress = null);
    }
}
