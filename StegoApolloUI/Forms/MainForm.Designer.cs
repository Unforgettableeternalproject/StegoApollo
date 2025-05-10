namespace StegoApolloUI
{
    partial class MainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menu = new System.Windows.Forms.MenuStrip();
            this.menu_file = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_open = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_save = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_help = new System.Windows.Forms.ToolStripMenuItem();
            this.status = new System.Windows.Forms.StatusStrip();
            this.tLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tprogressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.btn_eHistogram = new System.Windows.Forms.Button();
            this.panel_Encrypt = new System.Windows.Forms.Panel();
            this.panel_eActions = new System.Windows.Forms.Panel();
            this.btn_eLogDisplay = new System.Windows.Forms.Button();
            this.btn_eReset = new System.Windows.Forms.Button();
            this.panel_eProgressBar = new System.Windows.Forms.Panel();
            this.lbl_ePercentage100 = new System.Windows.Forms.Label();
            this.lbl_ePercentage0 = new System.Windows.Forms.Label();
            this.lbl_eProcessText = new System.Windows.Forms.Label();
            this.btn_eExport = new System.Windows.Forms.Button();
            this.lbl_eProgressBarDisplay = new System.Windows.Forms.Label();
            this.pBar_eProgress = new System.Windows.Forms.ProgressBar();
            this.panel_eTextArea = new System.Windows.Forms.Panel();
            this.btn_eExampleText = new System.Windows.Forms.Button();
            this.btn_eStartAction = new System.Windows.Forms.Button();
            this.rtxtbox_eEncryptText = new System.Windows.Forms.RichTextBox();
            this.lbl_eTextNotice = new System.Windows.Forms.Label();
            this.panel_eFileManagement = new System.Windows.Forms.Panel();
            this.btn_eBrowse = new System.Windows.Forms.Button();
            this.txtbox_eFilePath = new System.Windows.Forms.TextBox();
            this.lbl_eImport = new System.Windows.Forms.Label();
            this.eImageDisplay = new System.Windows.Forms.PictureBox();
            this.panel_Decrypt = new System.Windows.Forms.Panel();
            this.panel_dActions = new System.Windows.Forms.Panel();
            this.btn_dLogDisplay = new System.Windows.Forms.Button();
            this.btn_dReset = new System.Windows.Forms.Button();
            this.panel_dProgressBar = new System.Windows.Forms.Panel();
            this.lbl_dPercentage100 = new System.Windows.Forms.Label();
            this.lbl_dPercentage0 = new System.Windows.Forms.Label();
            this.lbl_dProcessText = new System.Windows.Forms.Label();
            this.btn_dExport = new System.Windows.Forms.Button();
            this.lbl_dProgressBarDisplay = new System.Windows.Forms.Label();
            this.pBar_dProgress = new System.Windows.Forms.ProgressBar();
            this.panel_dTextArea = new System.Windows.Forms.Panel();
            this.btn_dStartAction = new System.Windows.Forms.Button();
            this.rtxtbox_dDecryptText = new System.Windows.Forms.RichTextBox();
            this.lbl_dTextNotice = new System.Windows.Forms.Label();
            this.panel_dFileManagement = new System.Windows.Forms.Panel();
            this.btn_dBrowse = new System.Windows.Forms.Button();
            this.txtbox_dFilePath = new System.Windows.Forms.TextBox();
            this.lbl_dImport = new System.Windows.Forms.Label();
            this.dImageDisplay = new System.Windows.Forms.PictureBox();
            this.panel_Default = new System.Windows.Forms.Panel();
            this.lbl_DefaultMsg = new System.Windows.Forms.Label();
            this.lbl_ModeTitle = new System.Windows.Forms.Label();
            this.btn_Encrypt = new System.Windows.Forms.Button();
            this.btn_Decrypt = new System.Windows.Forms.Button();
            this.panel_Functions = new System.Windows.Forms.Panel();
            this.panel_Algorithm = new System.Windows.Forms.Panel();
            this.cBox_AlgoSelect = new System.Windows.Forms.ComboBox();
            this.lbl_AlgoSelect = new System.Windows.Forms.Label();
            this.menu.SuspendLayout();
            this.status.SuspendLayout();
            this.panel_Encrypt.SuspendLayout();
            this.panel_eActions.SuspendLayout();
            this.panel_eProgressBar.SuspendLayout();
            this.panel_eTextArea.SuspendLayout();
            this.panel_eFileManagement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eImageDisplay)).BeginInit();
            this.panel_Decrypt.SuspendLayout();
            this.panel_dActions.SuspendLayout();
            this.panel_dProgressBar.SuspendLayout();
            this.panel_dTextArea.SuspendLayout();
            this.panel_dFileManagement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dImageDisplay)).BeginInit();
            this.panel_Default.SuspendLayout();
            this.panel_Functions.SuspendLayout();
            this.panel_Algorithm.SuspendLayout();
            this.SuspendLayout();
            // 
            // menu
            // 
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_file,
            this.menu_help});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(800, 24);
            this.menu.TabIndex = 0;
            this.menu.Text = "menuStrip1";
            // 
            // menu_file
            // 
            this.menu_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_open,
            this.menu_save});
            this.menu_file.Name = "menu_file";
            this.menu_file.Size = new System.Drawing.Size(43, 20);
            this.menu_file.Text = "檔案";
            // 
            // menu_open
            // 
            this.menu_open.Name = "menu_open";
            this.menu_open.Size = new System.Drawing.Size(158, 22);
            this.menu_open.Text = "開啟先前的嘗試";
            // 
            // menu_save
            // 
            this.menu_save.Name = "menu_save";
            this.menu_save.Size = new System.Drawing.Size(158, 22);
            this.menu_save.Text = "儲存當前嘗試";
            // 
            // menu_help
            // 
            this.menu_help.Name = "menu_help";
            this.menu_help.Size = new System.Drawing.Size(43, 20);
            this.menu_help.Text = "幫助";
            // 
            // status
            // 
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tLabel1,
            this.tLabel2,
            this.tLabel3,
            this.tprogressBar});
            this.status.Location = new System.Drawing.Point(0, 506);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(800, 25);
            this.status.TabIndex = 4;
            this.status.Text = "statusStrip1";
            // 
            // tLabel1
            // 
            this.tLabel1.Name = "tLabel1";
            this.tLabel1.Size = new System.Drawing.Size(124, 20);
            this.tLabel1.Text = "開發者: Bernie (U.E.P)";
            this.tLabel1.ToolTipText = "開發者: Bernie";
            // 
            // tLabel2
            // 
            this.tLabel2.Name = "tLabel2";
            this.tLabel2.Size = new System.Drawing.Size(10, 20);
            this.tLabel2.Text = "|";
            // 
            // tLabel3
            // 
            this.tLabel3.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tLabel3.Name = "tLabel3";
            this.tLabel3.Size = new System.Drawing.Size(90, 20);
            this.tLabel3.Text = "藏密/萃取進度: ";
            // 
            // tprogressBar
            // 
            this.tprogressBar.Name = "tprogressBar";
            this.tprogressBar.Size = new System.Drawing.Size(100, 19);
            // 
            // btn_eHistogram
            // 
            this.btn_eHistogram.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_eHistogram.Location = new System.Drawing.Point(299, 3);
            this.btn_eHistogram.Name = "btn_eHistogram";
            this.btn_eHistogram.Size = new System.Drawing.Size(90, 37);
            this.btn_eHistogram.TabIndex = 1;
            this.btn_eHistogram.Text = "顯示直方圖(?)";
            this.toolTip.SetToolTip(this.btn_eHistogram, "只支援Qim演算法");
            this.btn_eHistogram.UseVisualStyleBackColor = true;
            this.btn_eHistogram.Click += new System.EventHandler(this.btn_eHistogram_Click);
            // 
            // panel_Encrypt
            // 
            this.panel_Encrypt.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel_Encrypt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel_Encrypt.Controls.Add(this.panel_eActions);
            this.panel_Encrypt.Controls.Add(this.panel_eProgressBar);
            this.panel_Encrypt.Controls.Add(this.panel_eTextArea);
            this.panel_Encrypt.Controls.Add(this.panel_eFileManagement);
            this.panel_Encrypt.Controls.Add(this.eImageDisplay);
            this.panel_Encrypt.Location = new System.Drawing.Point(0, 122);
            this.panel_Encrypt.Name = "panel_Encrypt";
            this.panel_Encrypt.Size = new System.Drawing.Size(799, 375);
            this.panel_Encrypt.TabIndex = 5;
            // 
            // panel_eActions
            // 
            this.panel_eActions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_eActions.Controls.Add(this.btn_eLogDisplay);
            this.panel_eActions.Controls.Add(this.btn_eHistogram);
            this.panel_eActions.Controls.Add(this.btn_eReset);
            this.panel_eActions.Location = new System.Drawing.Point(399, 15);
            this.panel_eActions.Name = "panel_eActions";
            this.panel_eActions.Size = new System.Drawing.Size(392, 45);
            this.panel_eActions.TabIndex = 7;
            // 
            // btn_eLogDisplay
            // 
            this.btn_eLogDisplay.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_eLogDisplay.Location = new System.Drawing.Point(150, 3);
            this.btn_eLogDisplay.Name = "btn_eLogDisplay";
            this.btn_eLogDisplay.Size = new System.Drawing.Size(90, 37);
            this.btn_eLogDisplay.TabIndex = 2;
            this.btn_eLogDisplay.Text = "顯示詳細流程";
            this.btn_eLogDisplay.UseVisualStyleBackColor = true;
            this.btn_eLogDisplay.Click += new System.EventHandler(this.btn_eLogDisplay_Click);
            // 
            // btn_eReset
            // 
            this.btn_eReset.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_eReset.Location = new System.Drawing.Point(3, 3);
            this.btn_eReset.Name = "btn_eReset";
            this.btn_eReset.Size = new System.Drawing.Size(90, 37);
            this.btn_eReset.TabIndex = 3;
            this.btn_eReset.Text = "重置藏密程序";
            this.btn_eReset.UseVisualStyleBackColor = true;
            this.btn_eReset.Click += new System.EventHandler(this.btn_eReset_Click);
            // 
            // panel_eProgressBar
            // 
            this.panel_eProgressBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_eProgressBar.Controls.Add(this.lbl_ePercentage100);
            this.panel_eProgressBar.Controls.Add(this.lbl_ePercentage0);
            this.panel_eProgressBar.Controls.Add(this.lbl_eProcessText);
            this.panel_eProgressBar.Controls.Add(this.btn_eExport);
            this.panel_eProgressBar.Controls.Add(this.lbl_eProgressBarDisplay);
            this.panel_eProgressBar.Controls.Add(this.pBar_eProgress);
            this.panel_eProgressBar.Location = new System.Drawing.Point(1, 278);
            this.panel_eProgressBar.Name = "panel_eProgressBar";
            this.panel_eProgressBar.Size = new System.Drawing.Size(393, 89);
            this.panel_eProgressBar.TabIndex = 6;
            // 
            // lbl_ePercentage100
            // 
            this.lbl_ePercentage100.AutoSize = true;
            this.lbl_ePercentage100.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_ePercentage100.Location = new System.Drawing.Point(342, 67);
            this.lbl_ePercentage100.Name = "lbl_ePercentage100";
            this.lbl_ePercentage100.Size = new System.Drawing.Size(42, 16);
            this.lbl_ePercentage100.TabIndex = 5;
            this.lbl_ePercentage100.Text = "100 %";
            // 
            // lbl_ePercentage0
            // 
            this.lbl_ePercentage0.AutoSize = true;
            this.lbl_ePercentage0.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_ePercentage0.Location = new System.Drawing.Point(5, 67);
            this.lbl_ePercentage0.Name = "lbl_ePercentage0";
            this.lbl_ePercentage0.Size = new System.Drawing.Size(28, 16);
            this.lbl_ePercentage0.TabIndex = 4;
            this.lbl_ePercentage0.Text = "0 %";
            // 
            // lbl_eProcessText
            // 
            this.lbl_eProcessText.AutoSize = true;
            this.lbl_eProcessText.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_eProcessText.ForeColor = System.Drawing.Color.Goldenrod;
            this.lbl_eProcessText.Location = new System.Drawing.Point(88, 13);
            this.lbl_eProcessText.Name = "lbl_eProcessText";
            this.lbl_eProcessText.Size = new System.Drawing.Size(66, 19);
            this.lbl_eProcessText.TabIndex = 3;
            this.lbl_eProcessText.Text = "處理中...";
            // 
            // btn_eExport
            // 
            this.btn_eExport.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_eExport.Location = new System.Drawing.Point(291, 7);
            this.btn_eExport.Name = "btn_eExport";
            this.btn_eExport.Size = new System.Drawing.Size(93, 32);
            this.btn_eExport.TabIndex = 2;
            this.btn_eExport.Text = "匯出圖片";
            this.btn_eExport.UseVisualStyleBackColor = true;
            this.btn_eExport.Click += new System.EventHandler(this.btn_eExport_Click);
            // 
            // lbl_eProgressBarDisplay
            // 
            this.lbl_eProgressBarDisplay.AutoSize = true;
            this.lbl_eProgressBarDisplay.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_eProgressBarDisplay.Location = new System.Drawing.Point(4, 11);
            this.lbl_eProgressBarDisplay.Name = "lbl_eProgressBarDisplay";
            this.lbl_eProgressBarDisplay.Size = new System.Drawing.Size(78, 21);
            this.lbl_eProgressBarDisplay.TabIndex = 1;
            this.lbl_eProgressBarDisplay.Text = "藏密進度:";
            // 
            // pBar_eProgress
            // 
            this.pBar_eProgress.Location = new System.Drawing.Point(8, 47);
            this.pBar_eProgress.Name = "pBar_eProgress";
            this.pBar_eProgress.Size = new System.Drawing.Size(376, 17);
            this.pBar_eProgress.TabIndex = 0;
            // 
            // panel_eTextArea
            // 
            this.panel_eTextArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_eTextArea.Controls.Add(this.btn_eExampleText);
            this.panel_eTextArea.Controls.Add(this.btn_eStartAction);
            this.panel_eTextArea.Controls.Add(this.rtxtbox_eEncryptText);
            this.panel_eTextArea.Controls.Add(this.lbl_eTextNotice);
            this.panel_eTextArea.Location = new System.Drawing.Point(1, 75);
            this.panel_eTextArea.Name = "panel_eTextArea";
            this.panel_eTextArea.Size = new System.Drawing.Size(392, 189);
            this.panel_eTextArea.TabIndex = 5;
            // 
            // btn_eExampleText
            // 
            this.btn_eExampleText.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_eExampleText.Location = new System.Drawing.Point(196, 8);
            this.btn_eExampleText.Name = "btn_eExampleText";
            this.btn_eExampleText.Size = new System.Drawing.Size(84, 33);
            this.btn_eExampleText.TabIndex = 6;
            this.btn_eExampleText.Text = "範例文字";
            this.btn_eExampleText.UseVisualStyleBackColor = true;
            this.btn_eExampleText.Click += new System.EventHandler(this.btn_eExampleText_Click);
            // 
            // btn_eStartAction
            // 
            this.btn_eStartAction.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_eStartAction.Location = new System.Drawing.Point(299, 8);
            this.btn_eStartAction.Name = "btn_eStartAction";
            this.btn_eStartAction.Size = new System.Drawing.Size(84, 33);
            this.btn_eStartAction.TabIndex = 5;
            this.btn_eStartAction.Text = "執行藏密!";
            this.btn_eStartAction.UseVisualStyleBackColor = true;
            this.btn_eStartAction.Click += new System.EventHandler(this.btn_eStartAction_Click);
            // 
            // rtxtbox_eEncryptText
            // 
            this.rtxtbox_eEncryptText.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtxtbox_eEncryptText.ForeColor = System.Drawing.Color.DarkGray;
            this.rtxtbox_eEncryptText.Location = new System.Drawing.Point(7, 42);
            this.rtxtbox_eEncryptText.Name = "rtxtbox_eEncryptText";
            this.rtxtbox_eEncryptText.Size = new System.Drawing.Size(376, 144);
            this.rtxtbox_eEncryptText.TabIndex = 4;
            this.rtxtbox_eEncryptText.Text = "在這裡填入你的文字...";
            this.rtxtbox_eEncryptText.TextChanged += new System.EventHandler(this.rtxtbox_eEncryptText_TextChanged);
            // 
            // lbl_eTextNotice
            // 
            this.lbl_eTextNotice.AutoSize = true;
            this.lbl_eTextNotice.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_eTextNotice.Location = new System.Drawing.Point(3, 13);
            this.lbl_eTextNotice.Name = "lbl_eTextNotice";
            this.lbl_eTextNotice.Size = new System.Drawing.Size(110, 21);
            this.lbl_eTextNotice.TabIndex = 3;
            this.lbl_eTextNotice.Text = "填入欲藏文字:";
            // 
            // panel_eFileManagement
            // 
            this.panel_eFileManagement.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_eFileManagement.Controls.Add(this.btn_eBrowse);
            this.panel_eFileManagement.Controls.Add(this.txtbox_eFilePath);
            this.panel_eFileManagement.Controls.Add(this.lbl_eImport);
            this.panel_eFileManagement.Location = new System.Drawing.Point(1, 15);
            this.panel_eFileManagement.Name = "panel_eFileManagement";
            this.panel_eFileManagement.Size = new System.Drawing.Size(392, 46);
            this.panel_eFileManagement.TabIndex = 4;
            // 
            // btn_eBrowse
            // 
            this.btn_eBrowse.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_eBrowse.Location = new System.Drawing.Point(324, 7);
            this.btn_eBrowse.Name = "btn_eBrowse";
            this.btn_eBrowse.Size = new System.Drawing.Size(59, 30);
            this.btn_eBrowse.TabIndex = 2;
            this.btn_eBrowse.Text = "瀏覽...";
            this.btn_eBrowse.UseVisualStyleBackColor = true;
            this.btn_eBrowse.Click += new System.EventHandler(this.btn_eBrowse_Click);
            // 
            // txtbox_eFilePath
            // 
            this.txtbox_eFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtbox_eFilePath.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtbox_eFilePath.Location = new System.Drawing.Point(118, 7);
            this.txtbox_eFilePath.Name = "txtbox_eFilePath";
            this.txtbox_eFilePath.ReadOnly = true;
            this.txtbox_eFilePath.Size = new System.Drawing.Size(200, 33);
            this.txtbox_eFilePath.TabIndex = 1;
            // 
            // lbl_eImport
            // 
            this.lbl_eImport.AutoSize = true;
            this.lbl_eImport.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_eImport.Location = new System.Drawing.Point(3, 12);
            this.lbl_eImport.Name = "lbl_eImport";
            this.lbl_eImport.Size = new System.Drawing.Size(110, 21);
            this.lbl_eImport.TabIndex = 0;
            this.lbl_eImport.Text = "導入圖像檔案:";
            // 
            // eImageDisplay
            // 
            this.eImageDisplay.BackgroundImage = global::StegoApolloUI.Properties.Resources.Default_Preview;
            this.eImageDisplay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.eImageDisplay.InitialImage = global::StegoApolloUI.Properties.Resources.Default_Preview;
            this.eImageDisplay.Location = new System.Drawing.Point(399, 67);
            this.eImageDisplay.Name = "eImageDisplay";
            this.eImageDisplay.Size = new System.Drawing.Size(393, 300);
            this.eImageDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.eImageDisplay.TabIndex = 0;
            this.eImageDisplay.TabStop = false;
            // 
            // panel_Decrypt
            // 
            this.panel_Decrypt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel_Decrypt.Controls.Add(this.panel_dActions);
            this.panel_Decrypt.Controls.Add(this.panel_dProgressBar);
            this.panel_Decrypt.Controls.Add(this.panel_dTextArea);
            this.panel_Decrypt.Controls.Add(this.panel_dFileManagement);
            this.panel_Decrypt.Controls.Add(this.dImageDisplay);
            this.panel_Decrypt.Location = new System.Drawing.Point(0, 122);
            this.panel_Decrypt.Name = "panel_Decrypt";
            this.panel_Decrypt.Size = new System.Drawing.Size(799, 375);
            this.panel_Decrypt.TabIndex = 6;
            // 
            // panel_dActions
            // 
            this.panel_dActions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_dActions.Controls.Add(this.btn_dLogDisplay);
            this.panel_dActions.Controls.Add(this.btn_dReset);
            this.panel_dActions.Location = new System.Drawing.Point(3, 20);
            this.panel_dActions.Name = "panel_dActions";
            this.panel_dActions.Size = new System.Drawing.Size(395, 45);
            this.panel_dActions.TabIndex = 8;
            // 
            // btn_dLogDisplay
            // 
            this.btn_dLogDisplay.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_dLogDisplay.Location = new System.Drawing.Point(295, 2);
            this.btn_dLogDisplay.Name = "btn_dLogDisplay";
            this.btn_dLogDisplay.Size = new System.Drawing.Size(90, 37);
            this.btn_dLogDisplay.TabIndex = 2;
            this.btn_dLogDisplay.Text = "顯示詳細流程";
            this.btn_dLogDisplay.UseVisualStyleBackColor = true;
            this.btn_dLogDisplay.Click += new System.EventHandler(this.btn_dLogDisplay_Click);
            // 
            // btn_dReset
            // 
            this.btn_dReset.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_dReset.Location = new System.Drawing.Point(7, 2);
            this.btn_dReset.Name = "btn_dReset";
            this.btn_dReset.Size = new System.Drawing.Size(90, 37);
            this.btn_dReset.TabIndex = 3;
            this.btn_dReset.Text = "重置萃取程序";
            this.btn_dReset.UseVisualStyleBackColor = true;
            this.btn_dReset.Click += new System.EventHandler(this.btn_dReset_Click);
            // 
            // panel_dProgressBar
            // 
            this.panel_dProgressBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_dProgressBar.Controls.Add(this.lbl_dPercentage100);
            this.panel_dProgressBar.Controls.Add(this.lbl_dPercentage0);
            this.panel_dProgressBar.Controls.Add(this.lbl_dProcessText);
            this.panel_dProgressBar.Controls.Add(this.btn_dExport);
            this.panel_dProgressBar.Controls.Add(this.lbl_dProgressBarDisplay);
            this.panel_dProgressBar.Controls.Add(this.pBar_dProgress);
            this.panel_dProgressBar.Location = new System.Drawing.Point(404, 275);
            this.panel_dProgressBar.Name = "panel_dProgressBar";
            this.panel_dProgressBar.Size = new System.Drawing.Size(392, 94);
            this.panel_dProgressBar.TabIndex = 7;
            // 
            // lbl_dPercentage100
            // 
            this.lbl_dPercentage100.AutoSize = true;
            this.lbl_dPercentage100.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_dPercentage100.Location = new System.Drawing.Point(342, 67);
            this.lbl_dPercentage100.Name = "lbl_dPercentage100";
            this.lbl_dPercentage100.Size = new System.Drawing.Size(42, 16);
            this.lbl_dPercentage100.TabIndex = 5;
            this.lbl_dPercentage100.Text = "100 %";
            // 
            // lbl_dPercentage0
            // 
            this.lbl_dPercentage0.AutoSize = true;
            this.lbl_dPercentage0.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_dPercentage0.Location = new System.Drawing.Point(5, 67);
            this.lbl_dPercentage0.Name = "lbl_dPercentage0";
            this.lbl_dPercentage0.Size = new System.Drawing.Size(28, 16);
            this.lbl_dPercentage0.TabIndex = 4;
            this.lbl_dPercentage0.Text = "0 %";
            // 
            // lbl_dProcessText
            // 
            this.lbl_dProcessText.AutoSize = true;
            this.lbl_dProcessText.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_dProcessText.ForeColor = System.Drawing.Color.Goldenrod;
            this.lbl_dProcessText.Location = new System.Drawing.Point(88, 13);
            this.lbl_dProcessText.Name = "lbl_dProcessText";
            this.lbl_dProcessText.Size = new System.Drawing.Size(66, 19);
            this.lbl_dProcessText.TabIndex = 3;
            this.lbl_dProcessText.Text = "處理中...";
            // 
            // btn_dExport
            // 
            this.btn_dExport.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_dExport.Location = new System.Drawing.Point(291, 7);
            this.btn_dExport.Name = "btn_dExport";
            this.btn_dExport.Size = new System.Drawing.Size(93, 32);
            this.btn_dExport.TabIndex = 2;
            this.btn_dExport.Text = "複製文字";
            this.btn_dExport.UseVisualStyleBackColor = true;
            this.btn_dExport.Click += new System.EventHandler(this.btn_dExport_Click);
            // 
            // lbl_dProgressBarDisplay
            // 
            this.lbl_dProgressBarDisplay.AutoSize = true;
            this.lbl_dProgressBarDisplay.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_dProgressBarDisplay.Location = new System.Drawing.Point(4, 11);
            this.lbl_dProgressBarDisplay.Name = "lbl_dProgressBarDisplay";
            this.lbl_dProgressBarDisplay.Size = new System.Drawing.Size(78, 21);
            this.lbl_dProgressBarDisplay.TabIndex = 1;
            this.lbl_dProgressBarDisplay.Text = "萃取進度:";
            // 
            // pBar_dProgress
            // 
            this.pBar_dProgress.Location = new System.Drawing.Point(8, 47);
            this.pBar_dProgress.Name = "pBar_dProgress";
            this.pBar_dProgress.Size = new System.Drawing.Size(376, 17);
            this.pBar_dProgress.TabIndex = 0;
            // 
            // panel_dTextArea
            // 
            this.panel_dTextArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_dTextArea.Controls.Add(this.btn_dStartAction);
            this.panel_dTextArea.Controls.Add(this.rtxtbox_dDecryptText);
            this.panel_dTextArea.Controls.Add(this.lbl_dTextNotice);
            this.panel_dTextArea.Location = new System.Drawing.Point(404, 77);
            this.panel_dTextArea.Name = "panel_dTextArea";
            this.panel_dTextArea.Size = new System.Drawing.Size(392, 189);
            this.panel_dTextArea.TabIndex = 6;
            // 
            // btn_dStartAction
            // 
            this.btn_dStartAction.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_dStartAction.Location = new System.Drawing.Point(299, 8);
            this.btn_dStartAction.Name = "btn_dStartAction";
            this.btn_dStartAction.Size = new System.Drawing.Size(84, 33);
            this.btn_dStartAction.TabIndex = 5;
            this.btn_dStartAction.Text = "執行萃取!";
            this.btn_dStartAction.UseVisualStyleBackColor = true;
            this.btn_dStartAction.Click += new System.EventHandler(this.btn_dStartAction_Click);
            // 
            // rtxtbox_dDecryptText
            // 
            this.rtxtbox_dDecryptText.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtxtbox_dDecryptText.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.rtxtbox_dDecryptText.Location = new System.Drawing.Point(7, 42);
            this.rtxtbox_dDecryptText.Name = "rtxtbox_dDecryptText";
            this.rtxtbox_dDecryptText.ReadOnly = true;
            this.rtxtbox_dDecryptText.Size = new System.Drawing.Size(376, 144);
            this.rtxtbox_dDecryptText.TabIndex = 4;
            this.rtxtbox_dDecryptText.Text = "這裡會輸出萃取後的文字...";
            // 
            // lbl_dTextNotice
            // 
            this.lbl_dTextNotice.AutoSize = true;
            this.lbl_dTextNotice.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_dTextNotice.Location = new System.Drawing.Point(3, 13);
            this.lbl_dTextNotice.Name = "lbl_dTextNotice";
            this.lbl_dTextNotice.Size = new System.Drawing.Size(110, 21);
            this.lbl_dTextNotice.TabIndex = 3;
            this.lbl_dTextNotice.Text = "萃取出的文字:";
            // 
            // panel_dFileManagement
            // 
            this.panel_dFileManagement.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_dFileManagement.Controls.Add(this.btn_dBrowse);
            this.panel_dFileManagement.Controls.Add(this.txtbox_dFilePath);
            this.panel_dFileManagement.Controls.Add(this.lbl_dImport);
            this.panel_dFileManagement.Location = new System.Drawing.Point(404, 20);
            this.panel_dFileManagement.Name = "panel_dFileManagement";
            this.panel_dFileManagement.Size = new System.Drawing.Size(392, 46);
            this.panel_dFileManagement.TabIndex = 5;
            // 
            // btn_dBrowse
            // 
            this.btn_dBrowse.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_dBrowse.Location = new System.Drawing.Point(324, 7);
            this.btn_dBrowse.Name = "btn_dBrowse";
            this.btn_dBrowse.Size = new System.Drawing.Size(59, 30);
            this.btn_dBrowse.TabIndex = 2;
            this.btn_dBrowse.Text = "瀏覽...";
            this.btn_dBrowse.UseVisualStyleBackColor = true;
            this.btn_dBrowse.Click += new System.EventHandler(this.btn_dBrowse_Click);
            // 
            // txtbox_dFilePath
            // 
            this.txtbox_dFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtbox_dFilePath.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtbox_dFilePath.Location = new System.Drawing.Point(118, 7);
            this.txtbox_dFilePath.Name = "txtbox_dFilePath";
            this.txtbox_dFilePath.ReadOnly = true;
            this.txtbox_dFilePath.Size = new System.Drawing.Size(200, 33);
            this.txtbox_dFilePath.TabIndex = 1;
            // 
            // lbl_dImport
            // 
            this.lbl_dImport.AutoSize = true;
            this.lbl_dImport.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_dImport.Location = new System.Drawing.Point(3, 12);
            this.lbl_dImport.Name = "lbl_dImport";
            this.lbl_dImport.Size = new System.Drawing.Size(110, 21);
            this.lbl_dImport.TabIndex = 0;
            this.lbl_dImport.Text = "導入圖像檔案:";
            // 
            // dImageDisplay
            // 
            this.dImageDisplay.BackgroundImage = global::StegoApolloUI.Properties.Resources.Default_Preview;
            this.dImageDisplay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.dImageDisplay.InitialImage = global::StegoApolloUI.Properties.Resources.Default_Preview;
            this.dImageDisplay.Location = new System.Drawing.Point(5, 69);
            this.dImageDisplay.Name = "dImageDisplay";
            this.dImageDisplay.Size = new System.Drawing.Size(393, 300);
            this.dImageDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.dImageDisplay.TabIndex = 1;
            this.dImageDisplay.TabStop = false;
            // 
            // panel_Default
            // 
            this.panel_Default.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.panel_Default.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Default.Controls.Add(this.lbl_DefaultMsg);
            this.panel_Default.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel_Default.Location = new System.Drawing.Point(0, 122);
            this.panel_Default.Name = "panel_Default";
            this.panel_Default.Size = new System.Drawing.Size(799, 375);
            this.panel_Default.TabIndex = 7;
            // 
            // lbl_DefaultMsg
            // 
            this.lbl_DefaultMsg.AutoSize = true;
            this.lbl_DefaultMsg.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_DefaultMsg.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lbl_DefaultMsg.Location = new System.Drawing.Point(251, 159);
            this.lbl_DefaultMsg.Name = "lbl_DefaultMsg";
            this.lbl_DefaultMsg.Size = new System.Drawing.Size(292, 40);
            this.lbl_DefaultMsg.TabIndex = 0;
            this.lbl_DefaultMsg.Text = "等你選一個模式 <3";
            // 
            // lbl_ModeTitle
            // 
            this.lbl_ModeTitle.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_ModeTitle.Location = new System.Drawing.Point(87, 3);
            this.lbl_ModeTitle.Name = "lbl_ModeTitle";
            this.lbl_ModeTitle.Size = new System.Drawing.Size(202, 34);
            this.lbl_ModeTitle.TabIndex = 0;
            this.lbl_ModeTitle.Text = "選擇一個模式來開始";
            this.lbl_ModeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_ModeTitle.UseCompatibleTextRendering = true;
            // 
            // btn_Encrypt
            // 
            this.btn_Encrypt.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_Encrypt.Location = new System.Drawing.Point(20, 47);
            this.btn_Encrypt.Name = "btn_Encrypt";
            this.btn_Encrypt.Size = new System.Drawing.Size(116, 48);
            this.btn_Encrypt.TabIndex = 9;
            this.btn_Encrypt.Text = "藏密";
            this.btn_Encrypt.UseVisualStyleBackColor = true;
            this.btn_Encrypt.Click += new System.EventHandler(this.btn_Encrypt_Click);
            // 
            // btn_Decrypt
            // 
            this.btn_Decrypt.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_Decrypt.Location = new System.Drawing.Point(236, 47);
            this.btn_Decrypt.Name = "btn_Decrypt";
            this.btn_Decrypt.Size = new System.Drawing.Size(116, 48);
            this.btn_Decrypt.TabIndex = 10;
            this.btn_Decrypt.Text = "萃取";
            this.btn_Decrypt.UseVisualStyleBackColor = true;
            this.btn_Decrypt.Click += new System.EventHandler(this.btn_Decrypt_Click);
            // 
            // panel_Functions
            // 
            this.panel_Functions.Controls.Add(this.btn_Decrypt);
            this.panel_Functions.Controls.Add(this.btn_Encrypt);
            this.panel_Functions.Controls.Add(this.lbl_ModeTitle);
            this.panel_Functions.Location = new System.Drawing.Point(214, 21);
            this.panel_Functions.Name = "panel_Functions";
            this.panel_Functions.Size = new System.Drawing.Size(373, 101);
            this.panel_Functions.TabIndex = 11;
            // 
            // panel_Algorithm
            // 
            this.panel_Algorithm.Controls.Add(this.cBox_AlgoSelect);
            this.panel_Algorithm.Controls.Add(this.lbl_AlgoSelect);
            this.panel_Algorithm.Location = new System.Drawing.Point(587, 21);
            this.panel_Algorithm.Name = "panel_Algorithm";
            this.panel_Algorithm.Size = new System.Drawing.Size(213, 101);
            this.panel_Algorithm.TabIndex = 11;
            // 
            // cBox_AlgoSelect
            // 
            this.cBox_AlgoSelect.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cBox_AlgoSelect.FormattingEnabled = true;
            this.cBox_AlgoSelect.Location = new System.Drawing.Point(48, 59);
            this.cBox_AlgoSelect.Name = "cBox_AlgoSelect";
            this.cBox_AlgoSelect.Size = new System.Drawing.Size(121, 28);
            this.cBox_AlgoSelect.TabIndex = 99;
            this.cBox_AlgoSelect.SelectedIndexChanged += new System.EventHandler(this.cBox_AlgoSelect_SelectedIndexChanged);
            // 
            // lbl_AlgoSelect
            // 
            this.lbl_AlgoSelect.AutoSize = true;
            this.lbl_AlgoSelect.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_AlgoSelect.Location = new System.Drawing.Point(38, 16);
            this.lbl_AlgoSelect.Name = "lbl_AlgoSelect";
            this.lbl_AlgoSelect.Size = new System.Drawing.Size(142, 21);
            this.lbl_AlgoSelect.TabIndex = 0;
            this.lbl_AlgoSelect.Text = "選擇使用的演算法:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 531);
            this.Controls.Add(this.panel_Algorithm);
            this.Controls.Add(this.panel_Functions);
            this.Controls.Add(this.panel_Decrypt);
            this.Controls.Add(this.panel_Default);
            this.Controls.Add(this.panel_Encrypt);
            this.Controls.Add(this.status);
            this.Controls.Add(this.menu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menu;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "StegoApollo";
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.panel_Encrypt.ResumeLayout(false);
            this.panel_eActions.ResumeLayout(false);
            this.panel_eProgressBar.ResumeLayout(false);
            this.panel_eProgressBar.PerformLayout();
            this.panel_eTextArea.ResumeLayout(false);
            this.panel_eTextArea.PerformLayout();
            this.panel_eFileManagement.ResumeLayout(false);
            this.panel_eFileManagement.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eImageDisplay)).EndInit();
            this.panel_Decrypt.ResumeLayout(false);
            this.panel_dActions.ResumeLayout(false);
            this.panel_dProgressBar.ResumeLayout(false);
            this.panel_dProgressBar.PerformLayout();
            this.panel_dTextArea.ResumeLayout(false);
            this.panel_dTextArea.PerformLayout();
            this.panel_dFileManagement.ResumeLayout(false);
            this.panel_dFileManagement.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dImageDisplay)).EndInit();
            this.panel_Default.ResumeLayout(false);
            this.panel_Default.PerformLayout();
            this.panel_Functions.ResumeLayout(false);
            this.panel_Algorithm.ResumeLayout(false);
            this.panel_Algorithm.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem menu_file;
        private System.Windows.Forms.ToolStripMenuItem menu_help;
        private System.Windows.Forms.ToolStripMenuItem menu_open;
        private System.Windows.Forms.ToolStripMenuItem menu_save;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel panel_Encrypt;
        private System.Windows.Forms.Panel panel_Decrypt;
        private System.Windows.Forms.Panel panel_Default;
        private System.Windows.Forms.Label lbl_ModeTitle;
        private System.Windows.Forms.Button btn_Encrypt;
        private System.Windows.Forms.Button btn_Decrypt;
        private System.Windows.Forms.Panel panel_eTextArea;
        private System.Windows.Forms.Panel panel_eFileManagement;
        private System.Windows.Forms.Button btn_eReset;
        private System.Windows.Forms.Button btn_eLogDisplay;
        private System.Windows.Forms.Button btn_eHistogram;
        private System.Windows.Forms.PictureBox eImageDisplay;
        private System.Windows.Forms.Button btn_eBrowse;
        private System.Windows.Forms.TextBox txtbox_eFilePath;
        private System.Windows.Forms.Label lbl_eImport;
        private System.Windows.Forms.Label lbl_eTextNotice;
        private System.Windows.Forms.RichTextBox rtxtbox_eEncryptText;
        private System.Windows.Forms.Button btn_eExampleText;
        private System.Windows.Forms.Button btn_eStartAction;
        private System.Windows.Forms.Panel panel_eProgressBar;
        private System.Windows.Forms.Button btn_eExport;
        private System.Windows.Forms.Label lbl_eProgressBarDisplay;
        private System.Windows.Forms.ProgressBar pBar_eProgress;
        private System.Windows.Forms.Label lbl_eProcessText;
        private System.Windows.Forms.Label lbl_ePercentage100;
        private System.Windows.Forms.Label lbl_ePercentage0;
        private System.Windows.Forms.Panel panel_eActions;
        private System.Windows.Forms.Panel panel_Functions;
        private System.Windows.Forms.Panel panel_dProgressBar;
        private System.Windows.Forms.Label lbl_dPercentage100;
        private System.Windows.Forms.Label lbl_dPercentage0;
        private System.Windows.Forms.Label lbl_dProcessText;
        private System.Windows.Forms.Button btn_dExport;
        private System.Windows.Forms.Label lbl_dProgressBarDisplay;
        private System.Windows.Forms.ProgressBar pBar_dProgress;
        private System.Windows.Forms.Panel panel_dTextArea;
        private System.Windows.Forms.Button btn_dStartAction;
        private System.Windows.Forms.RichTextBox rtxtbox_dDecryptText;
        private System.Windows.Forms.Label lbl_dTextNotice;
        private System.Windows.Forms.Panel panel_dFileManagement;
        private System.Windows.Forms.Button btn_dBrowse;
        private System.Windows.Forms.TextBox txtbox_dFilePath;
        private System.Windows.Forms.Label lbl_dImport;
        private System.Windows.Forms.PictureBox dImageDisplay;
        private System.Windows.Forms.Panel panel_dActions;
        private System.Windows.Forms.Button btn_dLogDisplay;
        private System.Windows.Forms.Button btn_dReset;
        private System.Windows.Forms.Label lbl_DefaultMsg;
        private System.Windows.Forms.ToolStripStatusLabel tLabel3;
        private System.Windows.Forms.ToolStripProgressBar tprogressBar;
        private System.Windows.Forms.ToolStripStatusLabel tLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tLabel2;
        private System.Windows.Forms.Panel panel_Algorithm;
        private System.Windows.Forms.Label lbl_AlgoSelect;
        private System.Windows.Forms.ComboBox cBox_AlgoSelect;
    }
}

