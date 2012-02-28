using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SharpWoW.ADT.Cataclysm
{
    /// <summary>
    /// Header of the main ADT-file
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MHDR
    {
        /// <summary>
        /// flags AND 1 => MFBO included
        /// </summary>
        public uint flags;

        /// <summary>
        /// Offset to the MCIN-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMcin;
        /// <summary>
        /// Offset to the MTEX-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMtex;
        /// <summary>
        /// Offset to the MMDX-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMmdx;
        /// <summary>
        /// Offset to the MMID-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMmid;
        /// <summary>
        /// Offset to the MWMO-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMwmo;
        /// <summary>
        /// Offset to the MWID-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMwid;
        /// <summary>
        /// Offset to the MDDF-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMddf;
        /// <summary>
        /// Offset to the MODF-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsModf;
        /// <summary>
        /// Offset to the MFBO-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMfbo;
        /// <summary>
        /// Offset to the MH2O-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMh2o;
        /// <summary>
        /// Offset to the MTFX-chunk relative to the beginning of MDHR (normally at 0x14)
        /// </summary>
        public uint ofsMtfx;
        internal uint pad, pad2, pad3, pad4;
    }

    public class ChunkOffset
    {
        public uint Offset { get; set; }
        public uint Size { get; set; }
        public uint OffsetTexStream { get; set; }
    }

    /// <summary>
    /// The header of every of the 256 chunks within the ADT
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MCNK
    {
        /// <summary>
        /// 0x01 => MCSH available
        /// 0x02 => Cannot enter chunk
        /// 0x04 => Water displayed as river
        /// 0x08 => Water displayed as ocean
        /// 0x10 => Water displayed as Magma
        /// 0x20 => Water displayed as Slime
        /// 0x40 => MCCV is available
        /// </summary>
        public uint flags;
        /// <summary>
        /// The horizontal, 0-based index of the chunk in the ADT
        /// </summary>
        public uint indexX;
        /// <summary>
        /// The vertical, 0-based index of the chunk in the ADT
        /// </summary>
        public uint indexZ;
        /// <summary>
        /// Number of texture layers that get blended [1, 4]
        /// </summary>
        public uint numLayers;
        /// <summary>
        /// Number of references in MCRF that are for m2-models
        /// </summary>
        public uint numM2Refs;
        /// <summary>
        /// Offset to the MCVT-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCVT;
        /// <summary>
        /// Offset to the MCNR-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCNR;
        /// <summary>
        /// Offset to the MCLY-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCLY;
        /// <summary>
        /// Offset to the MCRF-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCRF;
        /// <summary>
        /// Offset to the MCAL-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCAL;
        /// <summary>
        /// Size of the MCAL-chunk including the magic and chunksize (real data is sizeAlpha - 8)
        /// </summary>
        public uint sizeAlpha;
        /// <summary>
        /// Offset to the MCSH-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCSH;
        /// <summary>
        /// Size of the MCSH-chunk including the magic and chunksize (real data is sizeShadow - 8)
        /// </summary>
        public uint sizeShadow;
        /// <summary>
        /// Reference into AreaTable.dbc
        /// </summary>
        public uint refAreaId;
        /// <summary>
        /// The number of entries in the MCRF that represent WMO-objects
        /// </summary>
        public uint numWmoRefs;
        /// <summary>
        /// Arranged rowwise. If the bit is set, there is a hole at the point:
        /// 0x0001  0x0002  0x0004  0x0008
        /// 0x0010  0x0020  0x0040  0x0080
        /// 0x0100  0x0200  0x0400  0x0800
        /// 0x1000  0x2000  0x4000  0x8000
        /// </summary>
        public uint holeBitmap;
        internal ulong lowQualityUnknwon1;
        internal ulong lowQualityUnknwon2;
        internal uint predTex;
        internal uint noEffectDoodad;
        /// <summary>
        /// Offset to the MCSE-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCSE;
        /// <summary>
        /// Number of soundemitters in the MCSE-chunk
        /// </summary>
        public uint numSoundEmitters;
        /// <summary>
        /// Offset to the MCLQ-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCLQ;
        /// <summary>
        /// Size of the MCLQ-chunk including magic and size (real data is sizeLiquid - 8)
        /// </summary>
        public uint sizeLiquid;
        /// <summary>
        /// The basic positions of the chunk. About the system, see here: http://www.madx.dk/wowdev/wiki/index.php?title=ADT#An_important_note_about_the_coordinate_system_used
        /// </summary>
        public float xBase, yBase, zBase;
        /// <summary>
        /// Offset to the MCCV-chunk relative to the start of MCNK
        /// </summary>
        public uint ofsMCCV;
        internal uint unused1, unused2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCLY
    {
        /// <summary>
        /// An index into the list of texture in MTFX
        /// </summary>
        public uint textureId;
        /// <summary>
        /// 0x001 => Rotate 45° clockwise
        /// 0x002 => Rotate 90° clockwise
        /// 0x004 => Rotate 180° clockwise
        /// 0x008 => Use fast animation
        /// 0x010 => User faster animation
        /// 0x020 => Use very fast animation ("very fast" is kinda relative....)
        /// 0x040 => Perform the animations described (without 0x040 no animation will be displayed)
        /// 0x080 => Shindy
        /// 0x100 => Has an alphamap in MCAL
        /// 0x200 => Alpha is stored compressed for this layer
        /// 0x400 => Skybox. Thats pretty nasty. Dont use it!
        /// </summary>
        public uint flags;
        /// <summary>
        /// Offset _withing_ the MCAL-chunk of the current layer
        /// </summary>
        public uint ofsMCAL;
        /// <summary>
        /// Reference into GroundEffectTexture.dbc
        /// </summary>
        public short effectId;
        internal short padding;

        public static uint SizeOf = (uint)Marshal.SizeOf(typeof(MCLY));
    }
}
