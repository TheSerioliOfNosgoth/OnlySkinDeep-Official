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
using System.Runtime.InteropServices;
using System.Text;
using D3D = Microsoft.DirectX.Direct3D;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct UInt32Bytes
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public UInt32 u;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public Byte b0;

        [System.Runtime.InteropServices.FieldOffset(1)]
        public Byte b1;

        [System.Runtime.InteropServices.FieldOffset(2)]
        public Byte b2;

        [System.Runtime.InteropServices.FieldOffset(3)]
        public Byte b3;

        public static implicit operator UInt32Bytes(Byte[] bytes)
        {
            UInt32Bytes data = new UInt32Bytes();
            data.b0 = bytes[0];
            data.b1 = bytes[1];
            data.b2 = bytes[2];
            data.b3 = bytes[3];
            return data;
        }

        public static implicit operator UInt32Bytes(UInt32 u)
        {
            UInt32Bytes data = new UInt32Bytes();
            data.u = u;
            return data;
        }
    }

    public class DDSTextureHeader
    {
        public UInt32Bytes dxtcZero;

        public UInt32Bytes dxtcMagic;
        public UInt32Bytes dxtcSize;

        public UInt32Bytes dxtcFlags;

        public UInt32 dxtcWidth;
        public UInt32 dxtcHeight;

        public UInt32 dxtcLinearSize;
        public UInt32 dxtcDepth;
        public UInt32 dxtcMipMapCount;

        public UInt32Bytes dxtcSize2;
        public UInt32Bytes dxtcFlags2;

        public UInt32Bytes dxtcFourCC;

        public UInt32Bytes dxtcBPP;
        public UInt32Bytes dxtcMaskRed;
        public UInt32Bytes dxtcMaskGreen;
        public UInt32Bytes dxtcMaskBlue;
        public UInt32Bytes dxtcMaskAlpha;

        public UInt32Bytes dxtcCaps1;
        public UInt32Bytes dxtcCaps2;
        public UInt32Bytes dxtcCaps3;
        public UInt32Bytes dxtcCaps4;
        public UInt32Bytes dxtcTextureStage;

        public void Initialise(UInt32 width, UInt32 height, VRMFormat format, UInt32 flags2)
        {
            dxtcZero = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            dxtcMagic = new byte[] { 0x44, 0x44, 0x53, 0x20 };
            dxtcSize = new byte[] { 0x7C, 0x00, 0x00, 0x00 }; // is always 124
            dxtcFlags = new byte[] { 0x07, 0x10, 0x0A, 0x00 }; // ???

            // OR these together for flags.
            // From http://msdn.microsoft.com/en-us/library/windows/desktop/bb943991%28v=vs.85%29.aspx
            // DDSD_CAPS	    Required in every .dds file.	                                0x1
            // DDSD_HEIGHT	    Required in every .dds file.	                                0x2
            // DDSD_WIDTH	    Required in every .dds file.	                                0x4
            // DDSD_PITCH	    Required when pitch is provided for an uncompressed texture.	0x8
            // DDSD_PIXELFORMAT Required in every .dds file.	                                0x1000
            // DDSD_MIPMAPCOUNT Required in a mipmapped texture.	                            0x20000
            // DDSD_LINEARSIZE  Required when pitch is provided for a compressed texture.	    0x80000
            // DDSD_DEPTH	    Required in a depth texture.	                                0x800000

            // My edited glyph texture has no linear size, but apparantly we can just calculate it.
            // Safe to ignore in validate function?

            dxtcWidth = width;
            dxtcHeight = height;
            int blockSize = 16;
            switch (format)
            {
                case VRMFormat.DXTC1:
                    blockSize = 8;
                    break;
            }
            dxtcLinearSize = (uint)((dxtcWidth / 4) * (dxtcHeight / 4) * blockSize);
            dxtcDepth = 0;
            dxtcMipMapCount = flags2;
            dxtcSize2 = new byte[] { 0x20, 0x00, 0x00, 0x00 }; // is always 32
            dxtcFlags2 = new byte[] { 0x04, 0x00, 0x00, 0x00 }; // ???
            dxtcFourCC = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            switch (format)
            {
                case VRMFormat.DXTC1:
                    dxtcFourCC = new byte[] { 0x44, 0x58, 0x54, 0x31 };
                    break;
                case VRMFormat.DXTC5:
                    dxtcFourCC = new byte[] { 0x44, 0x58, 0x54, 0x35 }; // 0x33 as last digit = DXT3
                    break;
            }
            dxtcBPP = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            dxtcMaskRed = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            dxtcMaskGreen = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            dxtcMaskBlue = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            dxtcMaskAlpha = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            dxtcCaps1 = new byte[] { 0x08, 0x10, 0x40, 0x00 };
            dxtcCaps2 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            dxtcCaps3 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            dxtcCaps4 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            dxtcTextureStage = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        }

        public bool Validate()
        {
            // bool isPowerOfTwo = (x != 0) && ((x & (x - 1)) == 0);
            if ((dxtcWidth < 0) || ((dxtcWidth & (dxtcWidth - 1)) != 0) ||
                (dxtcHeight < 0) || ((dxtcHeight & (dxtcHeight - 1)) != 0))
            {
                return false;
            }

            VRMFormat format = VRMFormat.Uncompressed;
            switch (dxtcFourCC.b3)
            {
                case 0x31:
                    format = VRMFormat.DXTC1;
                    break;
                case 0x35:
                    format = VRMFormat.DXTC5;
                    break;
                default:
                    return false;
            }

            DDSTextureHeader ddsHeader = new DDSTextureHeader();
            ddsHeader.Initialise(
                dxtcWidth,
                dxtcHeight,
                format,
                dxtcMipMapCount
            );

            bool isValid = true;

            isValid &= (dxtcMagic.u == ddsHeader.dxtcMagic.u);
            isValid &= (dxtcSize.u == ddsHeader.dxtcSize.u);
            // My edited glyph texture has no linear size, but apparantly we can just calculate it.
            // Safe to ignore in validate function?
            // isValid &= (dxtcFlags.u == ddsHeader.dxtcFlags.u);
            isValid &= (dxtcWidth == ddsHeader.dxtcWidth);
            isValid &= (dxtcHeight == ddsHeader.dxtcHeight);
            // isValid &= (dxtcLinearSize == ddsHeader.dxtcLinearSize);
            isValid &= (dxtcDepth == ddsHeader.dxtcDepth);
            isValid &= (dxtcMipMapCount == ddsHeader.dxtcMipMapCount);
            isValid &= (dxtcSize2.u == ddsHeader.dxtcSize2.u);
            isValid &= (dxtcFlags2.u == ddsHeader.dxtcFlags2.u);
            isValid &= (dxtcFourCC.u == ddsHeader.dxtcFourCC.u);
            isValid &= (dxtcBPP.u == ddsHeader.dxtcBPP.u);
            isValid &= (dxtcMaskRed.u == ddsHeader.dxtcMaskRed.u);
            isValid &= (dxtcMaskGreen.u == ddsHeader.dxtcMaskGreen.u);
            isValid &= (dxtcMaskBlue.u == ddsHeader.dxtcMaskBlue.u);
            isValid &= (dxtcMaskAlpha.u == ddsHeader.dxtcMaskAlpha.u);
            isValid &= (dxtcCaps1.u == ddsHeader.dxtcCaps1.u);
            isValid &= (dxtcCaps2.u == ddsHeader.dxtcCaps2.u);
            isValid &= (dxtcCaps3.u == ddsHeader.dxtcCaps3.u);
            isValid &= (dxtcCaps4.u == ddsHeader.dxtcCaps4.u);
            isValid &= (dxtcTextureStage.u == ddsHeader.dxtcTextureStage.u);

            if (isValid)
            {
                return true;
            }

            return false;
        }

        public void FromTextureDef(ref VRMTextureDefinition textureDef)
        {
            Initialise(
                (uint)textureDef.Width,
                (uint)textureDef.Height,
                textureDef.Format,
                textureDef.Flags2
            );
        }

        public void WriteToSteam(BinaryWriter mWriter)
        {
            if (mWriter == null)
            {
                return;
            }

            mWriter.Write(dxtcMagic.u);
            mWriter.Write(dxtcSize.u);
            mWriter.Write(dxtcFlags.u);
            mWriter.Write(dxtcWidth);
            mWriter.Write(dxtcHeight);
            mWriter.Write(dxtcLinearSize);
            mWriter.Write(dxtcDepth);
            mWriter.Write(dxtcMipMapCount);
            // 11 uints of zero
            for (int i = 0; i < 11; i++)
            {
                mWriter.Write(dxtcZero.u);
            }
            mWriter.Write(dxtcSize2.u);
            mWriter.Write(dxtcFlags2.u);
            mWriter.Write(dxtcFourCC.u);
            mWriter.Write(dxtcBPP.u);
            mWriter.Write(dxtcMaskRed.u);
            mWriter.Write(dxtcMaskGreen.u);
            mWriter.Write(dxtcMaskBlue.u);
            mWriter.Write(dxtcMaskAlpha.u);
            mWriter.Write(dxtcCaps1.u);
            mWriter.Write(dxtcCaps2.u);
            mWriter.Write(dxtcCaps3.u);
            mWriter.Write(dxtcCaps4.u);
            mWriter.Write(dxtcTextureStage.u);
        }

        public void ReadFromStream(BinaryReader mReader)
        {
            if (mReader == null)
            {
                return;
            }

            dxtcMagic.u = mReader.ReadUInt32();
            dxtcSize.u = mReader.ReadUInt32();
            dxtcFlags.u = mReader.ReadUInt32();
            dxtcWidth = mReader.ReadUInt32();
            dxtcHeight = mReader.ReadUInt32();
            dxtcLinearSize = mReader.ReadUInt32();
            dxtcDepth = mReader.ReadUInt32();
            dxtcMipMapCount = mReader.ReadUInt32();
            // 11 uints of zero
            for (int i = 0; i < 11; i++)
            {
                mReader.ReadUInt32(); //dxtcZero.u
            }
            dxtcSize2.u = mReader.ReadUInt32();
            dxtcFlags2.u = mReader.ReadUInt32();
            dxtcFourCC.u = mReader.ReadUInt32();
            dxtcBPP.u = mReader.ReadUInt32();
            dxtcMaskRed.u = mReader.ReadUInt32();
            dxtcMaskGreen.u = mReader.ReadUInt32();
            dxtcMaskBlue.u = mReader.ReadUInt32();
            dxtcMaskAlpha.u = mReader.ReadUInt32();
            dxtcCaps1.u = mReader.ReadUInt32();
            dxtcCaps2.u = mReader.ReadUInt32();
            dxtcCaps3.u = mReader.ReadUInt32();
            dxtcCaps4.u = mReader.ReadUInt32();
            dxtcTextureStage.u = mReader.ReadUInt32();
        }
    }

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

            DDSTextureHeader textureHeader = new DDSTextureHeader();
            textureHeader.FromTextureDef(ref _TextureDefinitions[index]);
            textureHeader.WriteToSteam(mWriter);

            //byte[] dxtcZero = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            //byte[] dxtcMagic = new byte[] { 0x44, 0x44, 0x53, 0x20 };
            //byte[] dxtcSize = new byte[] { 0x7C, 0x00, 0x00, 0x00 }; // is always 124
            //mWriter.Write(dxtcMagic);
            //mWriter.Write(dxtcSize);

            //byte[] dxtcFlags = new byte[] { 0x07, 0x10, 0x0A, 0x00 }; // ???
            //mWriter.Write(dxtcFlags);
            ////mWriter.Write(_TextureDefinitions[index].Flags1);
            ////byte[] dxtcFlags1a = new byte[] { 0x0A, 0x00 };
            ////mWriter.Write(dxtcFlags1a);

            //uint width = (uint)_TextureDefinitions[index].Width;
            //uint height = (uint)_TextureDefinitions[index].Height;
            //mWriter.Write(width);
            //mWriter.Write(height);

            //// uint linearsize
            //int blockSize = 16;
            //switch (_TextureDefinitions[index].Format)
            //{
            //    case VRMFormat.DXTC1:
            //        blockSize = 8;
            //        break;
            //}
            //uint linearSize = (uint)((width / 4) * (height / 4) * blockSize);
            //mWriter.Write(linearSize);

            //// uint depth
            //mWriter.Write(dxtcZero);

            //// uint mipmapcount
            //mWriter.Write(_TextureDefinitions[index].Flags2);

            //// 11 uints of zero
            //for (int i = 0; i < 11; i++)
            //{
            //    mWriter.Write(dxtcZero);
            //}

            //byte[] dxtcSize2 = new byte[] { 0x20, 0x00, 0x00, 0x00 }; // is always 32
            //byte[] dxtcFlags2 = new byte[] { 0x04, 0x00, 0x00, 0x00 }; // ???
            //mWriter.Write(dxtcSize2);
            //mWriter.Write(dxtcFlags2);
            //byte[] dxtcFourCC = new byte[0];
            //switch (_TextureDefinitions[index].Format)
            //{
            //    case VRMFormat.DXTC1:
            //        dxtcFourCC = new byte[] { 0x44, 0x58, 0x54, 0x31 };
            //        break;
            //    case VRMFormat.DXTC5:
            //        dxtcFourCC = new byte[] { 0x44, 0x58, 0x54, 0x35 }; // 0x33 as last digit = DXT3
            //        break;
            //}
            //mWriter.Write(dxtcFourCC);

            //byte[] dxtcBPP = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            //byte[] dxtcMaskRed = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            //byte[] dxtcMaskGreen = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            //byte[] dxtcMaskBlue = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            //byte[] dxtcMaskAlpha = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
            //mWriter.Write(dxtcBPP);
            //mWriter.Write(dxtcMaskRed);
            //mWriter.Write(dxtcMaskGreen);
            //mWriter.Write(dxtcMaskBlue);
            //mWriter.Write(dxtcMaskAlpha);

            //byte[] dxtcCaps1 = new byte[] { 0x08, 0x10, 0x40, 0x00 };
            //byte[] dxtcCaps2 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            //byte[] dxtcCaps3 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            //byte[] dxtcCaps4 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            //byte[] dxtcTextureStage = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            //mWriter.Write(dxtcCaps1);
            //mWriter.Write(dxtcCaps2);
            //mWriter.Write(dxtcCaps3);
            //mWriter.Write(dxtcCaps4);
            //mWriter.Write(dxtcTextureStage);

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

        public override void ImportFile(int index, string inPath)
        {
            if (index < 0 || index >= _TextureCount)
            {
                return;
            }

            DDSTextureHeader textureHeader = new DDSTextureHeader();
            bool isTextureValid = false;

            try
            {
                FileStream iStreamIn = new FileStream(inPath, FileMode.Open, FileAccess.Read);
                BinaryReader iReaderIn = new BinaryReader(iStreamIn);

                textureHeader.ReadFromStream(iReaderIn);
                isTextureValid = textureHeader.Validate();

                iReaderIn.Close();
                iStreamIn.Close();
            }
            catch (Exception) { }
            
            if (!isTextureValid)
            {
                return;
            }

            _TextureDefinitions[index].NewTexturePath = inPath;
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

        public override void ExportArchiveFile(string destPath)
        {
            FileStream iStreamSource = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
            BinaryReader iReaderSource = new BinaryReader(iStreamSource);

            FileStream iStreamDest = new FileStream(destPath, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter iWriterDest = new BinaryWriter(iStreamDest);

            iWriterDest.Write((short)_TextureCount);
            iWriterDest.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            iWriterDest.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            iWriterDest.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            iWriterDest.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            for (int i = 0; i < _TextureCount; i++)
            {
                if (_TextureDefinitions[i].NewTexturePath != null)
                {
                    FileStream iStreamIn = new FileStream(_TextureDefinitions[i].NewTexturePath, FileMode.Open, FileAccess.Read);
                    BinaryReader iReaderIn = new BinaryReader(iStreamIn);

                    long newLength = iStreamIn.Length - 128;

                    iWriterDest.Write(_TextureDefinitions[i].Flags1);
                    iWriterDest.Write(_TextureDefinitions[i].Type);
                    iWriterDest.Write((UInt16)_TextureDefinitions[i].Height);
                    iWriterDest.Write((UInt16)_TextureDefinitions[i].Width);
                    iWriterDest.Write((UInt32)newLength); // (_TextureDefinitions[i].Length - 16)
                    iWriterDest.Write(_TextureDefinitions[i].Flags2);

                    iReaderIn.BaseStream.Position = (long)128;
                    for (int b = 128; b < iStreamIn.Length; b++)
                    {
                        byte currentByte = iReaderIn.ReadByte();
                        iWriterDest.Write(currentByte);
                    }

                    iReaderIn.Close();
                    iStreamIn.Close();
                }
                else
                {
                    iWriterDest.Write(_TextureDefinitions[i].Flags1);
                    iWriterDest.Write(_TextureDefinitions[i].Type);
                    iWriterDest.Write((UInt16)_TextureDefinitions[i].Height);
                    iWriterDest.Write((UInt16)_TextureDefinitions[i].Width);
                    iWriterDest.Write((UInt32)(_TextureDefinitions[i].Length - 16));
                    iWriterDest.Write(_TextureDefinitions[i].Flags2);

                    iStreamSource.Seek(_TextureDefinitions[i].Offset + 16, SeekOrigin.Begin);
                    for (int b = 16; b < _TextureDefinitions[i].Length; b++)
                    {
                        byte currentByte = iReaderSource.ReadByte();
                        iWriterDest.Write(currentByte);
                    }
                }
            }

            iWriterDest.Close();
            iStreamDest.Close();
            iReaderSource.Close();
            iStreamSource.Close();
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
