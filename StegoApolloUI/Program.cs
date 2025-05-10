using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using StegoApolloUI.Presenters;
using StegoApolloUI.Views;
using StegoLib.Services;

namespace StegoApolloUI
{
    internal static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. 建立實例
            IMainView view = new MainForm(maxMessageLength: 256);           // MainForm 實作了 IMainView

            var alg = "LSB 演算法"; // 或 "LSB", "QIM"

            IStegoService stego;
            switch (alg)
            {
                case "LSB 演算法":
                    stego = new LsbStegoService();
                    break;
                case "DCT 演算法":
                    stego = new DctStegoService(coefIndex: 10);
                    break;
                case "QIM 演算法":
                    stego = new QimStegoService(16);
                    break;
                default:
                    throw new NotSupportedException($"不支援的演算法：{alg}");
            }

            // IStegoService stego = new QimgStegoService(); // 也可以使用 QimG Stego Service

            // 2. 注入到 Presenter
            var presenter = new MainPresenter(view, stego);

            // 3. 啟動 UI（必須把 view 轉回 Form）
            Application.Run((Form)view);
        }
    }
}
