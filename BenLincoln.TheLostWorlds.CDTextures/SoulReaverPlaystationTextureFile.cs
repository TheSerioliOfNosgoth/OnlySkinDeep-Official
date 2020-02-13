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
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using D3D = Microsoft.DirectX.Direct3D;
using AMEX = AMF.ModelEx;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public class SoulReaverPlaystationTextureFile : BenLincoln.TheLostWorlds.CDTextures.TextureFile
    {
        public struct SoulReaverPlaystationPolygonTextureData
        {
            public int[] u;               // 0-255 each
            public int[] v;               // 0-255 each 
            public int textureID;          // 0-8
            public int paletteColumn;      // 0-32
            public int paletteRow;         // 0-255
        }

        protected byte[][] _TextureData;
        protected SoulReaverPlaystationPolygonTextureData[] _PolygonData;
        protected Bitmap[] _Textures;
        protected int _TotalWidth;
        protected int _TotalHeight;


        public SoulReaverPlaystationTextureFile(string path)
            : base(path)
        {
            _FileType = TextureFileType.SoulReaverPlaystation;
            _FileTypeName = "Soul Reaver (Playstation) Indexed Texture File";

            Point size = GetImageSize();
            _TotalWidth = size.X;
            _TotalHeight = size.Y;

            _TextureCount = _GetTextureCount();
            _Textures = new Bitmap[_TextureCount];
            _TextureData = ReadTextureData(LoadTextureData());

            string pcmFileName = Path.GetDirectoryName(FilePath) + @"\" +
                Path.GetFileNameWithoutExtension(FilePath) + ".pcm";
            if (File.Exists(pcmFileName))
            {
                try
                {
                    AMEX.SR1File meshFile = new AMEX.SR1File(pcmFileName, AMF.ModelEx.GexModelType.SoulReaverPlaystation);
                    // copy the polygon data for texturing
                    _PolygonData = new SoulReaverPlaystationPolygonTextureData[meshFile.polygonCount];
                    int polyNum = 0;
                    foreach (AMEX.ExPolygon poly in meshFile.polygons)
                    {
                        _PolygonData[polyNum].paletteColumn = poly.paletteColumn;
                        _PolygonData[polyNum].paletteRow = poly.paletteRow;
                        _PolygonData[polyNum].u = new int[3];
                        _PolygonData[polyNum].v = new int[3];
                        _PolygonData[polyNum].u[0] = (int)poly.v1.rawU;
                        _PolygonData[polyNum].u[1] = (int)poly.v2.rawU;
                        _PolygonData[polyNum].u[2] = (int)poly.v3.rawU;
                        _PolygonData[polyNum].v[0] = (int)poly.v1.rawV;
                        _PolygonData[polyNum].v[1] = (int)poly.v2.rawV;
                        _PolygonData[polyNum].v[2] = (int)poly.v3.rawV;
                        _PolygonData[polyNum].textureID = poly.material.textureID;
                        polyNum++;
                    }
                }
                catch (Exception)
                {
                    _PolygonData = null;
                }
            }

            //BuildTexturesFromPolygonData(_PolygonData, _PolygonData != null, true);

            for (int i = 0; i < _TextureCount; i++)
            {
                _Textures[i] = GetTextureAsBitmap(i, GetGreyscalePalette());
            }
        }

        protected override int _GetTextureCount()
        {
            if (_FileInfo.Length < (36))
            {
                throw new TextureFileException("The file '" + _FilePath + "' does not contain enough data for it to be " +
                    "a texture file for the Playstation version of Soul Reaver.");
            }
            long textureCountLong;
            float textureCountFloat;
            textureCountLong = (_FileInfo.Length - 36) / 32768;
            textureCountFloat = ((float)_FileInfo.Length - 36f) / 32768f;
            if ((float)textureCountLong != textureCountFloat)
            {
                textureCountLong++;
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

        protected byte[] LoadTextureData()
        {
            byte[] data;

            try
            {
                FileStream inStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader inReader = new BinaryReader(inStream);

                data = new byte[(_TotalWidth / 2) * _TotalHeight];

                inStream.Seek(36, SeekOrigin.Begin);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = inReader.ReadByte();
                }

                inReader.Close();
                inStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error reading texture.", ex);
            }

            return data;
        }

        protected byte[][] ReadTextureData(byte[] inData)
        {
            int numTextures = (inData.Length / 32768);
            int remainder = inData.Length % 32768;
            if (remainder > 0)
            {
                numTextures++;
            }
            byte[][] textures = new byte[numTextures][];
            for (int textureNum = 0; textureNum < numTextures; textureNum++)
            {
                textures[textureNum] = new byte[32768];
            }
            int byteNum = 0;
            for (int y = 0; y < _TotalHeight; y++)
            {
                for (int textureNum = 0; textureNum < numTextures; textureNum++)
                {
                    int startX = 128 * textureNum;
                    int endX = startX + 128;
                    int maxX = ((_TotalWidth / 2) - startX);
                    int padX = 0;
                    if (maxX < 128)
                    {
                        padX = 128 - maxX;
                        endX = startX + maxX;
                    }
                    for (int x = startX; x < endX; x++)
                    {
                        textures[textureNum][(y * 128) + (x - startX)] = inData[byteNum];
                        byteNum++;
                    }
                    for (int i = 0; i < padX; i++)
                    {
                        textures[textureNum][(y * 128) + (endX - startX) + i] = (byte)0;
                    }
                }
            }

            return textures;
        }

        public void BuildTextureFromPolygonData(int index, bool quantizeBounds)
        {
            // hashtable to store counts of palette usage
            Hashtable palettes = new Hashtable();

            // initialize texture
            Color chromaKey = Color.FromArgb(1, 128, 128, 128);
            _Textures[index] = new Bitmap(256, 256);
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    _Textures[index].SetPixel(x, y, chromaKey);
                }
            }

            #region Colour in from polygon data
            // use the polygon data to colour in all possible parts of the textures
            foreach (SoulReaverPlaystationPolygonTextureData poly in _PolygonData)
            {
                if (poly.textureID != index)
                {
                    continue;
                }

                // add or update palette list
                string paletteIDString = poly.paletteColumn.ToString() + "-" + poly.paletteRow.ToString();
                if (palettes.Contains(paletteIDString))
                {
                    int newPalCount = (int)palettes[paletteIDString];
                    newPalCount++;
                    palettes[paletteIDString] = newPalCount;
                }
                else
                {
                    int newPalCount = 1;
                    palettes.Add(paletteIDString, newPalCount);
                }

                int uMin = 255;
                int uMax = 0;
                int vMin = 255;
                int vMax = 0;
                Color[] palette = GetPalette(poly.paletteColumn, poly.paletteRow);
                // get the rectangle defined by the minimum and maximum U and V coords
                foreach (int u in poly.u)
                {
                    uMin = Math.Min(uMin, u);
                    uMax = Math.Max(uMax, u);
                }
                foreach (int v in poly.v)
                {
                    vMin = Math.Min(vMin, v);
                    vMax = Math.Max(vMax, v);
                }

                int width = uMax - uMin;
                for (int b = 0; b < 8; b++)
                {
                    if ((1 << b) >= width)
                    {
                        width = 1 << b;
                        break;
                    }
                }

                int height = vMax - vMin;
                for (int b = 0; b < 8; b++)
                {
                    if ((1 << b) >= height)
                    {
                        height = 1 << b;
                        break;
                    }
                }

                // if specified, quantize the rectangle's boundaries 
                if (quantizeBounds)
                {
                    while ((uMin % width) > 0)
                    {
                        uMin--;
                    }
                    while ((uMax % width) > 0)
                    {
                        uMax++;
                    }
                    while ((vMin % height) > 0)
                    {
                        vMin--;
                    }
                    while ((vMax % height) > 0)
                    {
                        vMax++;
                    }
                    if (uMin < 0)
                    {
                        uMin = 0;
                    }
                    if (uMax > 256)
                    {
                        uMax = 256;
                    }
                    if (vMin < 0)
                    {
                        vMin = 0;
                    }
                    if (vMax > 256)
                    {
                        vMax = 256;
                    }
                }

                //// if specified, quantize the rectangle's boundaries to a multiple of 16
                //if (quantizeBounds)
                //{
                //    int quantizeRes = 16;
                //    while ((uMin % quantizeRes) > 0)
                //    {
                //        uMin--;
                //    }
                //    while ((uMax % quantizeRes) > 0)
                //    {
                //        uMax++;
                //    }
                //    while ((vMin % quantizeRes) > 0)
                //    {
                //        vMin--;
                //    }
                //    while ((vMax % quantizeRes) > 0)
                //    {
                //        vMax++;
                //    }
                //    if (uMin < 0)
                //    {
                //        uMin = 0;
                //    }
                //    if (uMax > 256)
                //    {
                //        uMax = 256;
                //    }
                //    if (vMin < 0)
                //    {
                //        vMin = 0;
                //    }
                //    if (vMax > 256)
                //    {
                //        vMax = 256;
                //    }
                //}

                for (int y = vMin; y < vMax; y++)
                {
                    for (int x = uMin; x < uMax; x += 2)
                    {
                        int dataOffset = (y * 128) + (x / 2);
                        byte currentByte = _TextureData[index][dataOffset];
                        int leftPixel = (int)currentByte & 0x0F;
                        int rightPixel = (int)((currentByte & 0xF0) >> 4);
                        Color leftPixelColour = palette[leftPixel];
                        Color rightPixelColour = palette[rightPixel];
                        _Textures[index].SetPixel(x, y, leftPixelColour);
                        _Textures[index].SetPixel((x + 1), y, rightPixelColour);
                    }
                }
            }
            #endregion

            int texNum = 0;

            // find the most frequently-used palette
            string palID = "";
            int palCount = 0;
            foreach (string pID in palettes.Keys)
            {
                int pCount = (int)palettes[pID];
                if (pCount > palCount)
                {
                    palID = pID;
                }
            }
            string[] palDecode = palID.Split('-');
            int mostCommonPaletteColumn = int.Parse(palDecode[0]);
            int mostCommonPaletteRow = int.Parse(palDecode[1]);
            Color[] commonPalette = GetPalette(mostCommonPaletteColumn, mostCommonPaletteRow);

            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x += 2)
                {
                    bool leftPixChroma = (_Textures[index].GetPixel(x, y).A == 1);
                    bool rightPixChroma = (_Textures[index].GetPixel((x + 1), y).A == 1);
                    if (leftPixChroma || rightPixChroma)
                    {
                        int dataOffset = (y * 128) + (x / 2);
                        byte currentByte = _TextureData[index][dataOffset];
                        int leftPixel = (int)currentByte & 0x0F;
                        int rightPixel = (int)((currentByte & 0xF0) >> 4);
                        Color leftPixelColour = commonPalette[leftPixel];
                        Color rightPixelColour = commonPalette[rightPixel];
                        if (leftPixChroma)
                        {
                            _Textures[index].SetPixel(x, y, leftPixelColour);
                        }
                        if (rightPixChroma)
                        {
                            _Textures[index].SetPixel((x + 1), y, rightPixelColour);
                        }
                    }
                }
            }
        }

        public void BuildTexturesFromPolygonData(SoulReaverPlaystationPolygonTextureData[] texData, bool drawGreyScaleFirst, bool quantizeBounds)
        {
            // hashtable to store counts of palette usage
            Hashtable palettes = new Hashtable();

            // initialize textures
            if (drawGreyScaleFirst)
            {
                for (int i = 0; i < _TextureCount; i++)
                {
                    _Textures[i] = GetTextureAsBitmap(i, GetGreyscalePalette());
                }
            }
            else
            {
                Color chromaKey = Color.FromArgb(1, 128, 128, 128);
                for (int i = 0; i < _TextureCount; i++)
                {
                    _Textures[i] = new Bitmap(256, 256);
                    for (int y = 0; y < 256; y++)
                    {
                        for (int x = 0; x < 256; x++)
                        {
                            _Textures[i].SetPixel(x, y, chromaKey);
                        }
                    }
                }
            }

            #region Colour in from polygon data
            // use the polygon data to colour in all possible parts of the textures
            foreach (SoulReaverPlaystationPolygonTextureData poly in texData)
            {
                // add or update palette list
                string paletteIDString = poly.paletteColumn.ToString() + "-" + poly.paletteRow.ToString();
                if (palettes.Contains(paletteIDString))
                {
                    int newPalCount = (int)palettes[paletteIDString];
                    newPalCount++;
                    palettes[paletteIDString] = newPalCount;
                }
                else
                {
                    int newPalCount = 1;
                    palettes.Add(paletteIDString, newPalCount);
                }

                int uMin = 255;
                int uMax = 0;
                int vMin = 255;
                int vMax = 0;
                Color[] palette = GetPalette(poly.paletteColumn, poly.paletteRow);
                // get the rectangle defined by the minimum and maximum U and V coords
                foreach (int u in poly.u)
                {
                    uMin = Math.Min(uMin, u);
                    uMax = Math.Max(uMax, u);
                }
                foreach (int v in poly.v)
                {
                    vMin = Math.Min(vMin, v);
                    vMax = Math.Max(vMax, v);
                }

                int width = uMax - uMin;
                for (int b = 0; b < 8; b++)
                {
                    if ((1 << b) >= width)
                    {
                        width = 1 << b;
                        break;
                    }
                }

                int height = vMax - vMin;
                for (int b = 0; b < 8; b++)
                {
                    if ((1 << b) >= height)
                    {
                        height = 1 << b;
                        break;
                    }
                }

                // if specified, quantize the rectangle's boundaries 
                if (quantizeBounds)
                {
                    while ((uMin % width) > 0)
                    {
                        uMin--;
                    }
                    while ((uMax % width) > 0)
                    {
                        uMax++;
                    }
                    while ((vMin % height) > 0)
                    {
                        vMin--;
                    }
                    while ((vMax % height) > 0)
                    {
                        vMax++;
                    }
                    if (uMin < 0)
                    {
                        uMin = 0;
                    }
                    if (uMax > 256)
                    {
                        uMax = 256;
                    }
                    if (vMin < 0)
                    {
                        vMin = 0;
                    }
                    if (vMax > 256)
                    {
                        vMax = 256;
                    }
                }

                //// if specified, quantize the rectangle's boundaries to a multiple of 16
                //if (quantizeBounds)
                //{
                //    int quantizeRes = 16;
                //    while ((uMin % quantizeRes) > 0)
                //    {
                //        uMin--;
                //    }
                //    while ((uMax % quantizeRes) > 0)
                //    {
                //        uMax++;
                //    }
                //    while ((vMin % quantizeRes) > 0)
                //    {
                //        vMin--;
                //    }
                //    while ((vMax % quantizeRes) > 0)
                //    {
                //        vMax++;
                //    }
                //    if (uMin < 0)
                //    {
                //        uMin = 0;
                //    }
                //    if (uMax > 256)
                //    {
                //        uMax = 256;
                //    }
                //    if (vMin < 0)
                //    {
                //        vMin = 0;
                //    }
                //    if (vMax > 256)
                //    {
                //        vMax = 256;
                //    }
                //}

                for (int y = vMin; y < vMax; y++)
                {
                    for (int x = uMin; x < uMax; x+= 2)
                    {
                        int dataOffset = (y * 128) + (x / 2);
                        byte currentByte = _TextureData[poly.textureID][dataOffset];
                        int leftPixel = (int)currentByte & 0x0F;
                        int rightPixel = (int)((currentByte & 0xF0) >> 4);
                        Color leftPixelColour = palette[leftPixel];
                        Color rightPixelColour = palette[rightPixel];
                        _Textures[poly.textureID].SetPixel(x, y, leftPixelColour);
                        _Textures[poly.textureID].SetPixel((x + 1), y, rightPixelColour);
                    }
                }
            }
            #endregion

            int texNum = 0;

            if (!drawGreyScaleFirst)
            {
                // find the most frequently-used palette
                string palID = "";
                int palCount = 0;
                foreach (string pID in palettes.Keys)
                {
                    int pCount = (int)palettes[pID];
                    if (pCount > palCount)
                    {
                        palID = pID;
                    }
                }
                string[] palDecode = palID.Split('-');
                int mostCommonPaletteColumn = int.Parse(palDecode[0]);
                int mostCommonPaletteRow = int.Parse(palDecode[1]);
                Color[] commonPalette = GetPalette(mostCommonPaletteColumn, mostCommonPaletteRow);

                for (texNum = 0; texNum <= _Textures.GetUpperBound(0); texNum++)
                {
                    for (int y = 0; y < 256; y++)
                    {
                        for (int x = 0; x < 256; x += 2)
                        {
                            bool leftPixChroma = (_Textures[texNum].GetPixel(x, y).A == 1);
                            bool rightPixChroma = (_Textures[texNum].GetPixel((x + 1), y).A == 1);
                            if (leftPixChroma || rightPixChroma)
                            {
                                int dataOffset = (y * 128) + (x / 2);
                                byte currentByte = _TextureData[texNum][dataOffset];
                                int leftPixel = (int)currentByte & 0x0F;
                                int rightPixel = (int)((currentByte & 0xF0) >> 4);
                                Color leftPixelColour = commonPalette[leftPixel];
                                Color rightPixelColour = commonPalette[rightPixel];
                                if (leftPixChroma)
                                {
                                    _Textures[texNum].SetPixel(x, y, leftPixelColour);
                                }
                                if (rightPixChroma)
                                {
                                    _Textures[texNum].SetPixel((x + 1), y, rightPixelColour);
                                }
                            }
                        }
                    }
                }
            }

            // dump all textures as PNGs for debugging
            //texNum = 0;
            //foreach (Bitmap tex in _Textures)
            //{
            //    tex.Save(@"C:\Debug\Tex-" + texNum + ".png", ImageFormat.Png);
            //    texNum++;
            //}
        }

        protected override D3D.Texture _GetTexture(D3D.Device device, int index)
        {
            return D3D.Texture.FromBitmap(device, _GetTextureAsBitmap(index), 0, D3D.Pool.Managed);
        }

        protected override Bitmap _GetTextureAsBitmap(int index)
        {
            if (_Textures[index] == null)
            {
                if (_PolygonData != null)
                {
                    BuildTextureFromPolygonData(index, true);
                }
                else
                {
                    _Textures[index] = GetTextureAsBitmap(index, GetGreyscalePalette());
                }
            }

            return _Textures[index];
        }

        protected System.Drawing.Bitmap GetTextureAsBitmap(int index, Color[] palette)
        {
            Bitmap retBitmap = new Bitmap(256, 256);

            int byteNum = 0;
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x += 2)
                {
                    byte currentByte = _TextureData[index][byteNum];
                    int leftPixel = (int)currentByte & 0x0F;
                    int rightPixel = (int)((currentByte & 0xF0) >> 4);
                    retBitmap.SetPixel(x, y, palette[leftPixel]);
                    retBitmap.SetPixel(x + 1, y, palette[rightPixel]);
                    byteNum++;
                }
            }

            return retBitmap;
        }

        protected Point GetImageSize()
        {
            Point size;
            try
            {
                FileStream inStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader inReader = new BinaryReader(inStream);
                inStream.Seek(28, SeekOrigin.Begin);
                int totalWidth = (int)(inReader.ReadUInt32() * 4);
                int totalHeight = (int)inReader.ReadUInt32();
                inReader.Close();
                inStream.Close();
                size = new Point(totalWidth, totalHeight);
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error getting image size.", ex);
            }
            return size;
        }

        protected Color[] GetGreyscalePalette()
        {
            Color[] greyPalette = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                int luma = 16 * i;  // 256 / 16 = 16
                greyPalette[i] = Color.FromArgb(luma, luma, luma);
            }
            return greyPalette;
        }

        protected Color[] GetPalette(int paletteColumn, int paletteRow)
        {
            Color[] palette = new Color[16];

            int textureID = paletteColumn / 4;
            int localColumn = paletteColumn % 4;

            int realOffset = (localColumn * 32) + (paletteRow * 128);

            for (int i = 0; i < 16; i++)
            {
                int arrayPos = realOffset + (i * 2);
                ushort currentVal = (ushort)(
                    (ushort)(_TextureData[textureID][arrayPos]) |
                    ((ushort)(_TextureData[textureID][arrayPos + 1]) << 8)
                    );

                ushort alpha = 255;
                ushort red = currentVal;
                ushort green = currentVal;
                ushort blue = currentVal;

                alpha = (ushort)(currentVal >> 15);
                if (alpha != 0)
                {
                    alpha = 255;
                }

                blue = (ushort)(((ushort)(currentVal << 1) >> 11) << 3);
                green = (ushort)(((ushort)(currentVal << 6) >> 11) << 3);
                red = (ushort)(((ushort)(currentVal << 11) >> 11) << 3);

                palette[i] = Color.FromArgb(alpha, red, green, blue);
            }

            return palette;
        }

        public override MemoryStream GetDataAsStream(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void ExportFile(int index, string outPath)
        {
            Bitmap tex = new Bitmap(1, 1);
            if (_Textures[index] == null)
            {
                tex = GetTextureAsBitmap(index, GetGreyscalePalette());
            }
            else
            {
                tex = _Textures[index];
            }
            tex.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
