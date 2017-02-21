namespace Youtube_Downloader
{
    partial class frmMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnDownload = new System.Windows.Forms.Button();
            this.textboxURL = new System.Windows.Forms.TextBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.listboxFormat = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnClipboard = new System.Windows.Forms.Button();
            this.listboxVideo = new System.Windows.Forms.ListBox();
            this.labelInfomation = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnDownload
            // 
            this.btnDownload.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnDownload.Location = new System.Drawing.Point(455, 275);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(125, 43);
            this.btnDownload.TabIndex = 0;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // textboxURL
            // 
            this.textboxURL.Location = new System.Drawing.Point(51, 45);
            this.textboxURL.Name = "textboxURL";
            this.textboxURL.Size = new System.Drawing.Size(502, 19);
            this.textboxURL.TabIndex = 1;
            // 
            // btnFind
            // 
            this.btnFind.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnFind.Location = new System.Drawing.Point(559, 45);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(70, 24);
            this.btnFind.TabIndex = 2;
            this.btnFind.Text = "find";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // listboxFormat
            // 
            this.listboxFormat.FormattingEnabled = true;
            this.listboxFormat.ItemHeight = 12;
            this.listboxFormat.Location = new System.Drawing.Point(51, 202);
            this.listboxFormat.Name = "listboxFormat";
            this.listboxFormat.Size = new System.Drawing.Size(363, 88);
            this.listboxFormat.TabIndex = 3;
            this.listboxFormat.SelectedIndexChanged += new System.EventHandler(this.listboxFormat_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(271, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "Please input the Youtube URL of a video or playlist.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "URL:";
            // 
            // btnClipboard
            // 
            this.btnClipboard.Location = new System.Drawing.Point(455, 3);
            this.btnClipboard.Name = "btnClipboard";
            this.btnClipboard.Size = new System.Drawing.Size(73, 36);
            this.btnClipboard.TabIndex = 6;
            this.btnClipboard.Text = "Copy from Clipboard";
            this.btnClipboard.UseVisualStyleBackColor = true;
            this.btnClipboard.Click += new System.EventHandler(this.btnClipboard_Click);
            // 
            // listboxVideo
            // 
            this.listboxVideo.FormattingEnabled = true;
            this.listboxVideo.ItemHeight = 12;
            this.listboxVideo.Location = new System.Drawing.Point(51, 82);
            this.listboxVideo.Name = "listboxVideo";
            this.listboxVideo.Size = new System.Drawing.Size(369, 76);
            this.listboxVideo.TabIndex = 7;
            this.listboxVideo.SelectedIndexChanged += new System.EventHandler(this.listboxVideo_SelectedIndexChanged);
            // 
            // labelInfomation
            // 
            this.labelInfomation.AutoSize = true;
            this.labelInfomation.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labelInfomation.Location = new System.Drawing.Point(455, 111);
            this.labelInfomation.Name = "labelInfomation";
            this.labelInfomation.Size = new System.Drawing.Size(0, 16);
            this.labelInfomation.TabIndex = 8;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(641, 358);
            this.Controls.Add(this.labelInfomation);
            this.Controls.Add(this.listboxVideo);
            this.Controls.Add(this.btnClipboard);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listboxFormat);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.textboxURL);
            this.Controls.Add(this.btnDownload);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Youtube Downloader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.TextBox textboxURL;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.ListBox listboxFormat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnClipboard;
        private System.Windows.Forms.ListBox listboxVideo;
        private System.Windows.Forms.Label labelInfomation;
    }
}

