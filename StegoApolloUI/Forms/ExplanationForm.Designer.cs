namespace StegoApolloUI.Forms
{
    partial class ExplanationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExplanationForm));
            this.rtxtbox_Content = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtxtbox_Content
            // 
            this.rtxtbox_Content.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtxtbox_Content.Location = new System.Drawing.Point(0, -1);
            this.rtxtbox_Content.Name = "rtxtbox_Content";
            this.rtxtbox_Content.ReadOnly = true;
            this.rtxtbox_Content.Size = new System.Drawing.Size(285, 537);
            this.rtxtbox_Content.TabIndex = 0;
            this.rtxtbox_Content.Text = "";
            // 
            // ExplanationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 531);
            this.Controls.Add(this.rtxtbox_Content);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExplanationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Explanation";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtxtbox_Content;
    }
}