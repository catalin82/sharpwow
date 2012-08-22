using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SlimDX;

namespace SharpWoW.ADT.Wotlk
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MHDR
    {
        public uint flags, ofsMcin, ofsMtex, ofsMmdx, ofsMmid, ofsMwmo;
        public uint ofsMwid, ofsMddf, ofsModf, ofsMfbo, ofsMh2o, ofsMtfx;
        public uint unused1, unused2, unused3, unused4;

        public static uint Size = (uint)Marshal.SizeOf(typeof(MHDR));
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCIN
    {
        public uint ofsMcnk, size, flags, asyncId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCNK
    {
        public uint flags, indexX, indexY, nLayers, nDoodadRefs;
        public uint ofsHeight, ofsNormal, ofsLayer, ofsRefs, ofsAlpha;
        public uint sizeAlpha, ofsShadow, sizeShadow, areaId, nMapObjRefs;
        public uint holes;
        public ulong lq1, lq2;
        public uint predTex, noEffectDoodad, ofsSndEmitter, nSndEmitters;
        public uint ofsLiquid, sizeLiqud;
        public Vector3 position;
        public uint ofsMCCV, ofsMCLV, unused;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCLY
    {
        public uint textureId, flags, offsetMCAL;
        public int effectId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ADTVertex
    {
        public float X, Y, Z;
        public float NX, NY, NZ;
        public float U, V;
        public float S, T;
        public Vector3 Tangent;
        public Vector3 Binormal;

        public static SlimDX.Direct3D9.VertexFormat FVF = SlimDX.Direct3D9.VertexFormat.Position | SlimDX.Direct3D9.VertexFormat.Texture4 | SlimDX.Direct3D9.VertexFormat.Normal | new TexCoordSize3(2) | new TexCoordSize3(3);
        public const int Size = 20;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MDDF
    {
        /// <summary>
        /// 0-based index into the MMID-array
        /// </summary>
        public uint idMMID;
        /// <summary>
        /// The unique (among surrounding ADT) ID of this m2-instance
        /// </summary>
        public uint uniqueId;
        /// <summary>
        /// the absolute positions of the doodad relative to the top left corner of the whole map
        /// </summary>
        public float posX, posY, posZ;
        /// <summary>
        /// Rotation about each axis.
        /// </summary>
        public float orientationX, orientationY, orientationZ;
        /// <summary>
        /// The real scale of the m2 is (scaleFactor / 1024.0f)
        /// </summary>
        public ushort scaleFactor;
        /// <summary>
        /// Not really known, but 1 for biodomes, 2 for clovers (both after Expansion01/Northred)
        /// </summary>
        public ushort flags;
    }

    /// <summary>
    /// Placement information for WMO-objects
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MODF
    {
        /// <summary>
        /// 0-based index into the MWID-array
        /// </summary>
        public uint idMWID;
        /// <summary>
        /// Unique ID of this WMO among surrounding ADT (to ensure its not drawn multiple times)
        /// </summary>
        public uint uniqueId;
        /// <summary>
        /// Position in the world relative to the top left corner of the world (NOT ADT)
        /// </summary>
        public SlimDX.Vector3 Position;
        /// <summary>
        /// Rotation around every axis
        /// </summary>
        public SlimDX.Vector3 Rotation;
        /// <summary>
        /// Not really known what theyre for...
        /// </summary>
        public float uextentX, uextentY, uextentZ;
        /// <summary>
        /// Not really known what theyre for...
        /// </summary>
        public float lextentX, lextentY, lextentZ;
        /// <summary>
        /// 1 seems to phasing related
        /// </summary>
        public ushort flags;
        /// <summary>
        /// Determines which set is used for rendering: See http://www.madx.dk/wowdev/wiki/index.php?title=WMO#MODS_chunk
        /// </summary>
        public ushort setIndex;
        internal ushort unknown, padding;
    }
}
