namespace SatIp.RtspSample
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.PlayList = new System.Windows.Forms.ListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslblLevel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tspgrLevel = new System.Windows.Forms.ToolStripProgressBar();
            this.tsslblQuality = new System.Windows.Forms.ToolStripStatusLabel();
            this.tspgrQuality = new System.Windows.Forms.ToolStripProgressBar();
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            this.SuspendLayout();
            // 
            // PlayList
            // 
            this.PlayList.DisplayMember = "Name";
            this.PlayList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PlayList.FormattingEnabled = true;
            this.PlayList.Location = new System.Drawing.Point(0, 0);
            this.PlayList.Margin = new System.Windows.Forms.Padding(0);
            this.PlayList.Name = "PlayList";
            this.PlayList.Size = new System.Drawing.Size(351, 264);
            this.PlayList.TabIndex = 5;
            this.PlayList.SelectedIndexChanged += new System.EventHandler(this.PlayList_SelectedIndexChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.axWindowsMediaPlayer1);
            this.splitContainer1.Size = new System.Drawing.Size(1055, 341);
            this.splitContainer1.SplitterDistance = 351;
            this.splitContainer1.TabIndex = 6;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.listBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer2.Panel2.Controls.Add(this.PlayList);
            this.splitContainer2.Size = new System.Drawing.Size(351, 341);
            this.splitContainer2.SplitterDistance = 62;
            this.splitContainer2.TabIndex = 0;
            // 
            // listBox1
            // 
            this.listBox1.DisplayMember = "FriendlyName";
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(351, 56);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslblLevel,
            this.tspgrLevel,
            this.tsslblQuality,
            this.tspgrQuality});
            this.statusStrip1.Location = new System.Drawing.Point(0, 253);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(351, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslblLevel
            // 
            this.tsslblLevel.Name = "tsslblLevel";
            this.tsslblLevel.Size = new System.Drawing.Size(40, 17);
            this.tsslblLevel.Text = "Level :";
            // 
            // tspgrLevel
            // 
            this.tspgrLevel.Name = "tspgrLevel";
            this.tspgrLevel.Size = new System.Drawing.Size(100, 16);
            // 
            // tsslblQuality
            // 
            this.tsslblQuality.Name = "tsslblQuality";
            this.tsslblQuality.Size = new System.Drawing.Size(51, 17);
            this.tsslblQuality.Text = "Quality :";
            // 
            // tspgrQuality
            // 
            this.tspgrQuality.Name = "tspgrQuality";
            this.tspgrQuality.Size = new System.Drawing.Size(100, 16);
            // 
            // axWindowsMediaPlayer1
            // 
            this.axWindowsMediaPlayer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axWindowsMediaPlayer1.Enabled = true;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(0, 0);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(700, 341);
            this.axWindowsMediaPlayer1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1055, 341);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "Sat>Ip Rtsp Sample";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        //private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private System.Windows.Forms.ListBox PlayList;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar tspgrLevel;
        private System.Windows.Forms.ToolStripProgressBar tspgrQuality;
        private System.Windows.Forms.ToolStripStatusLabel tsslblQuality;
        private System.Windows.Forms.ToolStripStatusLabel tsslblLevel;
        private System.Windows.Forms.ListBox listBox1;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
    }
}

