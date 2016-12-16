// BenLincoln.Shared
// Copyright 2006-2012 Ben Lincoln
// http://www.thelostworlds.net/
//

// This file is part of BenLincoln.Shared.

// BenLincoln.Shared is free software: you can redistribute it and/or modify
// it under the terms of version 3 of the GNU General Public License as published by
// the Free Software Foundation.

// BenLincoln.Shared is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with BenLincoln.Shared (in the file LICENSE.txt).  
// If not, see <http://www.gnu.org/licenses/>.


using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace BenLincoln.Shared
{
    public class RegistryValue
    {
        protected string _ValueName;
        protected RegistryValueKind _RegistryValueKind;

        #region Properties

        public string ValueName
        {
            get
            {
                return _ValueName;
            }
        }

        public RegistryValueKind RegistryValueKind
        {
            get
            {
                return _RegistryValueKind;
            }
        }

        #endregion

        public RegistryValue(string valueName)
        {
            _ValueName = valueName;
        }

        public virtual void WriteValue(RegistryKey key)
        {
        }
    }
}
