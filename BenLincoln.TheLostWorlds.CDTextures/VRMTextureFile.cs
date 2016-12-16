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
    public enum VRMFormat
    {
        Uncompressed,
        DXTC1,
        DXTC5,
        PS2_8Bit_Indexed,
        PS2_ARGB
    }
    
    public struct VRMTextureDefinition
    {
        public long Offset;
        public long Length;
        public int BPP;
        public int Width;
        public int Height;
        public ushort Type;
        public ushort Flags1;
        public uint Flags2;
        public VRMFormat Format;
        public VRMPaletteDefinition Palette;
        public VRMSubTextureDefinition[] SubTextures;
    }

    public struct VRMPaletteDefinition
    {
        public long Offset;
        public long Length;
        public int BPP;
    }

    public struct VRMSubTextureDefinition
    {
        public long Offset;
        public long Length;
        public uint Type;
        public int Width;
        public int Height;
    }

    public abstract class VRMTextureFile : TextureFile
    {
        protected int _HeaderLength;
        protected VRMTextureDefinition[] _TextureDefinitions;

        #region Properties

        public VRMTextureDefinition[] TextureDefinitions
        {
            get
            {
                return _TextureDefinitions;
            }
        }

        #endregion

        public VRMTextureFile(string path)
            : base(path)
        {
        }

        protected override int _GetTextureCount()
        {
            if (_FileInfo.Length < (_HeaderLength))
            {
                throw new TextureFileException("The file '" + _FilePath + "' does not contain enough data for it to be " +
                    "a VRM texture file of type '" + _FileTypeName + "'.");
            }
            try
            {
                FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader iReader = new BinaryReader(iStream);
                ushort numTextures = iReader.ReadUInt16();
                iReader.Close();
                iStream.Close();
                return numTextures;
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error obtaining the number of textures in '" + _FilePath + "'.", ex);
            }
        }

        protected override Size _GetTextureSize(int index)
        {
            Size size = new Size();
            size.Width = TextureDefinitions[index].Width;
            size.Height = TextureDefinitions[index].Height;

            return size;
        }

        protected override String _GetTextureName(int index)
        {
            int guid = (int)TextureDefinitions[index].Flags1;
            String name = Path.GetFileNameWithoutExtension(FilePath) + "-" + String.Format("{0:0000}", guid);
            return name;
        }

        public static TextureFileType GetVRMType(string path)
        {
            try
            {
                FileStream iStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (iStream.Length < 32)
                {
                    iStream.Close();
                    return TextureFileType.Unknown;
                }
                BinaryReader iReader = new BinaryReader(iStream);

                iStream.Seek(16, SeekOrigin.Begin);

                ulong testVal = iReader.ReadUInt64();

                iReader.Close();
                iStream.Close();

                if (testVal == 0)
                {
                    return TextureFileType.SoulReaver2PC;
                }
                else
                {
                    return TextureFileType.SoulReaver2Playstation2;
                }
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error reading file '" + path + "' to determine type.", ex);
            }
            return TextureFileType.SoulReaver2PC;
        }
    }
}
