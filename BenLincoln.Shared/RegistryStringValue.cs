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
using BLS = BenLincoln.Shared;

namespace BenLincoln.Shared
{
    public class RegistryStringValue : BLS.RegistryValue
    {
        protected string _Value;

        #region Properties

        public string Value
        {
            get
            {
                return _Value;
            }
        }

        #endregion

        public RegistryStringValue(string valueName, string value)
            : base(valueName)
        {
            _RegistryValueKind = RegistryValueKind.String;
            _Value = value;
        }

        public override void WriteValue(RegistryKey key)
        {
            key.SetValue(_ValueName, _Value, _RegistryValueKind);
        }
    }
}
