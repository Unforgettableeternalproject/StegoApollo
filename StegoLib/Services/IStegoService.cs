using StegoLib.Models;
using System;
using System.Drawing;

namespace StegoLib.Services
{
    public interface IStegoService
    {
        StegoResult Embed(Bitmap coverImage, string message, IProgress<int> progress = null);
        StegoResult Extract(Bitmap stegoImage, IProgress<int> progress = null);
    }
}
