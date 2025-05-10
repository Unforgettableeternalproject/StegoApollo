using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoLib.Models
{
    public class StegoResult
    {
        public Bitmap Image { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
