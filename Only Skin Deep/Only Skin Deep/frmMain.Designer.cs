// Only Skin Deep
// Copyright 2007-2012 Ben Lincoln
// http://www.thelostworlds.net/
//

// This file is part of Only Skin Deep.

// Only Skin Deep is free software: you can redistribute it and/or modify
// it under the terms of version 3 of the GNU General Public License as published by
// the Free Software Foundation.

// Only Skin Deep is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Only Skin Deep (in the file LICENSE.txt).  
// If not, see <http://www.gnu.org/licenses/>.

namespace Only_Skin_Deep
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.mnuMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchConvertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutOnlySkinDeepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnSave = new BenLincoln.UI.HoverImageButton();
            this.ilOpen = new System.Windows.Forms.ImageList(this.components);
            this.btnImportCurrent = new BenLincoln.UI.HoverImageButton();
            this.ilExportCurrent = new System.Windows.Forms.ImageList(this.components);
            this.btnExportCurrent = new BenLincoln.UI.HoverImageButton();
            this.btnExportRawData = new BenLincoln.UI.HoverImageButton();
            this.ilExportAll = new System.Windows.Forms.ImageList(this.components);
            this.btnExportAll = new BenLincoln.UI.HoverImageButton();
            this.btnOpen = new BenLincoln.UI.HoverImageButton();
            this.pnlInformation = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.lvTextureList = new Only_Skin_Deep.ListViewEx();
            this.chID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtInformation = new System.Windows.Forms.TextBox();
            this.mnuMenu.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlInformation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // mnuMenu
            // 
            this.mnuMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mnuMenu.Location = new System.Drawing.Point(0, 0);
            this.mnuMenu.Name = "mnuMenu";
            this.mnuMenu.Size = new System.Drawing.Size(484, 24);
            this.mnuMenu.TabIndex = 0;
            this.mnuMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exportCurrentToolStripMenuItem,
            this.exportAllToolStripMenuItem,
            this.batchConvertToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // exportCurrentToolStripMenuItem
            // 
            this.exportCurrentToolStripMenuItem.Name = "exportCurrentToolStripMenuItem";
            this.exportCurrentToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.exportCurrentToolStripMenuItem.Text = "Export Current";
            this.exportCurrentToolStripMenuItem.Click += new System.EventHandler(this.exportCurrentToolStripMenuItem_Click);
            // 
            // exportAllToolStripMenuItem
            // 
            this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            this.exportAllToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.exportAllToolStripMenuItem.Text = "Export &All";
            this.exportAllToolStripMenuItem.Click += new System.EventHandler(this.exportAllToolStripMenuItem_Click);
            // 
            // batchConvertToolStripMenuItem
            // 
            this.batchConvertToolStripMenuItem.Name = "batchConvertToolStripMenuItem";
            this.batchConvertToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.batchConvertToolStripMenuItem.Text = "&Batch Convert";
            this.batchConvertToolStripMenuItem.Click += new System.EventHandler(this.batchConvertToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutOnlySkinDeepToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutOnlySkinDeepToolStripMenuItem
            // 
            this.aboutOnlySkinDeepToolStripMenuItem.Name = "aboutOnlySkinDeepToolStripMenuItem";
            this.aboutOnlySkinDeepToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.aboutOnlySkinDeepToolStripMenuItem.Text = "&About Only Skin Deep";
            this.aboutOnlySkinDeepToolStripMenuItem.Click += new System.EventHandler(this.aboutOnlySkinDeepToolStripMenuItem_Click);
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Controls.Add(this.btnImportCurrent);
            this.pnlButtons.Controls.Add(this.btnExportCurrent);
            this.pnlButtons.Controls.Add(this.btnExportRawData);
            this.pnlButtons.Controls.Add(this.btnExportAll);
            this.pnlButtons.Controls.Add(this.btnOpen);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlButtons.Location = new System.Drawing.Point(0, 24);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(484, 66);
            this.pnlButtons.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ImageIndex = 1;
            this.btnSave.ImageList = this.ilOpen;
            this.btnSave.Location = new System.Drawing.Point(358, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(54, 54);
            this.btnSave.TabIndex = 3;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Visible = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // ilOpen
            // 
            this.ilOpen.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilOpen.ImageStream")));
            this.ilOpen.TransparentColor = System.Drawing.Color.Transparent;
            this.ilOpen.Images.SetKeyName(0, "Button-Open-Disabled.PNG");
            this.ilOpen.Images.SetKeyName(1, "Button-Open-Enabled.PNG");
            this.ilOpen.Images.SetKeyName(2, "Button-Open-Hover.PNG");
            // 
            // btnImportCurrent
            // 
            this.btnImportCurrent.FlatAppearance.BorderSize = 0;
            this.btnImportCurrent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportCurrent.ImageIndex = 1;
            this.btnImportCurrent.ImageList = this.ilExportCurrent;
            this.btnImportCurrent.Location = new System.Drawing.Point(418, 3);
            this.btnImportCurrent.Name = "btnImportCurrent";
            this.btnImportCurrent.Size = new System.Drawing.Size(54, 54);
            this.btnImportCurrent.TabIndex = 6;
            this.btnImportCurrent.UseVisualStyleBackColor = true;
            this.btnImportCurrent.Visible = false;
            this.btnImportCurrent.Click += new System.EventHandler(this.btnImportCurrent_Click);
            // 
            // ilExportCurrent
            // 
            this.ilExportCurrent.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilExportCurrent.ImageStream")));
            this.ilExportCurrent.TransparentColor = System.Drawing.Color.Transparent;
            this.ilExportCurrent.Images.SetKeyName(0, "Button-ExportCurrent-Disabled.PNG");
            this.ilExportCurrent.Images.SetKeyName(1, "Button-ExportCurrent-Enabled.PNG");
            this.ilExportCurrent.Images.SetKeyName(2, "Button-ExportCurrent-Hover.PNG");
            // 
            // btnExportCurrent
            // 
            this.btnExportCurrent.FlatAppearance.BorderSize = 0;
            this.btnExportCurrent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportCurrent.ImageIndex = 1;
            this.btnExportCurrent.ImageList = this.ilExportCurrent;
            this.btnExportCurrent.Location = new System.Drawing.Point(123, 3);
            this.btnExportCurrent.Name = "btnExportCurrent";
            this.btnExportCurrent.Size = new System.Drawing.Size(54, 54);
            this.btnExportCurrent.TabIndex = 5;
            this.btnExportCurrent.UseVisualStyleBackColor = true;
            this.btnExportCurrent.Click += new System.EventHandler(this.btnExportCurrent_Click);
            // 
            // btnExportRawData
            // 
            this.btnExportRawData.FlatAppearance.BorderSize = 0;
            this.btnExportRawData.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportRawData.ImageIndex = 1;
            this.btnExportRawData.ImageList = this.ilExportAll;
            this.btnExportRawData.Location = new System.Drawing.Point(183, 3);
            this.btnExportRawData.Name = "btnExportRawData";
            this.btnExportRawData.Size = new System.Drawing.Size(54, 54);
            this.btnExportRawData.TabIndex = 7;
            this.btnExportRawData.UseVisualStyleBackColor = true;
            this.btnExportRawData.Visible = false;
            this.btnExportRawData.Click += new System.EventHandler(this.btnExportRawData_Click);
            // 
            // ilExportAll
            // 
            this.ilExportAll.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilExportAll.ImageStream")));
            this.ilExportAll.TransparentColor = System.Drawing.Color.Transparent;
            this.ilExportAll.Images.SetKeyName(0, "Button-ExportAll-Disabled.PNG");
            this.ilExportAll.Images.SetKeyName(1, "Button-ExportAll-Enabled.PNG");
            this.ilExportAll.Images.SetKeyName(2, "Button-ExportAll-Hover.PNG");
            // 
            // btnExportAll
            // 
            this.btnExportAll.FlatAppearance.BorderSize = 0;
            this.btnExportAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportAll.ImageIndex = 1;
            this.btnExportAll.ImageList = this.ilExportAll;
            this.btnExportAll.Location = new System.Drawing.Point(63, 3);
            this.btnExportAll.Name = "btnExportAll";
            this.btnExportAll.Size = new System.Drawing.Size(54, 54);
            this.btnExportAll.TabIndex = 4;
            this.btnExportAll.UseVisualStyleBackColor = true;
            this.btnExportAll.Click += new System.EventHandler(this.btnExportAll_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.FlatAppearance.BorderSize = 0;
            this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpen.ImageIndex = 1;
            this.btnOpen.ImageList = this.ilOpen;
            this.btnOpen.Location = new System.Drawing.Point(3, 3);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(54, 54);
            this.btnOpen.TabIndex = 2;
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // pnlInformation
            // 
            this.pnlInformation.Controls.Add(this.splitContainer1);
            this.pnlInformation.Controls.Add(this.txtInformation);
            this.pnlInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInformation.Location = new System.Drawing.Point(0, 90);
            this.pnlInformation.Name = "pnlInformation";
            this.pnlInformation.Size = new System.Drawing.Size(484, 474);
            this.pnlInformation.TabIndex = 4;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lvTextureList);
            this.splitContainer1.Size = new System.Drawing.Size(484, 419);
            this.splitContainer1.SplitterDistance = 300;
            this.splitContainer1.TabIndex = 4;
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.Black;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(484, 300);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // lvTextureList
            // 
            this.lvTextureList.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.lvTextureList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chID,
            this.chName,
            this.chSize});
            this.lvTextureList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTextureList.FullRowSelect = true;
            this.lvTextureList.GridLines = true;
            this.lvTextureList.Location = new System.Drawing.Point(0, 0);
            this.lvTextureList.LockColumnSize = true;
            this.lvTextureList.MultiSelect = false;
            this.lvTextureList.Name = "lvTextureList";
            this.lvTextureList.Size = new System.Drawing.Size(484, 115);
            this.lvTextureList.TabIndex = 8;
            this.lvTextureList.UseCompatibleStateImageBehavior = false;
            this.lvTextureList.View = System.Windows.Forms.View.Details;
            // 
            // chID
            // 
            this.chID.Text = "ID";
            this.chID.Width = 50;
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 313;
            // 
            // chSize
            // 
            this.chSize.Text = "Size";
            this.chSize.Width = 100;
            // 
            // txtInformation
            // 
            this.txtInformation.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtInformation.Location = new System.Drawing.Point(0, 419);
            this.txtInformation.Multiline = true;
            this.txtInformation.Name = "txtInformation";
            this.txtInformation.ReadOnly = true;
            this.txtInformation.Size = new System.Drawing.Size(484, 55);
            this.txtInformation.TabIndex = 0;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 564);
            this.Controls.Add(this.pnlInformation);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.mnuMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mnuMenu;
            this.Name = "frmMain";
            this.Text = "Only Skin Deep";
            this.mnuMenu.ResumeLayout(false);
            this.mnuMenu.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlInformation.ResumeLayout(false);
            this.pnlInformation.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mnuMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutOnlySkinDeepToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem batchConvertToolStripMenuItem;
        private System.Windows.Forms.Panel pnlButtons;
        private BenLincoln.UI.HoverImageButton btnOpen;
        private BenLincoln.UI.HoverImageButton btnExportAll;
        private BenLincoln.UI.HoverImageButton btnExportCurrent;
        private System.Windows.Forms.ImageList ilOpen;
        private System.Windows.Forms.ImageList ilExportAll;
        private System.Windows.Forms.ImageList ilExportCurrent;
        private System.Windows.Forms.Panel pnlInformation;
        private System.Windows.Forms.TextBox txtInformation;
        private BenLincoln.UI.HoverImageButton btnExportRawData;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ListViewEx lvTextureList;
        private System.Windows.Forms.ColumnHeader chID;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chSize;
        private System.Windows.Forms.ToolStripMenuItem exportCurrentToolStripMenuItem;
        private BenLincoln.UI.HoverImageButton btnImportCurrent;
        private BenLincoln.UI.HoverImageButton btnSave;
        private System.Windows.Forms.PictureBox pictureBox;
    }
}

