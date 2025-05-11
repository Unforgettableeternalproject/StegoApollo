namespace StegoApolloUI.Forms
{
    partial class LogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogForm));
            this.rtxtbox_Logs = new System.Windows.Forms.RichTextBox();
            this.lbl_Title = new System.Windows.Forms.Label();
            this.btn_ClearLogs = new System.Windows.Forms.Button();
            this.btn_CopyLogs = new System.Windows.Forms.Button();
            this.timer_refreash = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // rtxtbox_Logs
            // 
            this.rtxtbox_Logs.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtxtbox_Logs.Location = new System.Drawing.Point(0, 34);
            this.rtxtbox_Logs.Name = "rtxtbox_Logs";
            this.rtxtbox_Logs.ReadOnly = true;
            this.rtxtbox_Logs.Size = new System.Drawing.Size(283, 459);
            this.rtxtbox_Logs.TabIndex = 0;
            this.rtxtbox_Logs.Text = "";
            // 
            // lbl_Title
            // 
            this.lbl_Title.AutoSize = true;
            this.lbl_Title.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_Title.Location = new System.Drawing.Point(84, 9);
            this.lbl_Title.Name = "lbl_Title";
            this.lbl_Title.Size = new System.Drawing.Size(106, 21);
            this.lbl_Title.TabIndex = 1;
            this.lbl_Title.Text = "詳細流程紀錄";
            // 
            // btn_ClearLogs
            // 
            this.btn_ClearLogs.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_ClearLogs.Location = new System.Drawing.Point(210, 499);
            this.btn_ClearLogs.Name = "btn_ClearLogs";
            this.btn_ClearLogs.Size = new System.Drawing.Size(68, 28);
            this.btn_ClearLogs.TabIndex = 2;
            this.btn_ClearLogs.Text = "清除內容";
            this.btn_ClearLogs.UseVisualStyleBackColor = true;
            this.btn_ClearLogs.Click += new System.EventHandler(this.btn_ClearLogs_Click);
            // 
            // btn_CopyLogs
            // 
            this.btn_CopyLogs.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_CopyLogs.Location = new System.Drawing.Point(7, 499);
            this.btn_CopyLogs.Name = "btn_CopyLogs";
            this.btn_CopyLogs.Size = new System.Drawing.Size(68, 28);
            this.btn_CopyLogs.TabIndex = 3;
            this.btn_CopyLogs.Text = "複製內容";
            this.btn_CopyLogs.UseVisualStyleBackColor = true;
            this.btn_CopyLogs.Click += new System.EventHandler(this.btn_CopyLogs_Click);
            // 
            // timer_refreash
            // 
            this.timer_refreash.Interval = 1000;
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 531);
            this.Controls.Add(this.btn_CopyLogs);
            this.Controls.Add(this.btn_ClearLogs);
            this.Controls.Add(this.lbl_Title);
            this.Controls.Add(this.rtxtbox_Logs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Logs";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtxtbox_Logs;
        private System.Windows.Forms.Label lbl_Title;
        private System.Windows.Forms.Button btn_ClearLogs;
        private System.Windows.Forms.Button btn_CopyLogs;
        private System.Windows.Forms.Timer timer_refreash;
    }
}