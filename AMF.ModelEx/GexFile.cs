using System;
using System.IO;

namespace AMF.ModelEx
{
    public enum GexModelType
    {
        SoulReaverPlaystation,
        SoulReaverPC,
        SoulReaverDreamcast
    }

    public struct ExVector
    {
        public Int16 x, y, z;
        ExVector(Int16 x, Int16 y, Int16 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static ExVector operator +(ExVector v1, ExVector v2)
        {
            return new ExVector(
                (Int16)(v1.x + v2.x),
                (Int16)(v1.y + v2.y),
                (Int16)(v1.z + v2.z)
            );
        }
    }
    public struct ExNormal
    {
        public Int32 x, y, z;
    }
    public struct ExBone
    {
        public UInt16 vFirst, vLast;    // The ID of first and last effected vertex 
        public ExVector localPos;       // Local bone coordinates
        public ExVector worldPos;       // World bone coordinated
        public UInt16 parentID;         // ID of parent bone
    }
    public struct ExVertex
    {
        public UInt16 index;            // Index in the file
        public ExVector localPos;       // Local vertex coordinates
        public ExVector worldPos;       // World vertex coordinates
        public UInt16 normalID;         // Index of the vertex normal
        public ExNormal normal;         // Normal for the vertex
        public UInt32 colour;           // Colour of the vertex
        public UInt16 boneID;           // Index of the bone effecting this vertex
        public float u, v;              // Texture coordinates
        public uint rawU, rawV;         // The raw data for the UV coordinates
    }
    public struct ExPolygon
    {
        public Boolean isVisible;
        public ExMaterial material;     // The material used
        public ExVertex v1, v2, v3;     // Vertices for the polygon
        public int paletteColumn;       //
        public int paletteRow;          //
    }
    public class ExMaterial
    {
        public UInt16 ID;               // The ID of the material
        public Boolean textureUsed;     // Flag specifying if a texture is used
        public UInt16 textureID;        // ID of the texture file
        public UInt32 colour;           // Diffuse colour
        public String textureName;      // Name of the texture file
    }
    public class ExMaterialList
    {
        private ExMaterialList _next;
        public ExMaterialList next
        {
            get { return _next; }
        }
        public ExMaterial material;
        public ExMaterialList(ExMaterial material)
        {
            this.material = material;
            _next = null;
        }
        // Tries to add the material to the list
        public ExMaterial AddToList(ExMaterial material)
        {
            // Check if the material is already in the list
            if ((material.textureID == this.material.textureID) &&
                (material.colour == this.material.colour) &&
                (material.textureUsed == this.material.textureUsed))
                return this.material;
            // Check the rest of the list
            if (next != null)
            {
                return next.AddToList(material);
            }
            // Add the material to the list
            _next = new ExMaterialList(material);
            return material;
        }
    }
    public class ExBSPTree
    {
        public UInt32 dataPos;
        public Boolean isLeaf;
        public ExBSPTree leftChild;
        public ExBSPTree rightChild;
    }
    public class ExBSPTreeStack
    {
        private class Node
        {
            public ExBSPTree tree;
            public Node lastNode;
        }
        private Node currentNode;
        public void Push(ExBSPTree tree)
        {
            Node lastNode = currentNode;
            currentNode = new Node();
            currentNode.tree = tree;
            currentNode.lastNode = lastNode;
            return;
        }
        public ExBSPTree Pop()
        {
            if (currentNode == null) return null;
            ExBSPTree tree = currentNode.tree;
            currentNode = currentNode.lastNode;
            return tree;
        }
        public ExBSPTree Top
        {
            get
            {
                if (currentNode == null) return null;
                return currentNode.tree;
            }
        }
    }
    public class GexFile
    {
        public String modelName;
        public UInt32 dataStart;
        public UInt32 modelData;
        public UInt32 vertexCount;
        public UInt32 vertexStart;
        public UInt32 polygonCount;
        public UInt32 polygonStart;
        public UInt32 boneCount;
        public UInt32 boneStart;
        public UInt32 materialStart;
        public UInt16 materialCount;
        public UInt32 bspTreeCount;
        public UInt32 bspTreeStart;
        public ExBone[] bones;
        public UInt16[] indices;
        public ExVertex[] vertices;
        public ExPolygon[] polygons;
        public ExMaterial[] materials;
        public UInt32 indexCount { get { return 3 * polygonCount; } }
        public Boolean isObject;
        public Boolean isArea
        {
            get { return !isObject; }
            set { isObject = !value; }
        }

