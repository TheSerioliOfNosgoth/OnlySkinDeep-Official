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
    public class SoulReaver2PCVRMTextureFile : VRMTextureFile
    {
        public SoulReaver2PCVRMTextureFile(string path)
            : base(path)
        {
            _FileType = TextureFileType.SoulReaver2PC;
            _FileTypeName = "Soul Reaver 2 / Defiance (PC) VRM";
            _HeaderLength = 32;
            _TextureCount = _GetTextureCount();
            GetTextureDefinitions();
        }

        protected void GetTextureDefinitions()
        {
            _TextureDefinitions = new VRMTextureDefinition[_TextureCount];

            try
            {
                long texStart = _HeaderLength;
                FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader iReader = new BinaryReader(iStream);
                for (int i = 0; i < _TextureCount; i++)
                {
                    _TextureDefinitions[i] = new VRMTextureDefinition();
                    _TextureDefinitions[i].Offset = texStart;
                    iStream.Seek(texStart, SeekOrigin.Begin);
                    _TextureDefinitions[i].Flags1 = iReader.ReadUInt16();
                    _TextureDefinitions[i].Type = iReader.ReadUInt16();
                    switch (_TextureDefinitions[i].Type)
                    {
                        case 3:
                            _TextureDefinitions[i].Format = VRMFormat.Uncompressed;
                            break;
                        case 5:
                            _TextureDefinitions[i].Format = VRMFormat.DXTC1;
                            break;
                        case 9:
                            _TextureDefinitions[i].Format = VRMFormat.DXTC5;
                            break;
                        default:
                            throw new NotImplementedException("Support for type '" + _TextureDefinitions[i].Type.ToString() +
                                "' files is not yet implemented.");
                            break;
                    }
                    _TextureDefinitions[i].Height = iReader.ReadUInt16();
                    _TextureDefinitions[i].Width = iReader.ReadUInt16();
                    uint dataSize = iReader.ReadUInt32();
                    _TextureDefinitions[i].Flags2 = iReader.ReadUInt32();
                    // the 16 in the next line of code is because the data size does not include the 
                    // texture definition information
                    _TextureDefinitions[i].Length = 16 + (long)dataSize;
                    texStart = texStart + _TextureDefinitions[i].Length;
                }
                iReader.Close();
                iStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error enumerating textures.", ex);
            }
        }

        protected override D3D.Texture _GetTexture(D3D.Device device, int index)
        {
            switch (_TextureDefinitions[index].Format)
            {
                case VRMFormat.Uncompressed:
                    {
                        Stream xStream = GetUncompressedDataAsStream(index);
                        return D3D.Texture.FromBitmap(device, (Bitmap)Bitmap.FromStream(xStream), D3D.Usage.None, D3D.Pool.Managed);
                    }
                default:
                    {
                        Stream xStream = GetDXTCDataAsStream(index);
                        D3D.ImageInformation xImageInfo = D3D.TextureLoader.ImageInformationFromStream(xStream);
                        xStream.Position = 0;
                        return D3D.TextureLoader.FromStream(
                            device,
                            xStream,
                            xImageInfo.Width,
                            xImageInfo.Height,
                            xImageInfo.MipLevels,
                            D3D.Usage.None,
                            xImageInfo.Format,
                            D3D.Pool.Managed,
                            D3D.Filter.None,
                            D3D.Filter.None,
                            0x00000000
                        );
                    }
            }
        }

        public override MemoryStream GetDataAsStream(int index)
        {
            switch (_TextureDefinitions[index].Format)
            {
                case VRMFormat.Uncompressed:
                    return GetUncompressedDataAsStream(index);
                    break;
                default:
                    return GetDXTCDataAsStream(index);
                    break;
            }
        }

        protected MemoryStream GetUncompressedDataAsStream(int index)
        {
            int streamLength = (int)(_TextureDefinitions[index].Length - 16);
            MemoryStream mStream = new MemoryStream(streamLength);
            BinaryWriter mWriter = new BinaryWriter(mStream);

            // get the actual texture data
            try
            {
                FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader iReader = new BinaryReader(iStream);

                iStream.Seek(_TextureDefinitions[index].Offset + 16, SeekOrigin.Begin);
                for (int byteNum = 0; byteNum < streamLength; byteNum++)
                {
                    mWriter.Write(iReader.ReadByte());
                }

                iReader.Close();
                iStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error reading texture data from file.", ex);
            }

            mStream.Seek(0, SeekOrigin.Begin);
            return mStream;
        }

        protected MemoryStream GetDXTCDataAsStream(int index)
        {
            int streamLength = (int)(_TextureDefinitions[index].Length + 128); // size of DXTC header
            MemoryStream mStream = new MemoryStream(streamLength);
            BinaryWriter mWriter = new BinaryWriter(mStream);

            byte[] dxtcZero = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            byte[] dxtcMagic = new byte[] { 0x44, 0x44, 0x53, 0x20 };
            byte[] dxtcSize = new byte[] { 0x7C, 0x00, 0x00, 0x00 }; // is always 124
            mWriter.Write(dxtcMagic);
            mWriter.Write(dxtcSize);

            byte[] dxtcFlags = new byte[] { 0x07, 0x10, 0x0A, 0x00 }; // ???
            mWriter.Write(dxtcFlags);
            //mWriter.Write(_TextureDefinitions[index].Flags1);
            //byte[] dxtcFlags1a = new byte[] { 0x0A, 0x00 };
            //mWriter.Write(dxtcFlags1a);

            uint width = (uint)_TextureDefinitions[index].Width;
            uint height = (uint)_TextureDefinitions[index].Height;
            mWriter.Write(width);
            mWriter.Write(height);

            // uint linearsize
            int blockSize = 16;
            switch (_TextureDefinitions[index].Format)
            {
                case VRMFormat.DXTC1:
                    blockSize = 8;
                    break;
            }
            uint linearSize = (uint)((width / 4) * (height / 4) * blockSize);
            mWriter.Write(linearSize);

            // uint depth
            mWriter.Write(dxtcZero);

            // uint mipmapcount
            mWriter.Write(_TextureDefinitions[index].Flags2);

            // 11 uints of zero
            for (int i = 0; i < 11; i++)
            {
                mWriter.Write(dxtcZero);
            }

            byte[] dxtcSize2 = new byte[] { 0x20, 0x00, 0x00, 0x00 }; // is always 32
            byte[] dxtcFlags2 = new byte[] { 0x04, 0x00, 0x00, 0x00 }; // ???
            mWriter.Write(dxtcSize2);
            mWriter.Write(dxtcFlags2);
            byte[] dxtcFourCC = new byte[0];
            switch (_TextureDefinitions[index].Format)
            {
                case VRMFormat.DXTC1:
                    dxtcFourCC = new byte[] { 0x44, 0x58, 0x54, 0x31 };
                    break;
                case VRMFormat.DXTC5:
                    dxtcFourCC = new byte[] { 0x44, 0x58, 0x54, 0x35 }; // 0x33 as last digit = DXT3
                    break;
            }
            mWriter.Write(dxtcFourCC);

            byte[] dxtcBPP = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            byte[] dxtcMaskRed = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            byte[] dxtcMaskGreen = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            byte[] dxtcMaskBlue = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            byte[] dxtcMaskAlpha = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            mWriter.Write(dxtcBPP);
            mWriter.Write(dxtcMaskRed);
            mWriter.Write(dxtcMaskGreen);
            mWriter.Write(dxtcMaskBlue);
            mWriter.Write(dxtcMaskAlpha);

            byte[] dxtcCaps1 = new byte[] { 0x08, 0x10, 0x40, 0x00 };
            byte[] dxtcCaps2 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            byte[] dxtcCaps3 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            byte[] dxtcCaps4 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            byte[] dxtcTextureStage = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            mWriter.Write(dxtcCaps1);
            mWriter.Write(dxtcCaps2);
            mWriter.Write(dxtcCaps3);
            mWriter.Write(dxtcCaps4);
            mWriter.Write(dxtcTextureStage);

            // get the actual texture data
            try
            {
                FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader iReader = new BinaryReader(iStream);

                iStream.Seek(_TextureDefinitions[index].Offset + 16, SeekOrigin.Begin);
                int length = (int)(_TextureDefinitions[index].Length - 16);
                for (int byteNum = 0; byteNum < length; byteNum++)
                {
                    mWriter.Write(iReader.ReadByte());
                }

                iReader.Close();
                iStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error reading texture data from file.", ex);
            }
            mStream.Seek(0, SeekOrigin.Begin);
            return mStream;
        }

        // for debugging
        public void WriteAllTextureData(string outFolder)
        {
            FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
            BinaryReader iReader = new BinaryReader(iStream);
            for (int i = 0; i <= _TextureDefinitions.GetUpperBound(0); i++)
            {
                FileStream oStream = new FileStream(outFolder + @"\Texture-" + String.Format("{0:0000}", i) + "-" + 
                    String.Format("{0:X2}", _TextureDefinitions[i].Type) + "-" +
                    String.Format("{0:X4}", _TextureDefinitions[i].Flags2) + "-" + 
                    String.Format("{0:0000}", _TextureDefinitions[i].Width) + "x" +
                    String.Format("{0:0000}", _TextureDefinitions[i].Height) +
                    ".dat",
                    FileMode.Create, FileAccess.Write);
                BinaryWriter oWriter = new BinaryWriter(oStream);

                iStream.Seek(_TextureDefinitions[i].Offset, SeekOrigin.Begin);
                for (int byteNum = 0; byteNum < _TextureDefinitions[i].Length; byteNum++)
                {
                    oWriter.Write(iReader.ReadByte());
                }

                oWriter.Close();
                oStream.Close();
            }
            iReader.Close();
            iStream.Close();
        }

        public override void ExportFile(int index, string outPath)
        {
            switch (_TextureDefinitions[index].Format)
            {
                case VRMFormat.DXTC1:
                    base.ExportFile(index, outPath);
                    break;
                case VRMFormat.DXTC5:
                    base.ExportFile(index, outPath);
                    break;
                case VRMFormat.Uncompressed:
                    _ErrorOccurred = false;
                    _LastErrorMessage = "";
                    try
                    {
                        Bitmap tex = GetUncompressedTextureAsBitmap(index);
                        tex.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex)
                    {
                        _ErrorOccurred = true;
                        _LastErrorMessage = ex.Message;
                    }
                    break;
            }
        }

        protected Bitmap GetUncompressedTextureAsBitmap(int index)
        {
            MemoryStream iStream = GetUncompressedDataAsStream(index);
            BinaryReader iReader = new BinaryReader(iStream);

            Bitmap tex = new Bitmap(_TextureDefinitions[index].Width, _TextureDefinitions[index].Height);

            for (int y = 0; y < tex.Height; y++)
            {
                for (int x = 0; x < tex.Width; x++)
                {
                    int blue = (int)iReader.ReadByte();
                    int green = (int)iReader.ReadByte();
                    int red = (int)iReader.ReadByte();
                    int alpha = (int)iReader.ReadByte();
                    tex.SetPixel(x, y, Color.FromArgb(alpha, red, green, blue));
                }
            }

            iReader.Close();
            iStream.Close();

            return tex;
        }

        protected override System.Drawing.Bitmap _GetTextureAsBitmap(int index)
        {
            switch (_TextureDefinitions[index].Format)
            {
                case VRMFormat.DXTC1:
                    // don't know of a way to do this without a hack involving rendering the texture to 
                    // a hidden panel and capturing that
                    throw new Exception("The method or operation is not implemented.");
                    break;
                case VRMFormat.DXTC5:
                    throw new Exception("The method or operation is not implemented.");
                    break;
                case VRMFormat.Uncompressed:
                    return GetUncompressedTextureAsBitmap(index);
                    break;
            }
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
