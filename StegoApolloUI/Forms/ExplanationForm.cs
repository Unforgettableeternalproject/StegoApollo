using System.Drawing;
using System.Windows.Forms;

namespace StegoApolloUI.Forms
{
    public partial class ExplanationForm : Form
    {
        public ExplanationForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            ShowInTaskbar = false;
        }
        public void SetDescription(string title, string content, Color titleColor)
        {
            rtxtbox_Content.Clear();
            // 標題
            rtxtbox_Content.SelectionFont = new Font(rtxtbox_Content.Font, FontStyle.Bold);
            rtxtbox_Content.SelectionColor = titleColor;
            rtxtbox_Content.AppendText(title + "\r\n\r\n");
            // 內容
            rtxtbox_Content.SelectionFont = new Font(rtxtbox_Content.Font, FontStyle.Regular);
            rtxtbox_Content.SelectionColor = Color.Black;
            rtxtbox_Content.AppendText(content);

            rtxtbox_Content.SelectionStart = 0;
            rtxtbox_Content.ScrollToCaret();
        }
    }
}