        protected GexModelType _ModelType;
        public GexModelType ModelType
        {
            get
            {
                return _ModelType;
            }
        }

        private ExMaterialList materialsList;
        private static Int32[,] normals = {
            {0, 0, 4096},
			{-1930, -3344, -1365},
			{3861, 0, -1365},
			{-1930, 3344, -1365},
			{-353, -613, 4034},
			{-697, -1207, 3851},
			{-1019, -1765, 3552},
			{-1311, -2270, 3146},
			{-1563, -2707, 2645},
			{-1768, -3063, 2065},
			{-1920, -3326, 1423},
			{-2014, -3489, 738},
			{-2047, -3547, 30},
			{-2019, -3498, -677},
			{707, 0, 4034},
			{1394, 0, 3851},
			{2039, 0, 3552},
			{2622, 0, 3146},
			{3126, 0, 2645},
			{3536, 0, 2065},
			{3840, 0, 1423},
			{4028, 0, 738},
			{4095, 0, 30},
			{4039, 0, -677},
			{-353, 613, 4034},
			{-697, 1207, 3851},
			{-1019, 1765, 3552},
			{-1311, 2270, 3146},
			{-1563, 2707, 2645},
			{-1768, 3063, 2065},
			{-1920, 3326, 1423},
			{-2014, 3489, 738},
			{-2047, 3547, 30},
			{-2019, 3498, -677},
			{-1311, -3498, -1678},
			{-653, -3547, -1941},
			{24, -3489, -2145},
			{701, -3326, -2285},
			{1358, -3063, -2355},
			{1973, -2707, -2355},
			{2529, -2270, -2285},
			{3009, -1765, -2145},
			{3398, -1207, -1941},
			{3685, -613, -1678},
			{3685, 613, -1678},
			{3398, 1207, -1941},
			{3009, 1765, -2145},
			{2529, 2270, -2285},
			{1973, 2707, -2355},
			{1358, 3063, -2355},
			{701, 3326, -2285},
			{24, 3489, -2145},
			{-653, 3547, -1941},
			{-1311, 3498, -1678},
			{-2373, 2885, -1678},
			{-2745, 2339, -1941},
			{-3033, 1723, -2145},
			{-3231, 1055, -2285},
			{-3331, 355, -2355},
			{-3331, -355, -2355},
			{-3231, -1055, -2285},
			{-3033, -1723, -2145},
			{-2745, -2339, -1941},
			{-2373, -2885, -1678},
			{364, -631, 4030},
			{33, -1270, 3893},
			{1083, -664, 3893},
			{-273, -1899, 3618},
			{787, -1364, 3780},
			{1781, -712, 3618},
			{-544, -2497, 3200},
			{520, -2080, 3489},
			{1541, -1490, 3489},
			{2435, -777, 3200},
			{-767, -3043, 2631},
			{293, -2785, 2989},
			{1331, -2306, 3111},
			{2265, -1646, 2989},
			{3019, -857, 2631},
			{-939, -3504, 1901},
			{110, -3426, 2240},
			{1151, -3100, 2416},
			{2108, -2547, 2416},
			{2912, -1808, 2240},
			{3504, -938, 1901},
			{-1067, -3821, 1017},
			{-52, -3906, 1230},
			{966, -3739, 1364},
			{1922, -3330, 1409},
			{2755, -2706, 1364},
			{3409, -1907, 1230},
			{3843, -985, 1017},
			{-1174, -3923, 42},
			{-238, -4088, 51},
			{711, -4033, 58},
			{1622, -3760, 61},
			{2445, -3285, 61},
			{3137, -2632, 58},
			{3660, -1838, 51},
			{3985, -944, 42},
			{-1265, -3792, -889},
			{-457, -3928, -1064},
			{368, -3900, -1194},
			{1179, -3709, -1275},
			{1941, -3363, -1302},
			{2622, -2876, -1275},
			{3193, -2269, -1194},
			{3631, -1567, -1064},
			{3917, -800, -889},
			{364, 631, 4030},
			{1083, 664, 3893},
			{33, 1270, 3893},
			{1781, 712, 3618},
			{787, 1364, 3780},
			{-273, 1899, 3618},
			{2435, 777, 3200},
			{1541, 1490, 3489},
			{520, 2080, 3489},
			{-544, 2497, 3200},
			{3019, 857, 2631},
			{2265, 1646, 2989},
			{1331, 2306, 3111},
			{293, 2785, 2989},
			{-767, 3043, 2631},
			{3504, 938, 1901},
			{2912, 1808, 2240},
			{2108, 2547, 2416},
			{1151, 3100, 2416},
			{110, 3426, 2240},
			{-939, 3504, 1901},
			{3843, 985, 1017},
			{3409, 1907, 1230},
			{2755, 2706, 1364},
			{1922, 3330, 1409},
			{966, 3739, 1364},
			{-52, 3906, 1230},
			{-1067, 3821, 1017},
			{3985, 944, 42},
			{3660, 1838, 51},
			{3137, 2632, 58},
			{2445, 3285, 61},
			{1622, 3760, 61},
			{711, 4033, 58},
			{-238, 4088, 51},
			{-1174, 3923, 42},
			{3917, 800, -889},
			{3631, 1567, -1064},
			{3193, 2269, -1194},
			{2622, 2876, -1275},
			{1941, 3363, -1302},
			{1179, 3709, -1275},
			{368, 3900, -1194},
			{-457, 3928, -1064},
			{-1265, 3792, -889},
			{-729, 0, 4030},
			{-1117, 606, 3893},
			{-1117, -606, 3893},
			{-1507, 1186, 3618},
			{-1575, 0, 3780},
			{-1507, -1186, 3618},
			{-1890, 1719, 3200},
			{-2061, 589, 3489},
			{-2061, -589, 3489},
			{-1890, -1719, 3200},
			{-2252, 2186, 2631},
			{-2558, 1138, 2989},
			{-2663, 0, 3111},
			{-2558, -1138, 2989},
			{-2252, -2186, 2631},
			{-2565, 2565, 1901},
			{-3022, 1618, 2240},
			{-3260, 552, 2416},
			{-3260, -552, 2416},
			{-3022, -1618, 2240},
			{-2565, -2565, 1901},
			{-2775, 2835, 1017},
			{-3356, 1998, 1230},
			{-3721, 1032, 1364},
			{-3845, 0, 1409},
			{-3721, -1032, 1364},
			{-3356, -1998, 1230},
			{-2775, -2835, 1017},
			{-2810, 2979, 42},
			{-3421, 2250, 51},
			{-3848, 1400, 58},
			{-4067, 475, 61},
			{-4067, -475, 61},
			{-3848, -1400, 58},
			{-3421, -2250, 51},
			{-2810, -2979, 42},
			{-2652, 2992, -889},
			{-3173, 2360, -1064},
			{-3562, 1630, -1194},
			{-3802, 832, -1275},
			{-3883, 0, -1302},
			{-3802, -832, -1275},
			{-3562, -1630, -1194},
			{-3173, -2360, -1064},
			{-2652, -2992, -889},
			{-1778, -3080, -2031},
			{-2174, -2553, -2351},
			{-1124, -3159, -2351},
			{-2482, -1926, -2627},
			{-1519, -2632, -2745},
			{-427, -3112, -2627},
			{-2683, -1207, -2849},
			{-1812, -1959, -3107},
			{-790, -2548, -3107},
			{295, -2927, -2849},
			{-2758, -404, -3000},
			{-1968, -1132, -3408},
			{-1022, -1771, -3548},
			{3, -2270, -3408},
			{1028, -2591, -3000},
			{-2690, 470, -3052},
			{-1953, -147, -3596},
			{-1074, -755, -3879},
			{-117, -1308, -3879},
			{848, -1766, -3596},
			{1753, -2094, -3052},
			{-2472, 1388, -2955},
			{-1751, 963, -3575},
			{-917, 476, -3963},
			{-23, -40, -4095},
			{871, -555, -3963},
			{1710, -1034, -3575},
			{2438, -1447, -2955},
			{-2131, 2266, -2664},
			{-1403, 2070, -3243},
			{-599, 1763, -3647},
			{237, 1361, -3855},
			{1060, 886, -3855},
			{1827, 363, -3647},
			{2495, -179, -3243},
			{3028, -712, -2664},
			{-1729, 2987, -2203},
			{-1013, 2965, -2637},
			{-255, 2819, -2960},
			{513, 2555, -3159},
			{1261, 2184, -3227},
			{1956, 1722, -3159},
			{2569, 1188, -2960},
			{3075, 604, -2637},
			{3452, -4, -2203}
        };

