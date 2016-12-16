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
// along with The Mirror's Surface Breaks (in the file LICENSE.txt).  
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BenLincoln.Shared
{
    public class TempFile : IDisposable
    {
        private bool _Disposed = false;
        protected string _FilePath;
        protected FileStream _FileStream;

        #region Properties

        public string FilePath
        {
            get
            {
                return _FilePath;
            }
        }

        public FileStream FileStream
        {
            get
            {
                return _FileStream;
            }
        }

        #endregion

        public TempFile()
        {
            _FilePath = Path.GetTempFileName();
            _FileStream = new FileStream(_FilePath, FileMode.Create, FileAccess.ReadWrite);
        }

        public TempFile(string extension)
        {
            _FilePath = Path.GetTempFileName() + "." + extension;
            _FileStream = new FileStream(_FilePath, FileMode.Create, FileAccess.ReadWrite);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._Disposed)
            {
                // close the file stream and dispose of it
                _FileStream.Close();
                _FileStream.Dispose();

                // delete the temp file
                if (File.Exists(_FilePath))
                {
                    File.Delete(_FilePath);
                }
            }
        }

    }
}
