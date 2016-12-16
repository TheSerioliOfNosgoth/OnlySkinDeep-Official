// BenLincoln.TheLostWorlds.CDTextures
// Copyright 2007-2012 Ben Lincoln
// http://www.thelostworlds.net/
//

// This file is part of BenLincoln.TheLostWorlds.CDTextures.

// BenLincoln.TheLostWorlds.CDTextures is free software: you can redistribute it and/or modify
// it under the terms of version 3 of the GNU General Public License as published by
// the Free Software Foundation.

// BenLincoln.TheLostWorlds.CDTextures is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with BenLincoln.TheLostWorlds.CDTextures (in the file LICENSE.txt).  
// If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using D3D = Microsoft.DirectX.Direct3D;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public enum TextureFileType
    {
        Unknown,
        SoulReaverPlaystation,
        SoulReaverPC,
        SoulReaverDreamcast,
        SoulReaver2Playstation2,
        SoulReaver2PC
    }

    public abstract class TextureFile
    {
        protected string _FilePath;
        protected TextureFileType _FileType;
        protected string _FileTypeName;
        protected FileInfo _FileInfo;
        protected int _TextureCount;

        protected bool _ErrorOccurred;
        protected string _LastErrorMessage;

        #region Properties

        public string FilePath
        {
            get
            {
                return _FilePath;
            }
        }

        public TextureFileType FileType
        {
            get
            {
                return _FileType;
            }
        }

        public string FileTypeName
        {
            get
            {
                return _FileTypeName;
            }
        }

        public FileInfo FileInfo
        {
            get
            {
                return _FileInfo;
            }
        }

        public int TextureCount
        {
            get
            {
                return _TextureCount;
            }
        }

        public bool ErrorOccurred
        {
            get
            {
                return _ErrorOccurred;
            }
        }

        public string LastErrorMessage
        {
            get
            {
                return _LastErrorMessage;
            }
        }

        #endregion

        public TextureFile(string path)
        {
            _ErrorOccurred = false;
            _LastErrorMessage = "";
            _FilePath = path;
            if (File.Exists(path))
            {
                try
                {
                    _FileInfo = new FileInfo(path);
                }
                catch (IOException iEx)
                {
                    throw new IOException("Unable to access the file '" + path + "'", iEx);
                }
            }
        }

        protected abstract int _GetTextureCount();
        // implemented in subclasses to handle specific file types

        public Size GetTextureSize(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException("Texture index cannot be negative.");
            }
            if (index > (_TextureCount - 1))
            {
                throw new IndexOutOfRangeException("Size of texture " + index.ToString() +
                    " was requested but the file '" + _FilePath + "' only contains enough data for " + _TextureCount.ToString() +
                    " textures.");
            }
            return _GetTextureSize(index);
        }

        protected abstract Size _GetTextureSize(int index);

        public String GetTextureName(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException("Texture index cannot be negative.");
            }
            if (index > (_TextureCount - 1))
            {
                throw new IndexOutOfRangeException("Name of texture " + index.ToString() +
                    " was requested but the file '" + _FilePath + "' only contains enough data for " + _TextureCount.ToString() +
                    " textures.");
            }
            return _GetTextureName(index);
        }

        protected abstract String _GetTextureName(int index);

        public D3D.Texture GetTexture(D3D.Device device, int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException("Texture index cannot be negative.");
            }
            if (index > (_TextureCount - 1))
            {
                throw new IndexOutOfRangeException("Texture " + index.ToString() +
                    " was requested but the file '" + _FilePath + "' only contains enough data for " + _TextureCount.ToString() +
                    " textures.");
            }
            return _GetTexture(device, index);
        }

        protected abstract D3D.Texture _GetTexture(D3D.Device device, int index);
        // implemented in subclasses to handle specific file types

        public Bitmap GetTextureAsBitmap(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException("Texture index cannot be negative.");
            }
            if (index > (_TextureCount - 1))
            {
                throw new IndexOutOfRangeException("Texture " + index.ToString() +
                    " was requested but the file '" + _FilePath + "' only contains enough data for " + _TextureCount.ToString() +
                    " textures.");
            }
            return _GetTextureAsBitmap(index);
        }

        protected abstract Bitmap _GetTextureAsBitmap(int index);
        // implemented in subclasses to handle specific file types

        public abstract MemoryStream GetDataAsStream(int index);
        // implemented in subclasses to handle specific file types

        public virtual void ExportFile(int index, string outPath)
        {
            _ErrorOccurred = false;
            _LastErrorMessage = "";
            try
            {
                MemoryStream iStream = GetDataAsStream(index);
                BinaryReader iReader = new BinaryReader(iStream);
                FileStream oStream = new FileStream(outPath,
                    FileMode.Create, FileAccess.Write);
                BinaryWriter oWriter = new BinaryWriter(oStream);
                for (int byteNum = 0; byteNum < iStream.Length; byteNum++)
                {
                    oWriter.Write(iReader.ReadByte());
                }

                oWriter.Close();
                oStream.Close();
                iReader.Close();
                iStream.Close();
            }
            catch (Exception ex)
            {
                _ErrorOccurred = true;
                _LastErrorMessage = ex.Message;
            }
        }

        // for threaded exports
        public virtual void ExportFileThreaded(object parms)
        {
            try
            {
                ArrayList parmList = (ArrayList)parms;
                ExportFile((int)parmList[0], (string)parmList[1]);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException("Passed object must be an arraylist with the first element being an int and the second a string.");
            }
        }

        public static TextureFileType GetFileType(string path)
        {
            string fileExtension = Path.GetExtension(path).ToLower();

            switch (fileExtension)
            {
                case ".vrm":
                    // determine if PC / Playstation / etc
                    return VRMTextureFile.GetVRMType(path);
                    break;
                case ".big":
                    return TextureFileType.SoulReaverPC;
                    break;
                case ".vq":
                    return TextureFileType.SoulReaverDreamcast;
                    break;
                case ".crm":
                    return TextureFileType.SoulReaverPlaystation;
                    break;
                default:
                    return TextureFileType.Unknown;
                    break;
            }
            return TextureFileType.Unknown;
        }
    }
}