        public GexFile(string fileName, GexModelType modelType)
        {
            _ModelType = modelType;
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(file);

            // Get start of usefull data
            dataStart = ((reader.ReadUInt32() >> 9) << 11) + 0x00000800;

            // Get the type of model
            /*file.Position = dataStart + 0x4C;
            ulong typeTest1 = reader.ReadUInt64();
            file.Position = dataStart + 0x58;
            ulong typeTest2 = reader.ReadUInt64();

            file.Position = dataStart + 0x6C;
            ulong typeTester = 0xFFFFFFFFFFFFFFFF;
            if (reader.ReadUInt32() == 0xFFFF63C0)
            {
                typeTester = 0x0100020000000000;
            }*/

            file.Position = dataStart + 0x9C;
            ulong typeTest3 = reader.ReadUInt64();

            if (typeTest3 == 0xFFFFFFFFFFFFFFFF ||
                typeTest3 == 0x0100020000000000)
            {
                isArea = true;
                MapAreaFile(reader);
            }
            else
            {
                isObject = true;
                MapObjectFile(reader);
            }

            // Check for errors
            if ((dataStart > reader.BaseStream.Length) ||
                (modelData > reader.BaseStream.Length) ||
                (vertexStart > reader.BaseStream.Length) ||
                (polygonStart > reader.BaseStream.Length) ||
                (boneCount > reader.BaseStream.Length) ||
                (materialStart > reader.BaseStream.Length))
                throw new Exception("Wrong file type or not a mesh file");

            // Get the vertices
            ReadVertices(reader);

            if (isObject)
            {
                // Get the armature
                ReadArmature(reader);
                // Apply the armature to the vertices
                ApplyArmature();
            }

            // Get the polygons
            ReadPolygons(reader);

            // Genetate the vertices, indices and materials to be output
            GenerateOutputData();

            // Close the file
            reader.Close();
            file.Close();
            reader = null;
            file = null;
        }

