using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;

namespace SharpWoW.Models.WMO
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MOHD
    {
        public uint nMaterials;
        public uint nGroups;
        public uint nPortals;
        public uint nLights;
        public uint nModels;
        public uint nDoodads;
        public uint nSets;
        public uint ambientColor;
        public uint wmoID;
        public Vector3 MinPosition;
        public Vector3 MaxPosition;
        public uint unk;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOMT
    {
        public uint flags1;
        public uint SpecularMode;
        public uint blendMode;
        public uint ofsTexture1;
        public uint texColor1;
        public uint texFlags1;
        public uint ofsTexture2;
        public uint texColor2;
        public uint texFlags2;
        public uint texColor3;
        public uint unk1, unk2, unk3, unk4, unk5, unk6;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOGI
    {
        public uint flags;
        public Vector3 MinPosition;
        public Vector3 MaxPosition;
        public uint nameOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOGP
    {
        public uint ofsGroupName;
        public uint ofsDescGroupName;
        public uint Flags;
        public Vector3 MinPosition;
        public Vector3 MaxPosition;
        public ushort ofsMOPR;
        public ushort numMOPR;
        public ushort batchesA;
        public ushort batchesB;
        public ushort batchesC;
        public uint fogList;
        public uint liquidType;
        public uint groupID;
        public uint unk1, unk2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOBA
    {
        public uint color1;
        public uint color2;
        public uint color3;
        public uint startIndex;
        public ushort numIndices;
        public ushort startVertex;
        public ushort endVertex;
        public byte unk1;
        public byte textureID;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WMOVertex
    {
        public float x, y, z;
        public float nx, ny, nz;
        public float u, v;

        public const SlimDX.Direct3D9.VertexFormat FVF = SlimDX.Direct3D9.VertexFormat.Position | SlimDX.Direct3D9.VertexFormat.Texture1 | SlimDX.Direct3D9.VertexFormat.Normal;
        public const uint Size = 20;
    }
}
