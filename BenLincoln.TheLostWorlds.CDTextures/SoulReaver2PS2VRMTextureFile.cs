// BenLincoln.TheLostWorlds.CDTextures
// Copyright 2007-2014 Ben Lincoln
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
using BLPS2 = BenLincoln.Playstation2;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public class SoulReaver2PS2VRMTextureFile : VRMTextureFile
    {
        protected int _TextureBlockSize = 16;
        protected int _SubTextureHeaderLength = 16;
        protected bool _IsNTSCDemo = false;

        public SoulReaver2PS2VRMTextureFile(string path)
            : base(path)
        {
            _FileType = TextureFileType.SoulReaver2Playstation2;
            _FileTypeName = "Soul Reaver 2 / Defiance (PS2) VRM";
            _HeaderLength = 16;
            _TextureCount = _GetTextureCount();

            Exception exception = null;
            try
            {
                GetTextureDefinitions_Retail();
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (exception == null)
            {
                return;
            }

            try
            {
                _SubTextureHeaderLength = 0;
                _IsNTSCDemo = true;
                GetTextureDefinitions_NTSCDemo();
            }
            catch (Exception e)
            {
                // Throw the exception that was found earlier.
                // Maybe concatonate them to avoid confusion.
                throw (e);
            }
        }

        protected void GetTextureDefinitions_Retail()
        {
            _TextureDefinitions = new VRMTextureDefinition[_TextureCount];

            try
            {
                long texStart = _HeaderLength;
                FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader iReader = new BinaryReader(iStream);

                iStream.Seek(texStart, SeekOrigin.Begin);

                for (int textureNum = 0; textureNum < _TextureCount; textureNum++)
                {
                    _TextureDefinitions[textureNum] = new VRMTextureDefinition();
                    // get the texture-wide information
                    _TextureDefinitions[textureNum].Offset = iStream.Position;
                    //iStream.Seek(texStart, SeekOrigin.Begin);
                    _TextureDefinitions[textureNum].Flags1 = iReader.ReadUInt16();
                    byte val2 = iReader.ReadByte();
                    byte val3 = iReader.ReadByte();
                    //if (val3 == 0)
                    //{
                    //    throw new TextureFileException("Texture type was 0.");
                    //}
                    switch (val2)
                    {
                        case 0:
                            _TextureDefinitions[textureNum].BPP = 16;
                            break;
                        case 17:
                            _TextureDefinitions[textureNum].BPP = 32;
                            break;
                    }
                    _TextureDefinitions[textureNum].Type = val3;
                    switch (_TextureDefinitions[textureNum].Type)
                    {
                        case 0:
                            _TextureDefinitions[textureNum].Format = VRMFormat.PS2_ARGB;
                            break;
                        case 2:
                        // 6 seems to work too for PAL Raziel.
                        // Fallthrough.
                        case 6:
                            _TextureDefinitions[textureNum].Format = VRMFormat.PS2_8Bit_Indexed;
                            break;
                        default:
                            throw new NotImplementedException("Support for type '" + _TextureDefinitions[textureNum].Type.ToString() +
                                "' files is not yet implemented.");
                            break;
                    }
                    ushort val4 = iReader.ReadUInt16();
                    ushort val5 = iReader.ReadUInt16();
                    _TextureDefinitions[textureNum].Width = val4;
                    _TextureDefinitions[textureNum].Height = val5;
                    uint val6 = iReader.ReadUInt32();
                    byte val7a = iReader.ReadByte();
                    byte val7b = iReader.ReadByte();
                    ushort val7c = iReader.ReadUInt16();
                    uint val8 = iReader.ReadUInt32();
                    uint val9 = iReader.ReadUInt32();
                    uint val10 = iReader.ReadUInt32();
                    uint val11 = iReader.ReadUInt32();

                    int subTexCount = val7a + 1;

                    _TextureDefinitions[textureNum].SubTextures = new VRMSubTextureDefinition[subTexCount];

                    long subTextureOffset = iStream.Position;
                    long subTextureLength = 0;
                    uint subTextureType = 0;

                    int subSize = iReader.ReadUInt16();
                    iStream.Seek(2, SeekOrigin.Current);

                    switch (_TextureDefinitions[textureNum].Format)
                    {
                        case VRMFormat.PS2_8Bit_Indexed:
                            // get the palette first
                            long paletteSize = (long)(_TextureBlockSize * subSize);
                            subTextureLength = paletteSize + _SubTextureHeaderLength;
                            // not doing anything with this yet
                            uint paletteVal1 = iReader.ReadUInt32();
                            // skip the rest of the header
                            iStream.Seek(8, SeekOrigin.Current);

                            VRMPaletteDefinition pal = new VRMPaletteDefinition();
                            pal.Offset = subTextureOffset;
                            pal.Length = subTextureLength;
                            switch (paletteSize)
                            {
                                case 512:
                                    pal.BPP = 16;
                                    break;
                                case 1024:
                                    pal.BPP = 32;
                                    break;
                            }
                            _TextureDefinitions[textureNum].Palette = pal;
                            iStream.Seek(subTextureLength - _SubTextureHeaderLength, SeekOrigin.Current);
                            subTextureOffset = iStream.Position;
                            subSize = iReader.ReadUInt16();
                            iStream.Seek(2, SeekOrigin.Current);
                            break;
                        default:
                            break;
                    }

                    int mipMapWidth = _TextureDefinitions[textureNum].Width;
                    int mipMapHeight = _TextureDefinitions[textureNum].Height;
                    for (int subTexNum = 0; subTexNum < subTexCount; subTexNum++)
                    {
                        long subTextureSize = (long)(_TextureBlockSize * subSize);
                        subTextureLength = subTextureSize + _SubTextureHeaderLength;
                        uint subTextureVal1 = iReader.ReadUInt32();
                        subTextureType = subTextureVal1;
                        // skip the rest of the header
                        iStream.Seek(8, SeekOrigin.Current);

                        VRMSubTextureDefinition subTex = new VRMSubTextureDefinition();
                        subTex.Offset = subTextureOffset;
                        subTex.Length = subTextureLength;
                        subTex.Type = subTextureType;
                        subTex.Width = mipMapWidth;
                        subTex.Height = mipMapHeight;
                        _TextureDefinitions[textureNum].SubTextures[subTexNum] = subTex;

                        iStream.Seek(subTextureLength - _SubTextureHeaderLength, SeekOrigin.Current);
                        if (iStream.Position < iStream.Length)
                        {
                            subTextureOffset = iStream.Position;
                            subSize = iReader.ReadUInt16();
                            iStream.Seek(2, SeekOrigin.Current);
                            mipMapWidth = (mipMapWidth / 2);
                            mipMapHeight = (mipMapHeight / 2);
                        }
                    }
                    iStream.Seek(-4, SeekOrigin.Current);
                    _TextureDefinitions[textureNum].Length = iStream.Position - _TextureDefinitions[textureNum].Offset;
                }

                iReader.Close();
                iStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error enumerating textures.", ex);
            }
        }

        protected void GetTextureDefinitions_NTSCDemo()
        {
            _TextureDefinitions = new VRMTextureDefinition[_TextureCount];

            try
            {
                long texStart = _HeaderLength;
                FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader iReader = new BinaryReader(iStream);

                iStream.Seek(texStart, SeekOrigin.Begin);

                for (int textureNum = 0; textureNum < _TextureCount; textureNum++)
                {
                    _TextureDefinitions[textureNum] = new VRMTextureDefinition();
                    // get the texture-wide information
                    _TextureDefinitions[textureNum].Offset = iStream.Position;
                    //iStream.Seek(texStart, SeekOrigin.Begin);
                    _TextureDefinitions[textureNum].Flags1 = iReader.ReadUInt16();
                    byte val2 = iReader.ReadByte();
                    byte val3 = iReader.ReadByte();
                    //if (val3 == 0)
                    //{
                    //    throw new TextureFileException("Texture type was 0.");
                    //}
                    switch (val2)
                    {
                        case 0:
                            _TextureDefinitions[textureNum].BPP = 16;
                            break;
                        case 17:
                            _TextureDefinitions[textureNum].BPP = 32;
                            break;
                    }
                    _TextureDefinitions[textureNum].Type = val3;
                    switch (_TextureDefinitions[textureNum].Type)
                    {
                        case 0:
                            _TextureDefinitions[textureNum].Format = VRMFormat.PS2_ARGB;
                            break;
                        case 2:
                        // 6 seems to work too for PAL Raziel.
                        // Fallthrough.
                        case 6:
                            _TextureDefinitions[textureNum].Format = VRMFormat.PS2_8Bit_Indexed;
                            break;
                        default:
                            throw new NotImplementedException("Support for type '" + _TextureDefinitions[textureNum].Type.ToString() +
                                "' files is not yet implemented.");
                            break;
                    }
                    ushort val4 = iReader.ReadUInt16();
                    ushort val5 = iReader.ReadUInt16();
                    _TextureDefinitions[textureNum].Width = val4;
                    _TextureDefinitions[textureNum].Height = val5;
                    ushort val6a = iReader.ReadUInt16();
                    ushort val6b = iReader.ReadUInt16();
                    ushort val6c = iReader.ReadUInt16();
                    // (6a + 6b + 6c) * 4 = pallette length?
                    // ^^Probably not.  Looks like fixed length of 0x200.
                    ushort val7c = iReader.ReadUInt16();

                    // 0x4000 // 0x80 * 0x80 // (length * height) / 1
                    // 0x1000 // 0x40 * 0x40 // (length * height) / 2
                    // 0x0400 // 0x20 * 0x20 // (length * height) / 4
                    // 0x0100 // 0x10 * 0x10 // (length * height) / 8
                    // 0x0040 // 0x08 * 0x08 // (length * height) / 16
                    // 0x0010 // 0x04 * 0x04 // (length * height) / 32
                    // =
                    // 0x5550

                    //int subTexCount = (_TextureDefinitions[textureNum].Format == VRMFormat.PS2_8Bit_Indexed) ? val6c + 1 : 1;
                    // AMF 29/07/14 - Safe to use val6c + 1 for everything afterall?
                    int subTexCount = val6c + 1;

                    _TextureDefinitions[textureNum].SubTextures = new VRMSubTextureDefinition[subTexCount];

                    long subTextureOffset = iStream.Position;
                    long subTextureLength = 0;
                    uint subTextureType = 0;

                    switch (_TextureDefinitions[textureNum].Format)
                    {
                        case VRMFormat.PS2_8Bit_Indexed:
                            VRMPaletteDefinition pal = new VRMPaletteDefinition();
                            pal.Offset = subTextureOffset;
                            pal.Length = 0x200;
                            pal.BPP = 16;
                            iStream.Seek(pal.Length, SeekOrigin.Current);
                            //pal.Length = subTextureLength;
                            //switch (paletteSize)
                            //{
                            //    case 512:
                            //        pal.BPP = 16;
                            //        break;
                            //    case 1024:
                            //        pal.BPP = 32;
                            //        break;
                            //}
                            _TextureDefinitions[textureNum].Palette = pal;
                            subTextureOffset = iStream.Position;
                            break;
                        default:
                            break;
                    }

                    int mipMapWidth = _TextureDefinitions[textureNum].Width;
                    int mipMapHeight = _TextureDefinitions[textureNum].Height;
                    for (int subTexNum = 0; subTexNum < subTexCount; subTexNum++)
                    {
                        if (_TextureDefinitions[textureNum].Format == VRMFormat.PS2_8Bit_Indexed)
                        {
                            subTextureLength = mipMapWidth * mipMapHeight;
                        }
                        else
                        {
                            subTextureLength = (_TextureDefinitions[textureNum].BPP >> 3) * mipMapWidth * mipMapHeight;
                        }

                        VRMSubTextureDefinition subTex = new VRMSubTextureDefinition();
                        subTex.Offset = subTextureOffset;
                        subTex.Length = subTextureLength;
                        subTex.Type = subTextureType; // ?
                        subTex.Width = mipMapWidth;
                        subTex.Height = mipMapHeight;
                        _TextureDefinitions[textureNum].SubTextures[subTexNum] = subTex;

                        iStream.Seek(subTextureLength, SeekOrigin.Current);
                        if (iStream.Position < iStream.Length)
                        {
                            subTextureOffset = iStream.Position;
                            mipMapWidth = (mipMapWidth / 2);
                            mipMapHeight = (mipMapHeight / 2);
                        }
                    }
                    _TextureDefinitions[textureNum].Length = iStream.Position - _TextureDefinitions[textureNum].Offset;
                }

                iReader.Close();
                iStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error enumerating textures.", ex);
            }
        }

        public void WriteAllTextureData(string outFolder)
        {
            FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
            BinaryReader iReader = new BinaryReader(iStream);
            for (int i = 0; i <= _TextureDefinitions.GetUpperBound(0); i++)
            {
                switch (_TextureDefinitions[i].Format)
                {
                    case VRMFormat.PS2_8Bit_Indexed:
                        FileStream oStream = new FileStream(outFolder + @"\Texture-" + String.Format("{0:0000}", i) + "-" +
                            "Offset_" + String.Format("{0:00000000}", _TextureDefinitions[i].Offset) + "-" +
                            "Type_" + String.Format("{0:X4}", _TextureDefinitions[i].Type) + "-" +
                            String.Format("{0:0000}", _TextureDefinitions[i].Width) + "x" +
                            String.Format("{0:0000}", _TextureDefinitions[i].Height) + "-" +
                            "Palette-" +
                            "BPP_" + String.Format("{0:X4}", _TextureDefinitions[i].Palette.BPP.ToString()) + "-" +
                            "Offset_" + String.Format("{0:00000000}", _TextureDefinitions[i].Palette.Offset) +
                            ".dat",
                            FileMode.Create, FileAccess.Write);
                        BinaryWriter oWriter = new BinaryWriter(oStream);

                        iStream.Seek(_TextureDefinitions[i].Palette.Offset, SeekOrigin.Begin);
                        for (int byteNum = 0; byteNum < _TextureDefinitions[i].Palette.Length; byteNum++)
                        {
                            oWriter.Write(iReader.ReadByte());
                        }

                        oWriter.Close();
                        oStream.Close();
                        break;
                    default:
                        break;
                }
                for (int j = 0; j <= _TextureDefinitions[i].SubTextures.GetUpperBound(0); j++)
                {
                    FileStream oStream = new FileStream(outFolder + @"\Texture-" + String.Format("{0:0000}", i) + "-" +
                        "Offset_" + String.Format("{0:00000000}", _TextureDefinitions[i].Offset) + "-" +
                        "Type_" + String.Format("{0:X4}", _TextureDefinitions[i].Type) + "-" +
                        String.Format("{0:0000}", _TextureDefinitions[i].Width) + "x" +
                        String.Format("{0:0000}", _TextureDefinitions[i].Height) + "-" +
                        "MipMap-" + String.Format("{0:00}", j) + "-" +
                        String.Format("{0:0000}", _TextureDefinitions[i].SubTextures[j].Width) + "x" +
                        String.Format("{0:0000}", _TextureDefinitions[i].SubTextures[j].Height) + "-" +
                        "Type_" + String.Format("{0:X4}", _TextureDefinitions[i].SubTextures[j].Type) + "-" +
                        "Offset_" + String.Format("{0:00000000}", _TextureDefinitions[i].SubTextures[j].Offset) +
                        ".dat",
                        FileMode.Create, FileAccess.Write);
                    BinaryWriter oWriter = new BinaryWriter(oStream);

                    iStream.Seek(_TextureDefinitions[i].SubTextures[j].Offset, SeekOrigin.Begin);
                    for (int byteNum = 0; byteNum < _TextureDefinitions[i].SubTextures[j].Length; byteNum++)
                    {
                        oWriter.Write(iReader.ReadByte());
                    }

                    oWriter.Close();
                    oStream.Close();
                }
            }
            iReader.Close();
            iStream.Close();
        }

        protected override Microsoft.DirectX.Direct3D.Texture _GetTexture(Microsoft.DirectX.Direct3D.Device device, int index)
        {
            return D3D.Texture.FromBitmap(device, _GetTextureAsBitmap(index), 0, D3D.Pool.Managed);
        }

        public override void ExportFileThreaded(object parms)
        {
            try
            {
                ArrayList parmList = (ArrayList)parms;
                ExportFile((int)parmList[0], (int)parmList[1], (string)parmList[2]);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException("Passed object must be an arraylist with the first and second elements being an int and the third a string.");
            }
        }

        public void ExportFile(int index, int mipMapNumber, string outPath)
        {
            Bitmap tex = _GetTextureAsBitmap(index, mipMapNumber);
            tex.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        public override void ExportFile(int index, string outPath)
        {
            ExportFile(index, 0, outPath);
        }

        protected Bitmap _GetTextureAsBitmap(int index, int mipMapNumber)
        {
            switch (_TextureDefinitions[index].Format)
            {
                case VRMFormat.PS2_8Bit_Indexed:
                    return _GetIndexedTextureAsBitmap(index, mipMapNumber);
                    break;
                case VRMFormat.PS2_ARGB:
                    return _GetARGBTextureAsBitmap(index, mipMapNumber);
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }
        }

        protected override Bitmap _GetTextureAsBitmap(int index)
        {
            return _GetTextureAsBitmap(index, 0);
        }

        protected Bitmap _GetIndexedTextureAsBitmap(int index)
        {
            return _GetIndexedTextureAsBitmap(index, 0);
        }

        protected Bitmap _GetIndexedTextureAsBitmap(int index, int mipMapNumber)
        {
            Color[] tempPalette = new Color[256];
            Color[] palette = new Color[256];
            FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
            BinaryReader iReader = new BinaryReader(iStream);

            iStream.Seek(_TextureDefinitions[index].Palette.Offset + _SubTextureHeaderLength, SeekOrigin.Begin);
            for (int i = 0; i <= tempPalette.GetUpperBound(0); i++)
            {
                int alpha = 0;
                int red = 0;
                int green = 0;
                int blue = 0;
                switch (_TextureDefinitions[index].Palette.BPP)
                {
                    case 16:
                        ushort cData = iReader.ReadUInt16();
                        alpha = cData;
                        red = cData;
                        green = cData;
                        blue = cData;

                        //alpha = ((alpha & 0x8000) >> 15) << 7;
                        alpha = 255;
                        red = ((red & 0x1F)) << 3;
                        green = ((green & 0x3E0) >> 5) << 3;
                        blue = ((blue & 0x7C00) >> 10) << 3;
                        break;
                    case 32:
                        red = iReader.ReadByte();
                        green = iReader.ReadByte();
                        blue = iReader.ReadByte();
                        alpha = iReader.ReadByte();
                        if (alpha != 0)
                        {
                            alpha = 255;
                        }
                        break;
                }
                tempPalette[i] = Color.FromArgb(alpha, red, green, blue);
            }

            int[] clutMap = BLPS2.PS2ImageData.BuildIDTex8ClutIndex();

            for (int i = 0; i <= palette.GetUpperBound(0); i++)
            {
                palette[i] = tempPalette[clutMap[i]];
            }

            int width = _TextureDefinitions[index].SubTextures[mipMapNumber].Width;
            int height = _TextureDefinitions[index].SubTextures[mipMapNumber].Height;

            Bitmap image = new Bitmap(width, height);

            iStream.Seek(_TextureDefinitions[index].SubTextures[mipMapNumber].Offset + _SubTextureHeaderLength, SeekOrigin.Begin);
            byte[] rawData = iReader.ReadBytes((int)(_TextureDefinitions[index].SubTextures[mipMapNumber].Length -
                _SubTextureHeaderLength));
            iReader.Close();
            iStream.Close();

            byte[] unSwizzled = rawData;
            if (_IsNTSCDemo == false && width >= 16)
            {
                unSwizzled = BLPS2.PS2ImageData.PS2DefianceUnSwizzle(rawData, width, height, 8);
            }

            int byteNum = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image.SetPixel(x, y, palette[unSwizzled[byteNum]]);
                    byteNum++;
                }
            }

            return image;

        }

        protected Bitmap _GetARGBTextureAsBitmap(int index)
        {
            return _GetARGBTextureAsBitmap(index, 0);
        }

        protected Bitmap _GetARGBTextureAsBitmap(int index, int mipMapNumber)
        {
            FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
            BinaryReader iReader = new BinaryReader(iStream);

            int width = _TextureDefinitions[index].SubTextures[mipMapNumber].Width;
            int height = _TextureDefinitions[index].SubTextures[mipMapNumber].Height;

            Bitmap image = new Bitmap(width, height);

            iStream.Seek(_TextureDefinitions[index].SubTextures[mipMapNumber].Offset + _SubTextureHeaderLength, SeekOrigin.Begin);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int alpha = 0;
                    int red = 0;
                    int green = 0;
                    int blue = 0;
                    switch (_TextureDefinitions[index].BPP)
                    {
                        case 16:
                            ushort cData = iReader.ReadUInt16();
                            alpha = cData;
                            red = cData;
                            green = cData;
                            blue = cData;

                            alpha = ((alpha & 0x8000) >> 15) << 7;
                            red = ((red & 0x1F)) << 3;
                            green = ((green & 0x3E0) >> 5) << 3;
                            blue = ((blue & 0x7C00) >> 10) << 3;
                            break;
                        case 32:
                            red = iReader.ReadByte();
                            green = iReader.ReadByte();
                            blue = iReader.ReadByte();
                            alpha = iReader.ReadByte();
                            break;
                    }
                    image.SetPixel(x, y, Color.FromArgb(alpha, red, green, blue));
                }
            }

            iReader.Close();
            iStream.Close();

            return image;
        }

        public override MemoryStream GetDataAsStream(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