        private void GenerateOutputData()
        {
            // Make the vertices unique and generate new index array
            vertices = new ExVertex[indexCount];
            indices = new UInt16[indexCount];
            for (UInt16 p = 0; p < polygonCount; p++)
            {
                vertices[(3 * p) + 0] = polygons[p].v1;
                vertices[(3 * p) + 1] = polygons[p].v2;
                vertices[(3 * p) + 2] = polygons[p].v3;
                indices[(3 * p) + 0] = (UInt16)((3 * p) + 0);
                indices[(3 * p) + 1] = (UInt16)((3 * p) + 1);
                indices[(3 * p) + 2] = (UInt16)((3 * p) + 2);
            }

            // Build the materials array
            materials = new ExMaterial[materialCount];
            UInt16 mNew = 0;

            // Get the untextured materials
            ExMaterialList matList = materialsList;
            while (matList != null)
            {
                if (!matList.material.textureUsed)
                {
                    materials[mNew] = matList.material;
                    materials[mNew].ID = mNew;
                    materials[mNew].textureName = "";
                    mNew++;
                }
                matList = matList.next;
            }

            // Get the textured materials
            matList = materialsList;
            while (matList != null)
            {
                if (matList.material.textureUsed)
                {
                    materials[mNew] = matList.material;
                    materials[mNew].ID = mNew;
                    materials[mNew].textureName =
                        "Texture-" +
                        materials[mNew].textureID.ToString("00000") +
                        ".png";
                    mNew++;
                }
                matList = matList.next;
            }
            return;
        }

