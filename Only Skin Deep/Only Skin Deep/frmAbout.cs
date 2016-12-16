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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Only_Skin_Deep
{
    public partial class frmAbout : Form
    {
        protected int _VerMaj;
        protected int _VerMin;
        protected int _Build;
        //protected const string _VerComment = " (Beta)";
        protected const string _VerComment = "";

        public frmAbout()
        {
            InitializeComponent();

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            AssemblyName assemblyName = currentAssembly.GetName();

            _VerMaj = assemblyName.Version.Major;
            _VerMin = assemblyName.Version.Minor;
            _Build = assemblyName.Version.Build;
            lblVersion.Text = "Version " + _VerMaj + "." + _VerMin + _VerComment + ", Build " + _Build;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

    }
}