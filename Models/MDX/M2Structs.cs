using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SharpWoW.Models.MDX
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct M2Header
    {
        internal fixed byte Magic[4];
        internal fixed byte Version[4];
        internal uint lenName;
        internal uint ofsName;
        internal uint globalFlags;
        internal uint nGlobalSequences;
        internal uint ofsGlobalSequences;
        internal uint nAnimations;
        internal uint ofsAnimations;
        internal uint nAnimationLookup;
        internal uint ofsAnimationLookup;
        internal uint nBones;
        internal uint ofsBones;
        internal uint nKeyBoneLookup;
        internal uint ofsKeyBoneLookup;
        internal uint nVertices;
        internal uint ofsVertices;
        internal uint nViews;
        internal uint nColors;
        internal uint ofsColors;
        internal uint nTextures;
        internal uint ofsTextures;
        internal uint nTransparencies;
        internal uint ofsTransparencies;
        internal uint nUVAnimations;
        internal uint ofsUVAnimations;
        internal uint nTexReplace;
        internal uint ofsTexReplace;
        internal uint nRenderFlags;
        internal uint ofsRenderFlags;
        internal uint nBoneLookupTables;
        internal uint ofsBoneLookupTables;
        internal uint nTexLookups;
        internal uint ofsTexLookups;
        internal uint nTexUnits;
        internal uint ofsTexUnits;
        internal uint nTransLookups;
        internal uint ofsTransLookups;
        internal uint nUVAnimLookups;
        internal uint ofsUVAnimLookups;
        internal SlimDX.Vector3 VertexMin;
        internal SlimDX.Vector3 VertexMax;
        internal float VertexRadius;
        internal SlimDX.Vector3 BoundingMin;
        internal SlimDX.Vector3 BoundingMax;
        internal float BoundingRadius;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct M2Texture
    {
        internal uint type;
        internal uint flags;
        internal uint lenName;
        internal uint ofsName;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct M2Vertex
    {
        internal float x, y, z;
        internal fixed byte boneWeight[4];
        internal fixed byte bonIndex[4];
        internal float nx, ny, nz;
        internal float u, v;
        internal fixed float unk[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MdxVertex
    {
        public float X, Y, Z;
        public float NX, NY, NZ;
        public float U, V;

        public const SlimDX.Direct3D9.VertexFormat FVF = SlimDX.Direct3D9.VertexFormat.Position | SlimDX.Direct3D9.VertexFormat.Normal | SlimDX.Direct3D9.VertexFormat.Texture1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MdxInstanceData
    {
        public uint InstanceId;
        public SlimDX.Matrix ModelMatrix;       
    }

    /// <summary>
    /// Contains all neccessary information for the M2Render-class in the Video-namespace to render a part of the M2
    /// </summary>
    public class M2RenderPass
    {
        /// <summary>
        /// The vertices of this pass
        /// </summary>
        public MdxVertex[] Vertices;
        /// <summary>
        /// The texture used in this pass
        /// </summary>
        public string Texture;

        public M2RenderFlags BlendMode;
    }

    public struct M2RenderFlags
    {
        public ushort flags;
        public ushort blend;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SKINView
    {
        internal uint ID, nIndices, ofsIndices, nTriangles, ofsTriangles;
        internal uint nProperties, ofsProperties, nSubMeshes, ofsSubMeshes;
        internal uint nTexUnits, ofsTexUnits, nBones;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SKINSubMesh
    {
        internal uint ID;
        internal ushort startVertx, nVertices, startTriangle, nTriangles;
        internal ushort nBones, startBone, unk1, unk2;
        internal float MinX, MinY, MinZ;
        internal float MaxX, MaxY, MaxZ;
        internal float Radius;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SKINTexUnit
    {
        internal ushort flags, shading, SubMesh1, SubMesh2, ColorIndex;
        internal ushort RenderFlags, TexUnitNumber, Mode, Texture, TexUnit2;
        internal ushort Transparency, TextureAnim;
    }
}