        private void MapObjectFile(BinaryReader reader)
        {
            reader.BaseStream.Position = dataStart + 0x00000024;
            reader.BaseStream.Position = dataStart + reader.ReadUInt32();
            modelName = new String(reader.ReadChars(8));
            reader.BaseStream.Position = dataStart + 0x0000000C;
            reader.BaseStream.Position = dataStart + reader.ReadUInt32();
            modelData = dataStart + reader.ReadUInt32();
            reader.BaseStream.Position = modelData;
            vertexCount = reader.ReadUInt32();
            vertexStart = dataStart + reader.ReadUInt32();
            reader.BaseStream.Position += 0x00000008;
            polygonCount = reader.ReadUInt32();
            polygonStart = dataStart + reader.ReadUInt32();
            boneCount = reader.ReadUInt32();
            boneStart = dataStart + reader.ReadUInt32();
            materialCount = 0;

            return;
        }

        private void MapAreaFile(BinaryReader reader)
        {
            reader.BaseStream.Position = dataStart + 0x98;
            reader.BaseStream.Position = dataStart + reader.ReadUInt32();
            modelName = new String(reader.ReadChars(8));
            reader.BaseStream.Position = dataStart;
            modelData = dataStart + reader.ReadUInt32();
            reader.BaseStream.Position = modelData + 0x10;
            vertexCount = reader.ReadUInt32();
            polygonCount = reader.ReadUInt32();
            reader.BaseStream.Position += 0x04;
            vertexStart = dataStart + reader.ReadUInt32();
            polygonStart = dataStart + reader.ReadUInt32();
            boneCount = 0;
            boneStart = 0;
            reader.BaseStream.Position += 0x10;
            materialStart = dataStart + reader.ReadUInt32();
            materialCount = 0;
            reader.BaseStream.Position += 0x0C;
            bspTreeCount = reader.ReadUInt32();
            bspTreeStart = dataStart + reader.ReadUInt32();
            return;
        }

        private void ReadVertices(BinaryReader reader)
        {
            if (vertexStart == 0 || vertexCount == 0) return;

            reader.BaseStream.Position = vertexStart;
            vertices = new ExVertex[vertexCount];
            for (UInt16 v = 0; v < vertexCount; v++)
            {
                // Read the local coordinates
                vertices[v].localPos.x = reader.ReadInt16();
                vertices[v].localPos.y = reader.ReadInt16();
                vertices[v].localPos.z = reader.ReadInt16();

                // Before transformation, the world coords equal the local coords
                vertices[v].worldPos = vertices[v].localPos;

                // If it's an object get the normals
                if (isObject)
                {
                    vertices[v].normalID = reader.ReadUInt16();
                    vertices[v].normal.x = normals[vertices[v].normalID, 0];
                    vertices[v].normal.y = normals[vertices[v].normalID, 1];
                    vertices[v].normal.z = normals[vertices[v].normalID, 2];
                }
                // If it's an area get the vertex colours
                if (isArea)
                {
                    reader.BaseStream.Position += 2;
                    vertices[v].colour = reader.ReadUInt32() | 0xFF000000;
                    // comment out to correct Dreamcast colours
                    FlipRedAndBlue(ref vertices[v].colour);
                }

                // The vertex may need to know it's own ID
                vertices[v].index = v;
            }
            return;
        }

        private void ReadArmature(BinaryReader reader)
        {
            if (boneStart == 0 || boneCount == 0) return;

            reader.BaseStream.Position = boneStart;
            bones = new ExBone[boneCount];
            bones = new ExBone[boneCount];
            for (UInt16 b = 0; b < boneCount; b++)
            {
                // Get the bone data
                reader.BaseStream.Position += 8;
                bones[b].vFirst = reader.ReadUInt16();
                bones[b].vLast = reader.ReadUInt16();
                bones[b].localPos.x = reader.ReadInt16();
                bones[b].localPos.y = reader.ReadInt16();
                bones[b].localPos.z = reader.ReadInt16();
                bones[b].parentID = reader.ReadUInt16();

                // Combine this bone with it's ancestors is there are any
                if ((bones[b].vFirst != 0xFFFF) && (bones[b].vLast != 0xFFFF))
                {
                    for (UInt16 ancestorID = b; ancestorID != 0xFFFF; )
                    {
                        bones[b].worldPos += bones[ancestorID].localPos;
                        if (bones[ancestorID].parentID == ancestorID) break;
                        ancestorID = bones[ancestorID].parentID;
                    }
                }
                reader.BaseStream.Position += 4;
            }
            return;
        }

