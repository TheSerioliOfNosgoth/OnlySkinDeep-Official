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
using D3D = Microsoft.DirectX.Direct3D;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public class SoulReaverPCTextureFile : BenLincoln.TheLostWorlds.CDTextures.SoulReaverMonolithicTextureFile
    {
        public SoulReaverPCTextureFile(string path)
            : base(path)
        {
            _FileType = TextureFileType.SoulReaverPC;
            _FileTypeName = "Soul Reaver (PC) Uncompressed ARGB 1555 Monolithic Texture File";
            _HeaderLength = 4096;
            _TextureLength = 131072;
            _TextureCount = _GetTextureCount();
        }

        protected override D3D.Texture _GetTexture(D3D.Device device, int index)
        {
            return D3D.Texture.FromBitmap(device, _GetTextureAsBitmap(index), 0, D3D.Pool.Managed);
        }

        protected override System.Drawing.Bitmap _GetTextureAsBitmap(int index)
        {
            int offset = _HeaderLength + (index * _TextureLength);
            if (offset > (_FileInfo.Length - _TextureLength))
            {
                throw new TextureFileException("An index was specified which resulted in an offset greater " +
                    "than the maximum allowed value.");
            }
            ushort iGT, jGT;
            ushort a, r, g, b, pixelData;
            int aFactor, rFactor, gFactor, bFactor;
            Bitmap retBitmap;
            Color colour;

            aFactor = 8;
            rFactor = 3;
            gFactor = 3;
            bFactor = 3;

            colour = new Color();
            retBitmap = new Bitmap(256, 256);

            try
            {
                FileStream inStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader inReader = new BinaryReader(inStream);
                inStream.Seek(offset, SeekOrigin.Begin);

                for (iGT = 0; iGT <= 255; iGT++)
                {
                    for (jGT = 0; jGT <= 255; jGT++)
                    {
                        pixelData = inReader.ReadUInt16();
                        a = pixelData;
                        r = pixelData;
                        g = pixelData;
                        b = pixelData;

                        //separate out the channels
                        a >>= 15;

                        r <<= 1;
                        r >>= 11;

                        g <<= 6;
                        g >>= 11;

                        b <<= 11;
                        b >>= 11;

                        if (a > 0)
                        {
                            a = (ushort)255;
                        }
                        r = (ushort)(r << rFactor);
                        g = (ushort)(g << gFactor);
                        b = (ushort)(b << bFactor);

                        colour = Color.FromArgb(a, r, g, b);
                        retBitmap.SetPixel(jGT, iGT, colour);
                    }
                }
                inReader.Close();
                inStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error reading the specified texture.", ex);
            }

            return retBitmap;
        }

        public override MemoryStream GetDataAsStream(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
