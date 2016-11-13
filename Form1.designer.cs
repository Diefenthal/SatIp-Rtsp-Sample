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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("SatIp Server");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.Devices = new System.Windows.Forms.TreeView();
            this.PlayList = new System.Windows.Forms.ListBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslblLevel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tspgrLevel = new System.Windows.Forms.ToolStripProgressBar();
            this.tsslblQuality = new System.Windows.Forms.ToolStripStatusLabel();
            this.tspgrQuality = new System.Windows.Forms.ToolStripProgressBar();
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            this.SuspendLayout();
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
            this.splitContainer1.Size = new System.Drawing.Size(1122, 562);
            this.splitContainer1.SplitterDistance = 373;
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
            this.splitContainer2.Panel1.Controls.Add(this.Devices);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.PlayList);
            this.splitContainer2.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer2.Size = new System.Drawing.Size(373, 562);
            this.splitContainer2.SplitterDistance = 102;
            this.splitContainer2.TabIndex = 0;
            // 
            // Devices
            // 
            this.Devices.Location = new System.Drawing.Point(3, 3);
            this.Devices.Name = "Devices";
            treeNode1.Name = "Node0";
            treeNode1.Text = "SatIp Server";
            this.Devices.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.Devices.Size = new System.Drawing.Size(367, 97);
            this.Devices.TabIndex = 0;
            this.Devices.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.Devices_AfterSelect);
            // 
            // PlayList
            // 
            this.PlayList.DisplayMember = "Name";
            this.PlayList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PlayList.FormattingEnabled = true;
            this.PlayList.Location = new System.Drawing.Point(0, 0);
            this.PlayList.Margin = new System.Windows.Forms.Padding(0, 30, 0, 0);
            this.PlayList.Name = "PlayList";
            this.PlayList.Size = new System.Drawing.Size(373, 434);
            this.PlayList.TabIndex = 8;
            this.PlayList.SelectedIndexChanged += new System.EventHandler(this.PlayList_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslblLevel,
            this.tspgrLevel,
            this.tsslblQuality,
            this.tspgrQuality});
            this.statusStrip1.Location = new System.Drawing.Point(0, 434);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(373, 22);
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
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(745, 562);
            this.axWindowsMediaPlayer1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1122, 562);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Sat>Ip Rtsp Sample";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        //private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar tspgrLevel;
        private System.Windows.Forms.ToolStripProgressBar tspgrQuality;
        private System.Windows.Forms.ToolStripStatusLabel tsslblQuality;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private System.Windows.Forms.ToolStripStatusLabel tsslblLevel;
        private System.Windows.Forms.ListBox PlayList;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TreeView Devices;
    }
}

