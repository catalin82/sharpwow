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
}
