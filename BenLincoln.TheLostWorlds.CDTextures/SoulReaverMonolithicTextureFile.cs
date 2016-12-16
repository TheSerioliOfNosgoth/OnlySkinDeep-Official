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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public abstract class SoulReaverMonolithicTextureFile : TextureFile
    {
        protected int _HeaderLength;
        protected int _TextureLength;

        public SoulReaverMonolithicTextureFile(string path)
            : base(path)
        {
        }

        public override void ExportFile(int index, string outPath)
        {
            Bitmap tex = GetTextureAsBitmap(index);
            tex.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        protected override int _GetTextureCount()
        {
            if (_FileInfo.Length < (_HeaderLength + _TextureLength))
            {
                throw new TextureFileException("The file '" + _FilePath + "' does not contain enough data for it to be " +
                    "a texture file for the " + _FileTypeName + " version of Soul Reaver ("
                    + _HeaderLength + " byte header + " + _TextureLength + " bytes per texture).");
            }
            long textureCountLong;
            float textureCountFloat;
            textureCountLong = (_FileInfo.Length - _HeaderLength) / _TextureLength;
            textureCountFloat = ((float)_FileInfo.Length - (float)_HeaderLength) / (float)_TextureLength;
            if ((float)textureCountLong != textureCountFloat)
            {
                throw new TextureFileException("The file '" + _FilePath + "' does not appear to be a valid " +
                    "texture file for the " + _FileTypeName + " version of Soul Reaver. File lengths for this type should be " +
                    +_HeaderLength + " bytes plus a whole number multiple of " + _TextureLength + " bytes.");
            }
            return (int)textureCountLong;
        }

        protected override Size _GetTextureSize(int index)
        {
            Size size = new Size();
            size.Width = 256;
            size.Height = 256;

            return size;
        }

        protected override String _GetTextureName(int index)
        {
            String name = Path.GetFileNameWithoutExtension(FilePath) + "-" + String.Format("{0:0000}", index);
            return name;
        }
    }
}
