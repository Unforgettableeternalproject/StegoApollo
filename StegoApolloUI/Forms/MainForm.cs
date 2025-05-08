using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StegoApolloUI.Views;

namespace StegoApolloUI
{
    public partial class MainForm : Form, IMainView
    {
        private readonly int _maxMessageLength;
        public MainForm(int maxMessageLength)
        {
            InitializeComponent();
            _maxMessageLength = maxMessageLength;
        }

        public string InputFilePath => throw new NotImplementedException();
        public string MessageText => throw new NotImplementedException();
        public event EventHandler EmbedRequested;
        public event EventHandler ExtractRequested;
        public event EventHandler ErrorRequested;

        public virtual void ShowProgress(int percent)
        {
            // 更新進度條或其他 UI 元件
        }

        public virtual void ShowResultImage(Bitmap bmp)
        {
            // 顯示結果圖片
            // pictureBox.Image = bmp;
        }

        public virtual void ShowError(string message)
        {
            // 顯示錯誤訊息
            MessageBox.Show(message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public virtual void ShowInfo(string message)
        {
            // 顯示資訊訊息
            MessageBox.Show(message, "資訊", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
