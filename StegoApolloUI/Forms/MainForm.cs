using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StegoApolloUI.Views;
using StegoApolloUI.Presenters;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using StegoLib.Utilities;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace StegoApolloUI
{
    public partial class MainForm : Form, IMainView
    {
        private readonly int _maxMessageLength;
        private string currentMode = "None";
        private string _inputFilePath = "";
        private string _messageText = "";
        private bool _isTextboxDefault = true;
        private bool _isProcessed = false;
        private Bitmap processedImage = null;

        public MainForm(int maxMessageLength)
        {
            InitializeComponent();
            InitApp();
            InitAlgorithmSelector();
            this.MaximizeBox = false;
            _maxMessageLength = maxMessageLength;
            this.FormClosing += PreventClose;
            txtbox_eFilePath.Enter += DisableFocus;
            txtbox_dFilePath.Enter += DisableFocus;
            rtxtbox_eEncryptText.Enter += ClearDefault;
            rtxtbox_eEncryptText.Leave += RegainDefault;
        }

        public string InputFilePath { get { return _inputFilePath; } set { _inputFilePath = value;  } }
        public string MessageText
        {
            get { return _messageText; }
            set
            {
                if (value.Length > _maxMessageLength)
                {
                    MessageBox.Show($"訊息長度超過最大限制：{_maxMessageLength} 字元", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _messageText = value;
            }
        }
        public bool IsProcessed
        {
            get { return _isProcessed; }
            set
            {
                _isProcessed = value;
                if (value)
                {
                    btn_eStartAction.Enabled = false;
                    btn_eExport.Enabled = true;
                    btn_dStartAction.Enabled = false;
                    btn_eHistogram.Enabled = true;
                    MessageBox.Show(currentMode=="Encrypt" ? "圖片已成功處理!" : "已成功萃取文字!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    btn_eStartAction.Enabled = true;
                    btn_eExport.Enabled = false;
                    btn_dStartAction.Enabled = true;
                    btn_eHistogram.Enabled = false;
                }
            }
        }
        public event EventHandler EmbedRequested;
        public event EventHandler ExtractRequested;
        public event EventHandler ErrorRequested;
        public event EventHandler<AlgorithmChangedEventArgs> AlgorithmChanged;

        private void InitAlgorithmSelector()
        {
            cBox_AlgoSelect.Items.AddRange(new string[] { "LSB 演算法", "DCT 演算法", "QIM 演算法" });
            cBox_AlgoSelect.SelectedIndex = 0; // 預設選擇第一個演算法
        }

        public void InitApp()
        {
            panel_Functions.Show();
            panel_Default.Show();
            panel_Encrypt.Hide();
            panel_Decrypt.Hide();
            currentMode = "None";
            _isTextboxDefault = true;
            InputFilePath = "";
            MessageText = "";
            processedImage = null;
            IsProcessed = false;
            lbl_ModeTitle.Text = "選擇一個模式來開始";
        }

        public void EncryptInit()
        {
            // 初始化加密模式的 UI 元件
            btn_Encrypt.Enabled = false;
            btn_Decrypt.Enabled = true;
            panel_Default.Hide();
            panel_Encrypt.Show();
            panel_Decrypt.Hide();
            currentMode = "Encrypt";
            rtxtbox_eEncryptText.Text = "在這裡填入你的文字...";
            rtxtbox_eEncryptText.ForeColor = Color.DarkGray;
            _isTextboxDefault = true;
            processedImage = null;
            IsProcessed = false;
            InputFilePath = "";
            MessageText = "";
            txtbox_eFilePath.Text = "";
            panel_eActions.Enabled = false;
            panel_eProgressBar.Enabled = false;
            panel_eTextArea.Enabled = false;
            lbl_ModeTitle.Text = "藏密模式";
            lbl_eProcessText.Hide();
            ShowProgress(0);
            ShowImage(null);
        }

        public void DecryptInit()
        {
            // 初始化解密模式的 UI 元件
            btn_Decrypt.Enabled = false;
            btn_Encrypt.Enabled = true;
            panel_Default.Hide();
            panel_Encrypt.Hide();
            panel_Decrypt.Show();
            currentMode = "Decrypt";
            rtxtbox_dDecryptText.Text = "這裡會輸出萃取後的文字...";
            rtxtbox_dDecryptText.ForeColor = Color.DarkGray;
            _isTextboxDefault = true;
            processedImage = null;
            IsProcessed = false;
            InputFilePath = "";
            MessageText = "";
            txtbox_dFilePath.Text = "";
            panel_dActions.Enabled = false;
            panel_dProgressBar.Enabled = false;
            panel_dTextArea.Enabled = false;
            lbl_ModeTitle.Text = "萃取模式";
            lbl_dProcessText.Hide();
            ShowProgress(0);
            ShowImage(null);
        }

        public virtual void ShowProgress(int percent)
        {
            // 顯示進度條
            if (currentMode == "Encrypt")
            {
                pBar_eProgress.Value = percent;

                if (percent == 0) return;

                lbl_eProcessText.Show();
                lbl_eProcessText.Text = percent == 100 ? "處理完成!" : $"處理中...{percent}%";
                lbl_eProcessText.ForeColor = percent == 100 ? Color.SeaGreen : Color.Goldenrod;
            }
            else if (currentMode == "Decrypt")
            {
                pBar_dProgress.Value = percent;

                if (percent == 0) return;

                lbl_dProcessText.Show();
                lbl_dProcessText.Text = percent == 100 ? "處理完成!" : $"處理中...{percent}%";
                lbl_dProcessText.ForeColor = percent == 100 ? Color.SeaGreen : Color.Goldenrod;
            }
            tprogressBar.Value = percent;
        }

        public virtual void ShowImage(Bitmap bmp)
        {
            if (bmp == null) {
                // MessageBox.Show("無法顯示圖片", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                eImageDisplay.BackgroundImage = Properties.Resources.Default_Preview;
                dImageDisplay.BackgroundImage = Properties.Resources.Default_Preview;
                eImageDisplay.Image = null;
                dImageDisplay.Image = null;
                return;
            }
          
            // 顯示圖片
            if(currentMode == "Encrypt")
            {
                eImageDisplay.Image = bmp;
                eImageDisplay.BackgroundImage = Properties.Resources.Background;

                if (IsProcessed) processedImage = bmp;
            }else if(currentMode == "Decrypt")
            {
                dImageDisplay.Image = bmp;
                dImageDisplay.BackgroundImage = Properties.Resources.Background;
            }
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

        public virtual void ShowExtracted(string message)
        {
            // 顯示萃取的訊息
            if(string.IsNullOrWhiteSpace(message))
            {
                rtxtbox_dDecryptText.Text = "沒有可提取的內容!";
                return;
            }
            rtxtbox_dDecryptText.Text = message;
            _isTextboxDefault = false;
        }

        #region Components

        private void cBox_AlgoSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(InputFilePath != "" || MessageText != "")
            {
                DialogResult f = MessageBox.Show("變更演算法會被迫拋棄目前的進度，確認嗎?", "變更演算法", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (f == DialogResult.No)
                {
                    cBox_AlgoSelect.SelectedIndex = cBox_AlgoSelect.Items.IndexOf(currentMode);
                    return;
                }
            }

            if(currentMode == "Encrypt")
            {
                EncryptInit();
            }
            else if(currentMode == "Decrypt")
            {
                DecryptInit();
            }

            // 取得選擇的演算法
            string selectedAlgorithm = cBox_AlgoSelect.SelectedItem.ToString();
            AlgorithmChanged?.Invoke(this, new AlgorithmChangedEventArgs(selectedAlgorithm));
        }

        #region Encrypt
        private void btn_Encrypt_Click(object sender, EventArgs e)
        {
            if (InputFilePath != "" || MessageText != "")
            {
                DialogResult f = MessageBox.Show("變更模式會被迫拋棄目前的進度，確認嗎?", "變更模式", MessageBoxButtons.YesNo, MessageBoxIcon.Question); 
                if (f == DialogResult.No)
                {
                    return;
                }
            }
            EncryptInit();
        }
        private void btn_eBrowse_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
            fileDialog.Title = "選擇要嵌入的圖片";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtbox_eFilePath.Text = ShortenFileName(Path.GetFileName(fileDialog.FileName));
                InputFilePath = fileDialog.FileName;
                btn_eStartAction.Enabled = true;
                panel_eActions.Enabled = true;
                panel_eProgressBar.Enabled = true;
                panel_eTextArea.Enabled = true;
                IsProcessed = false;
                ShowImage(new Bitmap(fileDialog.FileName));
            }
        }
        private void rtxtbox_eEncryptText_TextChanged(object sender, EventArgs e)
        {
            string _default = "在這裡填入你的文字...";
            if (rtxtbox_eEncryptText.Text == _default)
            {
                DoNothing();
            }
            else
            {
                if (rtxtbox_eEncryptText.Text.Length >= _maxMessageLength)
                {
                    MessageBox.Show($"訊息長度超過最大限制：{_maxMessageLength} 字元，請縮短訊息!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rtxtbox_eEncryptText.Text = rtxtbox_eEncryptText.Text.Substring(0, _maxMessageLength);
                    rtxtbox_eEncryptText.SelectionStart = rtxtbox_eEncryptText.Text.Length;
                    rtxtbox_eEncryptText.SelectionLength = 0;
                }
                MessageText = rtxtbox_eEncryptText.Text;
                IsProcessed = false;
            }
        }
        private void btn_eExampleText_Click(object sender, EventArgs e)
        {
            string _exampleText = "這一段文字將被藏進圖片裡，你連絲毫都不會發現!";
            rtxtbox_eEncryptText.Text = _exampleText;
            rtxtbox_eEncryptText.ForeColor = Color.Black;
            MessageText = _exampleText;
            _isTextboxDefault = false;
        }

        private void btn_eStartAction_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("確定要開始藏密嗎？", "開始藏密動作", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (d == DialogResult.No)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(rtxtbox_eEncryptText.Text) || _isTextboxDefault)
            {
                MessageBox.Show("你並沒有提供任何藏密用的文字!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            EmbedRequested?.Invoke(this, EventArgs.Empty);
        }
        private void btn_eExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png";
            saveFileDialog.Title = "儲存藏密後的圖片";
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(InputFilePath) + "_stego.png";
            saveFileDialog.OverwritePrompt = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    // 如果文件已存在，先刪除
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    // 保存圖像
                    processedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (IOException ioEx)
                {
                    MessageBox.Show($"文件可能被鎖定或無法訪問：{ioEx.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (ExternalException ex)
                {
                    MessageBox.Show($"保存圖像時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                MessageBox.Show("圖片已成功儲存!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btn_eReset_Click(object sender, EventArgs e)
        {
            if (InputFilePath != "" || MessageText != "")
            {
                DialogResult f = MessageBox.Show("重置會棄置目前所有的進度，確定要重置嗎?", "重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (f == DialogResult.No)
                {
                    return;
                }
            }
            EncryptInit();
        }
        private void btn_eLogDisplay_Click(object sender, EventArgs e)
        {
            MessageBox.Show("目前尚未實作!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DoNothing(); // TODO: Implement log display
        }
        private void btn_eHistogram_Click(object sender, EventArgs e)
        {
            GenerateHistogram(processedImage);
        }
        #endregion

        #region Decrypt
        private void btn_Decrypt_Click(object sender, EventArgs e)
        {
            if (InputFilePath != "")
            {
                DialogResult f = MessageBox.Show("變更模式會被迫拋棄目前的進度，確認嗎?", "變更模式", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (f == DialogResult.No)
                {
                    return;
                }
            }
            DecryptInit();
        }
        private void btn_dBrowse_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
            fileDialog.Title = "選擇要萃取的圖片";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtbox_dFilePath.Text = ShortenFileName(Path.GetFileName(fileDialog.FileName));
                InputFilePath = fileDialog.FileName;
                btn_dStartAction.Enabled = true;
                panel_dActions.Enabled = true;
                panel_dProgressBar.Enabled = true;
                panel_dTextArea.Enabled = true;
                IsProcessed = false;
                ShowImage(new Bitmap(fileDialog.FileName));
            }
        }

        private void btn_dStartAction_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("確定要開始萃取嗎？", "開始萃取動作", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (d == DialogResult.No)
            {
                return;
            }

            ExtractRequested?.Invoke(this, EventArgs.Empty);
        }
        private void btn_dExport_Click(object sender, EventArgs e)
        {
            // 實際上就只是複製rbox_dDecryptText的內容到剪貼簿
            if (_isTextboxDefault)
            {
                MessageBox.Show("沒有任何可複製的內容!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Clipboard.SetText(rtxtbox_dDecryptText.Text);
            MessageBox.Show("已成功複製到剪貼簿!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_dReset_Click(object sender, EventArgs e)
        {
            if (InputFilePath != "" || !_isTextboxDefault)
            {
                DialogResult f = MessageBox.Show("重置會棄置目前所有的進度，確定要重置嗎?", "重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (f == DialogResult.No)
                {
                    return;
                }
            }
            DecryptInit();
        }
        private void btn_dLogDisplay_Click(object sender, EventArgs e)
        {
            MessageBox.Show("目前尚未實作!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DoNothing(); // TODO: Implement log display
        }

        #endregion

        #endregion

        #region Others
        private void DoNothing() { ; }

        private String ShortenFileName(string _fileName)
        {
            if (_fileName.Length > 16)
            {
                string _shortenedFileName = _fileName.Substring(0, 8) + "..." + _fileName.Substring(_fileName.Length - 7);
                return _shortenedFileName;
            }
            else
            {
                return _fileName;
            }
        }

        private void PreventClose(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("確定要關閉應用程式嗎？", "關閉應用程式", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void DisableFocus(object sender, EventArgs e)
        {
            if (currentMode == "Encrypt") btn_eBrowse.Focus();
            if (currentMode == "Decrypt") btn_dBrowse.Focus();
        }

        private void ClearDefault(object sender, EventArgs e)
        {
            string _default = "在這裡填入你的文字...";

            if (rtxtbox_eEncryptText.Text == _default)
            {
                rtxtbox_eEncryptText.Text = "";
                rtxtbox_eEncryptText.ForeColor = Color.Black;
                _isTextboxDefault = false;
            }
        }

        private void RegainDefault(object sender, EventArgs e)
        {
            string _default = "在這裡填入你的文字...";

            if (string.IsNullOrWhiteSpace(rtxtbox_eEncryptText.Text))
            {
                rtxtbox_eEncryptText.Text = _default;
                rtxtbox_eEncryptText.ForeColor = Color.DarkGray;
                _isTextboxDefault = true;
            }
        }

        private void GenerateHistogram(Bitmap image)
        {
            // 1. 前置檢查
            if (cBox_AlgoSelect.SelectedItem?.ToString() != "QIM 演算法")
            {
                MessageBox.Show("只有 QIM 演算法才支援直方圖顯示。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. 計算直方圖
            int[] hist = HistogramHelper.ComputeGrayHistogram(image);

            // 3. 產生直方圖 Bitmap
            int histW = 512, histH = 200;
            var histBmp = new Bitmap(histW, histH);
            using (Graphics g = Graphics.FromImage(histBmp))
            {
                g.Clear(Color.White);
                int maxCount = hist.Max();
                float barWidth = histW / 256f;
                for (int i = 0; i < 256; i++)
                {
                    float x = i * barWidth;
                    float h = maxCount > 0
                        ? (hist[i] / (float)maxCount) * histH
                        : 0;
                    RectangleF rect = new RectangleF(x, histH - h, barWidth, h);
                    g.FillRectangle(Brushes.DimGray, rect);
                }
            }

            // 4. 存成暫存檔
            string tempFile = Path.Combine(Path.GetTempPath(),
                                   $"qim_hist_{Guid.NewGuid():N}.png");
            histBmp.Save(tempFile, ImageFormat.Png);

            // 5. 用新 Form 顯示
            var f = new Form
            {
                Text = "QIM 直方圖",
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(histW, histH)
            };
            var pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = histBmp,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            f.Controls.Add(pic);
            f.FormClosed += (s, args) =>
            {
                pic.Image = null;
                histBmp.Dispose();
                try { File.Delete(tempFile); } catch { }
            };
            f.ShowDialog();
        }
        #endregion
    }

}
