using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StegoApolloUI.Views;
using StegoApolloUI.Forms;
using StegoApolloUI.Resources;
using StegoLib.Utilities;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace StegoApolloUI
{
    public partial class MainForm : Form, IMainView
    {
        private readonly int _maxMessageLength;
        private LogForm _logForm = null; // 日誌視窗
        private ExplanationForm _expForm = null; // 演算法說明視窗
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
            this.Move += (s, e) =>
            {
                if (_logForm != null && !_logForm.IsDisposed)
                    _logForm.Location = new Point(this.Right, this.Top);
                if (_expForm != null && !_expForm.IsDisposed)
                    _expForm.Location = new Point(this.Left - _expForm.Width, this.Top);
            };
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
        public event EventHandler<AlgorithmChangedEventArgs> AlgorithmChanged;

        private void InitAlgorithmSelector()
        {
            cBox_AlgoSelect.Items.AddRange(new string[] { "LSB 演算法", "QIM 演算法" }); // 放棄DCT
            cBox_AlgoSelect.SelectedIndex = 0; // 預設選擇第一個演算法
        }

        public void InitApp()
        {
            btn_Encrypt.Enabled = true;
            btn_Decrypt.Enabled = true;
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
            tprogressBar.Value = 0;
            lbl_ModeTitle.Text = "選擇一個模式來開始";
            
            LogManager.Instance.Clear();
            LogManager.Instance.LogInfo("應用程式已初始化!");
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
            LogManager.Instance.LogInfo("已切換至藏密模式!");
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
            LogManager.Instance.LogInfo("已切換至萃取模式!");
        }

        public virtual void ShowProgress(int percent)
        {
            // 顯示進度條
            if (currentMode == "Encrypt")
            {
                pBar_eProgress.Value = percent;

                if (percent == 0) return;
                if (percent % 25 == 0) LogManager.Instance.LogInfo($"進度：{percent}%");
                if (percent == 100) LogManager.Instance.LogSuccess("藏密完成!");

                lbl_eProcessText.Show();
                lbl_eProcessText.Text = percent == 100 ? "處理完成!" : $"處理中...{percent}%";
                lbl_eProcessText.ForeColor = percent == 100 ? Color.SeaGreen : Color.Goldenrod;
            }
            else if (currentMode == "Decrypt")
            {
                pBar_dProgress.Value = percent;

                if (percent == 0) return;
                if (percent % 25 == 0) LogManager.Instance.LogInfo($"進度：{percent}%");
                if (percent == 100) LogManager.Instance.LogSuccess("萃取完成!");

                lbl_dProcessText.Show();
                lbl_dProcessText.Text = percent == 100 ? "處理完成!" : $"處理中...{percent}%";
                lbl_dProcessText.ForeColor = percent == 100 ? Color.SeaGreen : Color.Goldenrod;
            }
            tprogressBar.Value = percent;
        }

        private void ShowLogForm()
        {
            if (_logForm == null || _logForm.IsDisposed)
            {
                btn_eLogDisplay.Text = "隱藏詳細流程";
                _logForm = new LogForm();
                _logForm.FormClosed += (s, args) => { _logForm = null; btn_eLogDisplay.Text = "顯示詳細流程"; };

                // 1. 手動定位
                _logForm.StartPosition = FormStartPosition.Manual;
                // 2. 位置：X 座標貼齊主窗右側，Y 座標與主窗頂部對齊
                _logForm.Location = new Point(this.Right, this.Top);
                // 3. 高度同步
                _logForm.Height = this.Height;
                // （可選）寬度你也可以固定或動態設定
                //_logForm.Width = 300;

                _logForm.Show(this);  // 傳入 this，確保它不會跑到前面擋住主視窗
            }
            else
            {
                btn_eLogDisplay.Text = "顯示詳細流程";
                if (_logForm != null && !_logForm.IsDisposed)
                {
                    _logForm.Close();
                    _logForm = null;
                    return;
                }
            }
        }

        private void ShowExplanationForm()
        {
            if (_expForm == null || _expForm.IsDisposed)
            {
                _expForm = new ExplanationForm();
                _expForm.FormClosed += (s, args) => { _expForm = null; };

                _expForm.StartPosition = FormStartPosition.Manual;
                // 貼齊主窗左側、同頂部同高度
                _expForm.Location = new Point(this.Left - 300, this.Top); // 300 可改成你想要的寬度
                _expForm.Height = this.Height;
                _expForm.Width = 300; // 固定寬度
                UpdateAlgorithmDescription(); // 更新演算法說明
                _expForm.Show(this);
            }
            else
            {
                if (_expForm != null && !_expForm.IsDisposed)
                {
                    _expForm.Close();
                    _expForm = null;
                    return;
                }
            }
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
                LogManager.Instance.LogInfo($"顯示圖片：{bmp.Width}x{bmp.Height}");
                if (IsProcessed) processedImage = bmp;
            }else if(currentMode == "Decrypt")
            {
                dImageDisplay.Image = bmp;
                dImageDisplay.BackgroundImage = Properties.Resources.Background;
                LogManager.Instance.LogInfo($"顯示圖片：{bmp.Width}x{bmp.Height}");
            }
        }

        public virtual void ShowError(string message)
        {
            // 顯示錯誤訊息
            LogManager.Instance.LogError(message);
            MessageBox.Show(message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public virtual void ShowInfo(string message)
        {
            // 顯示資訊訊息
            LogManager.Instance.LogInfo(message);
            MessageBox.Show(message, "資訊", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public virtual void ShowExtracted(string message)
        {
            // 顯示萃取的訊息
            if(string.IsNullOrWhiteSpace(message))
            {
                LogManager.Instance.LogWarning("沒有可提取的內容!");
                rtxtbox_dDecryptText.Text = "沒有可提取的內容!";
                return;
            }
            LogManager.Instance.LogSuccess("萃取程序結束!");
            rtxtbox_dDecryptText.Text = message;
            _isTextboxDefault = false;
        }

        private void UpdateAlgorithmDescription()
        {
            string alg = cBox_AlgoSelect.SelectedItem?.ToString() ?? "";
            string title, content;
            Color color;

            switch (alg)
            {
                case "LSB 演算法":
                    title = ExplanationContents.LsbTitle;
                    content = ExplanationContents.LsbContent;
                    color = Color.MediumBlue;
                    break;
                case "QIM 演算法":
                    title = ExplanationContents.QimTitle;
                    content = ExplanationContents.QimContent;
                    color = Color.Green;
                    break;
                case "DCT 演算法": // 這個是 DCT-QIM 的說明，但我放棄這東西了
                    title = ExplanationContents.DctTitle;
                    content = ExplanationContents.DctContent;
                    color = Color.DarkCyan;
                    break;
                default:
                    // 如果沒選或是未知，隱藏窗體就好了
                    title = "";
                    content = "";
                    color = Color.Black;
                    break;
            }

            // 如果窗體存在，就更新；否則記錄在 _pendingTitle/_pendingContent 供開啟時用
            if (_expForm != null && !_expForm.IsDisposed)
            {
                _expForm.SetDescription(title, content, color);
            }
        }

        #region Components
        private void btn_Logo_Click(object sender, EventArgs e)
        {
            // Lead to my github
            string url = "https://github.com/Unforgettableeternalproject";
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError($"無法打開網址：{ex.Message}");
                MessageBox.Show(":(", "可惜了...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btn_AlgoExplain_Click(object sender, EventArgs e)
        {
            ShowExplanationForm();
        }

        private void menu_reset_Click(object sender, EventArgs e)
        {
            // Reset Everything
            DialogResult f = MessageBox.Show("重置會棄置目前所有的進度，並且回到應用程式的初始樣子，確認要繼續嗎?", "完全重置", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (f == DialogResult.No)
            {
                return;
            }

            EncryptInit(); // 順便把兩個模式下的元件也一起重置
            DecryptInit();
            InitApp();
            cBox_AlgoSelect.SelectedIndex = 0;
            LogManager.Instance.LogInfo("使用者進行完全重置。");
        }
        private void cBox_AlgoSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            var prev = cBox_AlgoSelect.SelectedItem?.ToString() ?? "";

            if (InputFilePath != "" || MessageText != "")
            {
                DialogResult f = MessageBox.Show("變更演算法會被迫拋棄目前的進度，確認嗎?", "變更演算法", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (f == DialogResult.No)
                {
                    cBox_AlgoSelect.SelectedIndex = prev == "LSB" ? 0 : 1; // 恢復到之前的選擇
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
            UpdateAlgorithmDescription();
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
            else
            {
                LogManager.Instance.LogWarning("使用者取消了檔案選擇。");
                return;
            }

            LogManager.Instance.LogInfo($"選擇的圖片路徑：{InputFilePath}");
            LogManager.Instance.LogInfo($"圖片尺寸: {new Bitmap(fileDialog.FileName).Width}x{new Bitmap(fileDialog.FileName).Height}");
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
            LogManager.Instance.LogInfo("使用範例文字!");
        }

        private void btn_eStartAction_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("確定要開始藏密嗎？", "開始藏密動作", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (d == DialogResult.No)
            {
                LogManager.Instance.LogInfo("使用者取消了藏密動作。");
                return;
            }

            if (string.IsNullOrWhiteSpace(rtxtbox_eEncryptText.Text) || _isTextboxDefault)
            {
                LogManager.Instance.LogError("使用者沒有提供任何藏密用的文字。");
                MessageBox.Show("你並沒有提供任何藏密用的文字!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LogManager.Instance.LogInfo("開始藏密動作...");
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
                    LogManager.Instance.LogSuccess($"儲存藏密後的圖片到：{filePath}");
                    processedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                    MessageBox.Show("圖片已成功儲存!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (IOException ioEx)
                {
                    LogManager.Instance.LogError(ioEx.Message);
                    MessageBox.Show($"文件可能被鎖定或無法訪問：{ioEx.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (ExternalException ex)
                {
                    LogManager.Instance.LogError(ex.Message);
                    MessageBox.Show($"保存圖像時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            LogManager.Instance.LogInfo("使用者進行重置。");
            EncryptInit();
        }
        private void btn_eLogDisplay_Click(object sender, EventArgs e)
        {
            ShowLogForm();
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
            else
            {
                LogManager.Instance.LogWarning("使用者取消了檔案選擇。");
                return;
            }

            LogManager.Instance.LogInfo($"選擇的圖片路徑：{InputFilePath}");
            LogManager.Instance.LogInfo($"圖片尺寸: {new Bitmap(fileDialog.FileName).Width}x{new Bitmap(fileDialog.FileName).Height}");
        }

        private void btn_dStartAction_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("確定要開始萃取嗎？", "開始萃取動作", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (d == DialogResult.No)
            {
                LogManager.Instance.LogInfo("使用者取消了萃取動作。");
                return;
            }

            LogManager.Instance.LogInfo("開始萃取動作...");
            ExtractRequested?.Invoke(this, EventArgs.Empty);
        }
        private void btn_dExport_Click(object sender, EventArgs e)
        {
            // 實際上就只是複製rbox_dDecryptText的內容到剪貼簿
            if (_isTextboxDefault)
            {
                LogManager.Instance.LogError("沒有可複製的萃取文字。");
                MessageBox.Show("沒有任何可複製的內容!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Clipboard.SetText(rtxtbox_dDecryptText.Text);
            LogManager.Instance.LogSuccess("已成功複製到剪貼簿!");
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
            LogManager.Instance.LogInfo("使用者進行重置。");
            DecryptInit();
        }
        private void btn_dLogDisplay_Click(object sender, EventArgs e)
        {
            ShowLogForm();
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
            if (MessageBox.Show("確定要關閉應用程式嗎？ (Bernie 會想念你的)", "關閉應用程式", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
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
            LogManager.Instance.LogInfo("正在生成直方圖...");
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
            LogManager.Instance.LogSuccess("直方圖生成完成!");
        }
        #endregion
    }

}
