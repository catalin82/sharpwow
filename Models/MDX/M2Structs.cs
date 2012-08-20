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
        internal byte bw1, bw2, bw3, bw4;
        internal byte bi1, bi2, bi3, bi4;
        internal float nx, ny, nz;
        internal float u, v;
        internal fixed float unk[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MdxVertex
    {
        public float X, Y, Z;
        public float w1, w2, w3, w4;
        public byte bi1, bi2, bi3, bi4;
        public float NX, NY, NZ;
        public float U, V;

        public const SlimDX.Direct3D9.VertexFormat FVF = SlimDX.Direct3D9.VertexFormat.Normal | SlimDX.Direct3D9.VertexFormat.Texture1
            | SlimDX.Direct3D9.VertexFormat.PositionBlend5 | SlimDX.Direct3D9.VertexFormat.LastBetaUByte4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MdxInstanceData
    {
        public uint InstanceId;
        public uint IsSelected;
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

        public ushort BoneBaseIndex = 0;
        public SlimDX.Matrix[] BoneMatrices;
        public M2Info ParentModel;

        public void UpdatePass()
        {
            for (ushort i = 0; i < BoneMatrices.Length; ++i)
            {
                BoneMatrices[i] = ParentModel.BoneAnimator.GetBone((short)(ParentModel.BoneLookupTable[i + BoneBaseIndex])).Matrix;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AnimationLineHeader
    {
        public uint nEntries;
        public uint ofsEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AnimationBlock
    {
        public ushort Type;
        public short SequenceID;
        public uint numTimeStamps;
        public uint ofsTimeStamps;
        public uint numKeyFrames;
        public uint ofsKeyFrames;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct M2Bone
    {
        public int KeyBoneID;
        public uint Flags;
        public short ParentBone;
        public short geoID;
        public int unk;
        public AnimationBlock Translation;
        public AnimationBlock Rotation;
        public AnimationBlock Scaling;
        public SlimDX.Vector3 PivotPoint;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct M2Animation
    {
        public ushort AnimationID;
        public ushort SubAnimationID;
        public uint Length;
        public float MoveSpeed;
        public uint flags;
        public ushort Probability;
        public ushort Unused;
        public uint Unk1;
        public uint Unk2;
        public uint PlaybackSpeed;
        public SlimDX.Vector3 MaxExtent;
        public SlimDX.Vector3 MinExtent;
        public float SphereRadius;
        public short NextAnimation;
        public ushort Index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureAnim
    {
        public AnimationBlock Translation;
        public AnimationBlock Rotation;
        public AnimationBlock Scaling;
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
