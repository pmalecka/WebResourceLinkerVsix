namespace WebResourceLinkerVsix
{
    partial class WebResourcePublisher
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
            this.webresources = new System.Windows.Forms.TreeView();
            this.linkorpublish = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.StatusStrip();
            this.statusmsg = new System.Windows.Forms.ToolStripStatusLabel();
            this.connect = new System.Windows.Forms.Button();
            this.currentmapping = new System.Windows.Forms.Label();
            this.createnew = new System.Windows.Forms.LinkLabel();
            this.refresh = new System.Windows.Forms.PictureBox();
            this.status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.refresh)).BeginInit();
            this.SuspendLayout();
            // 
            // webresources
            // 
            this.webresources.Location = new System.Drawing.Point(12, 62);
            this.webresources.Name = "webresources";
            this.webresources.Size = new System.Drawing.Size(360, 264);
            this.webresources.TabIndex = 0;
            // 
            // linkorpublish
            // 
            this.linkorpublish.Location = new System.Drawing.Point(12, 332);
            this.linkorpublish.Name = "linkorpublish";
            this.linkorpublish.Size = new System.Drawing.Size(90, 30);
            this.linkorpublish.TabIndex = 1;
            this.linkorpublish.Text = "Link/Publish";
            this.linkorpublish.UseVisualStyleBackColor = true;
            this.linkorpublish.Click += new System.EventHandler(this.linkorpublish_Click);
            // 
            // status
            // 
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusmsg});
            this.status.Location = new System.Drawing.Point(0, 370);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(384, 22);
            this.status.SizingGrip = false;
            this.status.TabIndex = 2;
            this.status.Text = "statusStrip1";
            // 
            // statusmsg
            // 
            this.statusmsg.Name = "statusmsg";
            this.statusmsg.Size = new System.Drawing.Size(16, 17);
            this.statusmsg.Text = "...";
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(222, 332);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(150, 30);
            this.connect.TabIndex = 3;
            this.connect.Text = "Connect to different org.";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // currentmapping
            // 
            this.currentmapping.Location = new System.Drawing.Point(12, 9);
            this.currentmapping.Name = "currentmapping";
            this.currentmapping.Size = new System.Drawing.Size(359, 37);
            this.currentmapping.TabIndex = 4;
            this.currentmapping.Text = "...";
            // 
            // createnew
            // 
            this.createnew.AutoSize = true;
            this.createnew.BackColor = System.Drawing.SystemColors.Control;
            this.createnew.Location = new System.Drawing.Point(12, 46);
            this.createnew.Name = "createnew";
            this.createnew.Size = new System.Drawing.Size(137, 13);
            this.createnew.TabIndex = 5;
            this.createnew.TabStop = true;
            this.createnew.Text = "Create new web resource";
            this.createnew.VisitedLinkColor = System.Drawing.Color.Blue;
            this.createnew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.createnew_LinkClicked);
            // 
            // refresh
            // 
            this.refresh.BackColor = System.Drawing.SystemColors.Control;
            this.refresh.Image = global::WebResourceLinkerVsix.Properties.Resources._16x16_refresh;
            this.refresh.Location = new System.Drawing.Point(354, 44);
            this.refresh.Name = "refresh";
            this.refresh.Size = new System.Drawing.Size(17, 17);
            this.refresh.TabIndex = 6;
            this.refresh.TabStop = false;
            this.refresh.Click += new System.EventHandler(this.refresh_Click);
            // 
            // WebResourcePublisher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 392);
            this.Controls.Add(this.refresh);
            this.Controls.Add(this.createnew);
            this.Controls.Add(this.currentmapping);
            this.Controls.Add(this.connect);
            this.Controls.Add(this.status);
            this.Controls.Add(this.linkorpublish);
            this.Controls.Add(this.webresources);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "WebResourcePublisher";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dynamics CRM 2011 - Web Resource Linker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WebResourcePublisher_FormClosing);
            this.Load += new System.EventHandler(this.WebResourcePublisher_Load);
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.refresh)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView webresources;
        private System.Windows.Forms.Button linkorpublish;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.ToolStripStatusLabel statusmsg;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.Label currentmapping;
        private System.Windows.Forms.LinkLabel createnew;
        private System.Windows.Forms.PictureBox refresh;
    }
}