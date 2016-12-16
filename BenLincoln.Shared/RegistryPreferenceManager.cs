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


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using BLS = BenLincoln.Shared;

namespace BenLincoln.Shared
{
    public class RegistryPreferenceManager
    {
        // which key to store the values in
        protected string _RegistryKey;

        public RegistryPreferenceManager(string registryKey)
        {
            _RegistryKey = registryKey;
        }

        #region Registry I/O

        public ArrayList ReadDWordValuesFromRegistry()
        {
            RegistryKey regKey;
            regKey = Registry.CurrentUser;

            ArrayList prefs = new ArrayList();
            regKey = regKey.OpenSubKey(_RegistryKey, true);
            foreach (string valueName in regKey.GetValueNames())
            {
                if (regKey.GetValueKind(valueName) == RegistryValueKind.DWord)
                {
                    int readValue = (int)regKey.GetValue(valueName);
                    BLS.RegistryDWordValue value = new BLS.RegistryDWordValue(valueName, readValue);
                    prefs.Add(value);
                }
            }

            return prefs;
        }

        public void WriteDWordValuesToRegistry(ArrayList prefs)
        {
            RegistryKey regKey;
            regKey = Registry.CurrentUser;

            regKey = regKey.OpenSubKey(_RegistryKey, true);
            foreach (BLS.RegistryDWordValue value in prefs)
            {
                value.WriteValue(regKey);
            }
        }

        public ArrayList ReadStringValuesFromRegistry()
        {
            RegistryKey regKey;
            regKey = Registry.CurrentUser;

            ArrayList prefs = new ArrayList();
            regKey = regKey.OpenSubKey(_RegistryKey, true);
            foreach (string valueName in regKey.GetValueNames())
            {
                if (regKey.GetValueKind(valueName) == RegistryValueKind.String)
                {
                    string readValue = (string)regKey.GetValue(valueName);
                    BLS.RegistryStringValue value = new BLS.RegistryStringValue(valueName, readValue);
                    prefs.Add(value);
                }
            }

            return prefs;
        }

        public void WriteStringValuesToRegistry(ArrayList prefs)
        {
            RegistryKey regKey;
            regKey = Registry.CurrentUser;

            regKey = regKey.OpenSubKey(_RegistryKey, true);
            foreach (BLS.RegistryStringValue value in prefs)
            {
                value.WriteValue(regKey);
            }
        }

        protected RegistryKey CreateKeysRecursively(RegistryKey key, string[] path, int index)
        {
            RegistryKey subKey = key.OpenSubKey(path[index], true);
            if (subKey == null)
            {
                key.CreateSubKey(path[index]);
                subKey = key.OpenSubKey(path[index], true);
            }
            if (index < path.Length - 1)
            {
                subKey = CreateKeysRecursively(subKey, path, index + 1);
            }
            return subKey;
        }

        public void InitRegistryKeys()
        {
            RegistryKey regKey;
            regKey = Registry.CurrentUser;

            // create the key path recursively if it doesn't exist
            string[] pathSplit = _RegistryKey.Split('\\');

            regKey = CreateKeysRecursively(regKey, pathSplit, 0);
        }

        public void InitRegistryStringValues(ArrayList defaultPrefs)
        {
            RegistryKey regKey;
            regKey = Registry.CurrentUser;
            regKey = regKey.OpenSubKey(_RegistryKey, true);

            // get a hashtable of the value names from the key
            Hashtable existingValues = new Hashtable();
            foreach (string valueName in regKey.GetValueNames())
            {
                existingValues.Add(valueName, regKey.GetValue(valueName));
            }

            // create any keys that don't exist
            foreach (BLS.RegistryStringValue value in defaultPrefs)
            {
                if (!existingValues.Contains(value.ValueName))
                {
                     value.WriteValue(regKey);
                }
            }
        }

        public void InitRegistryDWordValues(ArrayList defaultPrefs)
        {
            RegistryKey regKey;
            regKey = Registry.CurrentUser;
            regKey = regKey.OpenSubKey(_RegistryKey, true);

            // get a hashtable of the value names from the key
            Hashtable existingValues = new Hashtable();
            foreach (string valueName in regKey.GetValueNames())
            {
                existingValues.Add(valueName, regKey.GetValue(valueName));
            }

            // create any keys that don't exist
            foreach (BLS.RegistryDWordValue value in defaultPrefs)
            {
                if (!existingValues.Contains(value.ValueName))
                {
                    value.WriteValue(regKey);
                }
            }
        }

        #endregion
    }
}