        private void ApplyArmature()
        {
            if ((vertexStart == 0 || vertexCount == 0) ||
                (boneStart == 0 || boneCount == 0)) return;

            for (UInt16 b = 0; b < boneCount; b++)
            {
                if ((bones[b].vFirst != 0xFFFF) && (bones[b].vLast != 0xFFFF))
                {
                    for (UInt16 v = bones[b].vFirst; v <= bones[b].vLast; v++)
                    {
                        vertices[v].worldPos += bones[b].worldPos;
                        vertices[v].boneID = b;
                    }
                }
            }
            return;
        }

        private void ReadPolygons(BinaryReader reader)
        {
            polygons = new ExPolygon[polygonCount];
            if (isArea) ReadBSPTree(reader);
            for (UInt16 p = 0; p < polygonCount; p++)
            {
                reader.BaseStream.Position = polygonStart + (p * 12);

                // Copy vertices to the polygon
                polygons[p].v1 = vertices[reader.ReadUInt16()];
                polygons[p].v2 = vertices[reader.ReadUInt16()];
                polygons[p].v3 = vertices[reader.ReadUInt16()];

                polygons[p].material = new ExMaterial();

                if (isObject)
                {
                    // Get flag to say if a texture is used
                    polygons[p].material.textureUsed = (Boolean)(((int)reader.ReadUInt16() & 0x0200) != 0);

                    // Get the texture data if present
                    if (polygons[p].material.textureUsed)
                    {
                        reader.BaseStream.Position = dataStart + reader.ReadInt32();
                        ReadTextureData(reader, ref polygons[p]);
                        reader.BaseStream.Position += 2;
                    }
                    else
                    {
                        reader.BaseStream.Position = polygonStart + (12 * p) + 8;
                    }
                    polygons[p].material.colour = reader.ReadUInt32() | 0xFF000000;
                }
                if (isArea)
                {
                    // Get flag to say if a texture is used.  This needs work...  I improved it though :)
                    polygons[p].material.textureUsed = (Boolean)(((int)reader.ReadUInt16() & 0x0004) == 0);

                    // Get the texture data if present
                    reader.BaseStream.Position += 2;
                    UInt16 materialOffset = reader.ReadUInt16();
                    if (materialOffset != 0xFFFF &&
                        polygons[p].material.textureUsed &&
                        polygons[p].isVisible)
                    {
                        reader.BaseStream.Position = materialStart + materialOffset;
                        ReadTextureData(reader, ref polygons[p]);
                        polygons[p].material.colour = 0xFFFFFFFF;
                    }
                    else
                    {
                        polygons[p].material.textureUsed = false;
                        polygons[p].material.colour = 0x00000000;
                        polygons[p].v1.colour = 0x00000000;
                        polygons[p].v2.colour = 0x00000000;
                        polygons[p].v3.colour = 0x00000000;
                    }
                }
                FlipRedAndBlue(ref polygons[p].material.colour);

                // Add the material to the list
                if (materialsList == null)
                {
                    materialsList = new ExMaterialList(polygons[p].material);
                    materialCount++;
                }
                else
                {
                    ExMaterial newMaterial = materialsList.AddToList(polygons[p].material);
                    if (polygons[p].material != newMaterial)
                    {
                        polygons[p].material = newMaterial;
                    }
                    else
                    {
                        materialCount++;
                    }
                }
            }
        }

