using StegoLib.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StegoApolloUI.Forms
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            ShowInTaskbar = false;
            this.Load += LogForm_Load;
            rtxtbox_Logs.Enter += DisableFocus;
            timer_refreash.Tick += (s, _) =>
            {
                // 每 1 秒更新一次日誌顯示
                UpdateLogDisplay();
            };
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            rtxtbox_Logs.WordWrap = true;
            rtxtbox_Logs.ScrollBars = RichTextBoxScrollBars.Vertical;

            // 初始分隔線
            rtxtbox_Logs.Clear();
            rtxtbox_Logs.SelectionColor = Color.Blue;
            rtxtbox_Logs.AppendText("===== 日誌開始 =====\r\n");
            rtxtbox_Logs.SelectionColor = Color.Black;

            timer_refreash.Start();

            // 首次顯示
            UpdateLogDisplay();
        }
        private void btn_CopyLogs_Click(object sender, EventArgs e)
        {
            if (rtxtbox_Logs.Text.Length > 0)
            {
                Clipboard.SetText(rtxtbox_Logs.Text);
                MessageBox.Show("日誌已複製到剪貼簿！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("沒有日誌可供複製！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btn_ClearLogs_Click(object sender, EventArgs e)
        {
            if (rtxtbox_Logs.Text.Length > 0)
            {
                DialogResult result = MessageBox.Show("確定要清除所有日誌嗎？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // 清空日誌管理器中的記錄
                    LogManager.Instance.Clear();

                    // 清空 RichTextBox 的內容
                    rtxtbox_Logs.Clear();

                    // 添加初始分隔線
                    rtxtbox_Logs.SelectionColor = Color.Blue;
                    rtxtbox_Logs.AppendText("===== 日誌開始 =====\r\n");
                    rtxtbox_Logs.SelectionColor = Color.Black;

                    UpdateLogDisplay(); // 更新顯示
                }
            }
            else
            {
                MessageBox.Show("日誌已經是空的！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public void UpdateLogDisplay()
        {
            // 取得所有日誌
            var logs = LogManager.Instance.GetLogs(); // List<string> 或 IEnumerable<string>
            if (logs.Count == 0)
            {
                if (rtxtbox_Logs.TextLength == 1) // 如果日誌已經是空的，則不重複操作
                {
                    rtxtbox_Logs.SelectionColor = Color.Gray;
                    rtxtbox_Logs.AppendText("目前沒有任何日誌記錄...\r\n");
                    rtxtbox_Logs.SelectionColor = Color.Black;
                }
                return;
            }

            // 檢查是否有新日誌
            int currentLogCount = rtxtbox_Logs.Lines.Length - 1; // 減去分隔線
            if (currentLogCount >= logs.Count) return; // 沒有新日誌則不更新

            // 新增新日誌
            for (int i = currentLogCount; i < logs.Count; i++)
            {
                var line = logs[i];
                if (line.ToString().Contains("[錯誤]")) rtxtbox_Logs.SelectionColor = Color.Red;
                else if (line.ToString().Contains("[警告]")) rtxtbox_Logs.SelectionColor = Color.Orange;
                else if (line.ToString().Contains("[成功]")) rtxtbox_Logs.SelectionColor = Color.Green;
                else rtxtbox_Logs.SelectionColor = Color.Black;

                rtxtbox_Logs.AppendText(line + "\r\n");
            }

            // 滾動到最底部
            rtxtbox_Logs.SelectionStart = rtxtbox_Logs.Text.Length;
            rtxtbox_Logs.ScrollToCaret();
        }

        private void DisableFocus(object sender, EventArgs e)
        {
            btn_CopyLogs.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            timer_refreash.Stop();
            timer_refreash.Dispose();
        }
    }
}
