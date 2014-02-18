namespace HostApplication
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
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        	this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
        	this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
        	this.открытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.report = new System.Windows.Forms.ListBox();
        	this.contextMenu.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// notifyIcon1
        	// 
        	this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
        	this.notifyIcon1.ContextMenuStrip = this.contextMenu;
        	this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
        	this.notifyIcon1.Text = "Автообмен 4.0 Сервер";
        	this.notifyIcon1.Visible = true;
        	// 
        	// contextMenu
        	// 
        	this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.открытьToolStripMenuItem,
        	        	        	this.выходToolStripMenuItem});
        	this.contextMenu.Name = "contextMenu";
        	this.contextMenu.Size = new System.Drawing.Size(153, 70);
        	// 
        	// открытьToolStripMenuItem
        	// 
        	this.открытьToolStripMenuItem.Name = "открытьToolStripMenuItem";
        	this.открытьToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
        	this.открытьToolStripMenuItem.Text = "Открыть";
        	this.открытьToolStripMenuItem.Click += new System.EventHandler(this.открытьToolStripMenuItem_Click);
        	// 
        	// выходToolStripMenuItem
        	// 
        	this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
        	this.выходToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
        	this.выходToolStripMenuItem.Text = "Выход";
        	this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
        	// 
        	// report
        	// 
        	this.report.FormattingEnabled = true;
        	this.report.HorizontalScrollbar = true;
        	this.report.Location = new System.Drawing.Point(12, 12);
        	this.report.Name = "report";
        	this.report.Size = new System.Drawing.Size(260, 238);
        	this.report.TabIndex = 1;
        	// 
        	// Form1
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(284, 262);
        	this.Controls.Add(this.report);
        	this.Name = "Form1";
        	this.Text = "Автообмен 4.0 Сервер";
        	this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
        	this.Load += new System.EventHandler(this.Form1_Load);
        	this.Shown += new System.EventHandler(this.Form1_Shown);
        	this.Resize += new System.EventHandler(this.Form1_Resize);
        	this.contextMenu.ResumeLayout(false);
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem открытьToolStripMenuItem;
        private System.Windows.Forms.ListBox report;
    }
}