        private void ReadTextureData(BinaryReader reader, ref ExPolygon polygon)
        {
            //(importFile.vertices[v].v) / 255f) + (0.5f / 255)

            switch (_ModelType)
            {
                case GexModelType.SoulReaverPlaystation:
                    // Playstation textures
                    polygon.v1.rawU = reader.ReadByte();
                    polygon.v1.rawV = reader.ReadByte();
                    //polygon.v1.u = ((float)(polygon.v1.rawU) / 255f) + (0.5f / 255f);
                    //polygon.v1.v = ((float)(polygon.v1.rawV) / 255f) + (0.5f / 255f);
                    polygon.v1.u = ((float)(polygon.v1.rawU) / 255.0f);
                    polygon.v1.v = ((float)(polygon.v1.rawV) / 255.0f);
                    ushort paletteVal = reader.ReadUInt16();
                    ushort rowVal = (ushort)((ushort)(paletteVal << 2) >> 8);
                    ushort colVal = (ushort)((ushort)(paletteVal << 11) >> 11);
                    polygon.paletteColumn = colVal;
                    polygon.paletteRow = rowVal;
                    polygon.v2.rawU = reader.ReadByte();
                    polygon.v2.rawV = reader.ReadByte();
                    //polygon.v2.u = ((float)(polygon.v2.rawU) / 255f) + (0.5f / 255f);
                    //polygon.v2.v = ((float)(polygon.v2.rawV) / 255f) + (0.5f / 255f);
                    polygon.v2.u = ((float)(polygon.v2.rawU) / 255.0f);
                    polygon.v2.v = ((float)(polygon.v2.rawV) / 255.0f);
                    //polygon.material.textureID = (UInt16)((reader.ReadUInt16() & 0x07FF) - 8);
                    polygon.material.textureID = (UInt16)(((reader.ReadUInt16() & 0x07FF) - 8) % 8);
                    polygon.v3.rawU = reader.ReadByte();
                    polygon.v3.rawV = reader.ReadByte();
                    //polygon.v3.u = ((float)(polygon.v3.rawU) / 255f) + (0.5f / 255f);
                    //polygon.v3.v = ((float)(polygon.v3.rawV) / 255f) + (0.5f / 255f);
                    polygon.v3.u = ((float)(polygon.v3.rawU) / 255.0f);
                    polygon.v3.v = ((float)(polygon.v3.rawV) / 255.0f);
                    break;
                case GexModelType.SoulReaverPC:
                    // PC textures
                    polygon.v1.rawU = reader.ReadByte();
                    polygon.v1.rawV = reader.ReadByte();
                    polygon.v1.u = ((float)(polygon.v1.rawU) / 255f) + (0.5f / 255f);
                    polygon.v1.v = ((float)(polygon.v1.rawV) / 255f) + (0.5f / 255f);
                    polygon.material.textureID = (UInt16)(reader.ReadUInt16() & 0x07FF);
                    polygon.v2.rawU = reader.ReadByte();
                    polygon.v2.rawV = reader.ReadByte();
                    polygon.v2.u = ((float)(polygon.v2.rawU) / 255f) + (0.5f / 255f);
                    polygon.v2.v = ((float)(polygon.v2.rawV) / 255f) + (0.5f / 255f);
                    reader.BaseStream.Position += 2;
                    polygon.v3.rawU = reader.ReadByte();
                    polygon.v3.rawV = reader.ReadByte();
                    polygon.v3.u = ((float)(polygon.v3.rawU) / 255f) + (0.5f / 255f);
                    polygon.v3.v = ((float)(polygon.v3.rawV) / 255f) + (0.5f / 255f);
                    break;
                case GexModelType.SoulReaverDreamcast:
                    // DC textures
                    ushort int1 = reader.ReadUInt16();
                    ushort int2 = reader.ReadUInt16();
                    ushort int3 = reader.ReadUInt16();
                    ushort int4 = reader.ReadUInt16();
                    ushort int5 = reader.ReadUInt16();
                    ushort int6 = reader.ReadUInt16();
                    polygon.v1.rawU = int2;
                    polygon.v1.rawV = int1;
                    polygon.v2.rawU = int4;
                    polygon.v2.rawV = int3;
                    polygon.v3.rawU = int6;
                    //polygon.v4.rawV = int5;
                    polygon.v1.u = BizarreFloatToNormalFloat(int2);
                    polygon.v1.v = BizarreFloatToNormalFloat(int1);
                    polygon.v2.u = BizarreFloatToNormalFloat(int4);
                    polygon.v2.v = BizarreFloatToNormalFloat(int3);
                    polygon.v3.u = BizarreFloatToNormalFloat(int6);
                    polygon.v3.v = BizarreFloatToNormalFloat(int5);
                    polygon.material.textureID = (UInt16)((reader.ReadUInt16() & 0x07FF) - 1);
                    break;
            }

            ////Console.WriteLine(int1 + "\t" + int2 + "\t" + int3 + "\t" + int4 + "\t" + int5 + "\t" + int6);

            return;
        }


