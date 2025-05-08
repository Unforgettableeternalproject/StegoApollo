using StegoLib.Models;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoLib.Services
{
    internal interface IStegoService
    {
        StegoResult Embed(Bitmap coverImage, string message);
        StegoResult Extract(Bitmap stegoImage);
    }
}
