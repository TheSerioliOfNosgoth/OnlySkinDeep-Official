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
    partial class frmAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.btnClose = new System.Windows.Forms.Button();
            this.lblSite1 = new System.Windows.Forms.Label();
            this.lblCreator = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblProduct = new System.Windows.Forms.Label();
            this.imgSplash = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgSplash)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(215, 321);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 23);
            this.btnClose.TabIndex = 27;
            this.btnClose.Text = "OK";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblSite1
            // 
            this.lblSite1.Location = new System.Drawing.Point(8, 295);
            this.lblSite1.Name = "lblSite1";
            this.lblSite1.Size = new System.Drawing.Size(502, 23);
            this.lblSite1.TabIndex = 26;
            this.lblSite1.Text = "http://www.thelostworlds.net/";
            this.lblSite1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCreator
            // 
            this.lblCreator.Location = new System.Drawing.Point(8, 271);
            this.lblCreator.Name = "lblCreator";
            this.lblCreator.Size = new System.Drawing.Size(502, 23);
            this.lblCreator.TabIndex = 25;
            this.lblCreator.Text = "Copyright 2007-2014 Ben Lincoln and Andrew Fradley";
            this.lblCreator.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblVersion
            // 
            this.lblVersion.Location = new System.Drawing.Point(8, 247);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(502, 23);
            this.lblVersion.TabIndex = 24;
            this.lblVersion.Text = "Version 0.1 (Alpha Release)";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblProduct
            // 
            this.lblProduct.Location = new System.Drawing.Point(8, 223);
            this.lblProduct.Name = "lblProduct";
            this.lblProduct.Size = new System.Drawing.Size(502, 23);
            this.lblProduct.TabIndex = 23;
            this.lblProduct.Text = "A tool for extracting textures from Crystal Dynamics games";
            this.lblProduct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // imgSplash
            // 
            this.imgSplash.Image = ((System.Drawing.Image)(resources.GetObject("imgSplash.Image")));
            this.imgSplash.Location = new System.Drawing.Point(10, 12);
            this.imgSplash.Name = "imgSplash";
            this.imgSplash.Size = new System.Drawing.Size(500, 204);
            this.imgSplash.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgSplash.TabIndex = 22;
            this.imgSplash.TabStop = false;
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 353);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblSite1);
            this.Controls.Add(this.lblCreator);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblProduct);
            this.Controls.Add(this.imgSplash);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(535, 391);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(535, 391);
            this.Name = "frmAbout";
            this.Text = "About Only Skin Deep";
            ((System.ComponentModel.ISupportInitialize)(this.imgSplash)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btnClose;
        internal System.Windows.Forms.Label lblSite1;
        internal System.Windows.Forms.Label lblCreator;
        internal System.Windows.Forms.Label lblVersion;
        internal System.Windows.Forms.Label lblProduct;
        private System.Windows.Forms.PictureBox imgSplash;
    }
}