        protected float BizarreFloatToNormalFloat(ushort bizarreFloat)
        {
            // converts the 16-bit floating point values used in the DC version of Soul Reaver to normal 32-bit floats
            ushort exponent;
            int unbiasedExponent;
            ushort significand;
            bool positive = true;
            //ushort signCheck = bizarreFloat;
            //signCheck = signCheck >> 15;

            exponent = bizarreFloat;
            exponent = (ushort)(exponent << 1);
            exponent = (ushort)(exponent >> 8);
            unbiasedExponent = exponent - 127;
            significand = bizarreFloat;
            significand = (ushort)(significand << 9);
            significand = (ushort)(significand >> 9);
            float fraction = 1f;
            for (int i = 0; i < 7; i++)
            {
                byte current = (byte)significand;
                current = (byte)(current << (i + 1));
                current = (byte)(current >> 7);
                fraction += (float)((float)current * Math.Pow(2, 0 - (1 + i)));
            }
            float calcValue = (float)(fraction * Math.Pow(2, (double)unbiasedExponent));
            //if (!positive)
            //{
            //    calcValue *= -1f;
            //}
            return calcValue;
        }

        private void ReadBSPTree(BinaryReader reader)
        {
            Boolean drawTester;
            UInt16 bspID = 0;
            ExBSPTree currentTree;
            ExBSPTree[] bspTrees = new ExBSPTree[bspTreeCount];
            ExBSPTreeStack stack = new ExBSPTreeStack();
            for (UInt16 b = 0; b < bspTreeCount; b++)
            {
                reader.BaseStream.Position = bspTreeStart + (b * 0x24);
                bspTrees[b] = new ExBSPTree();
                bspTrees[b].dataPos = dataStart + reader.ReadUInt32();

                reader.BaseStream.Position += 0x0E;
                drawTester = ((reader.ReadInt16() & 1) != 1);

                reader.BaseStream.Position += 0x06;
                bspID = reader.ReadUInt16();
                stack.Push(bspTrees[b]);
                currentTree = stack.Top;

                while (currentTree != null)
                {
                    reader.BaseStream.Position = currentTree.dataPos + 0x0E;
                    currentTree.isLeaf = ((reader.ReadByte() & 0x02) == 0x02);
                    if (currentTree.isLeaf)
                    {
                        // Handle Leaf here
                        reader.BaseStream.Position = currentTree.dataPos + 0x08;
                        UInt32 polygonPos = dataStart + reader.ReadUInt32();
                        UInt32 polygonID = (polygonPos - polygonStart) / 0x0C;
                        UInt16 polyCount = reader.ReadUInt16();
                        for (UInt16 p = 0; p < polyCount; p++)
                        {
                            // 0 = dome, 2 = firelamps, 3 = barriers,
                            // 4 = centre floor, 5 = outer floor,
                            // 6 = collision around coffins,
                            // 7 = corridor, 8 = coffins and small dome,
                            // 9 = stairs, 
                            /*if (bspID == 0 || bspID == 2 || bspID == 5 ||
                                bspID == 7 || bspID == 8 || bspID == 9 ||
                                bspID == 4)*/
                            if (drawTester)
                            polygons[polygonID + p].isVisible = true;
                        }

                        // Finished with right child, now handle left.
                        currentTree = stack.Pop();
                        continue;
                    }
                    reader.BaseStream.Position = currentTree.dataPos + 0x14;
                    UInt32 leftPos = reader.ReadUInt32();
                    if (leftPos != 0)
                    {
                        currentTree.leftChild = new ExBSPTree();
                        currentTree.leftChild.dataPos = dataStart + leftPos;
                        stack.Push(currentTree.leftChild);
                    }
                    UInt32 rightPos = reader.ReadUInt32();
                    if (rightPos != 0)
                    {
                        currentTree.rightChild = new ExBSPTree();
                        currentTree.rightChild.dataPos = dataStart + rightPos;
                        currentTree = currentTree.rightChild;
                    }
                }
            }
            return;
        }

        private void FlipRedAndBlue(ref UInt32 colour)
        {
            UInt32 tempColour = colour;
            colour =
                (tempColour & 0xFF000000) |
                ((tempColour << 16) & 0x00FF0000) |
                (tempColour & 0x0000FF00) |
                ((tempColour >> 16) & 0x000000FF);
            return;
        }
    }
}