using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SlimDX;
using SlimDX.Direct3D9;

namespace SharpWoW.Models.WMO
{
    public class WMOGroup
    {
        public WMOGroup(string fileName, uint num, WMOFile parent)
        {
            mParent = parent;
            FileName = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_" + num.ToString("D3") + ".wmo";
        }

        public bool LoadGroup()
        {
            mFile = new Stormlib.MPQFile(FileName);
            SeekChunk("PGOM", false);
            mFile.Position += 4;
            mHeader = mFile.Read<MOGP>();

            Vector3[] vertices = ReadChunkAs<Vector3>("TVOM");
            ushort[] indices = ReadChunkAs<ushort>("IVOM");
            Vector2[] texCoords = ReadChunkAs<Vector2>("VTOM");

            MOBA[] batches = ReadChunkAs<MOBA>("ABOM");
            Game.GameManager.GraphicsThread.CallOnThread(
                () =>
                {
                    foreach (var batch in batches)
                    {
                        WMOVertex[] vert = new WMOVertex[batch.numIndices];
                        for (uint t = 0, j = batch.startIndex; t < batch.numIndices; ++t, ++j)
                        {
                            vert[t] = new WMOVertex()
                            {
                                x = vertices[indices[j]].X,
                                y = vertices[indices[j]].Y,
                                z = vertices[indices[j]].Z,
                                u = texCoords[indices[j]].X,
                                v = texCoords[indices[j]].Y
                            };
                        }

                        Mesh mesh = new Mesh(Game.GameManager.GraphicsThread.GraphicsManager.Device,
                            batch.numIndices / 3, batch.numIndices, MeshFlags.Managed, WMOVertex.FVF);

                        var strm = mesh.LockVertexBuffer(LockFlags.None);
                        strm.WriteRange(vert);
                        mesh.UnlockVertexBuffer();

                        strm = mesh.LockIndexBuffer(LockFlags.None);
                        ushort[] meshIndices = new ushort[vert.Length];
                        for (int i = 0; i < vert.Length; ++i)
                            meshIndices[i] = (ushort)i;

                        strm.WriteRange(meshIndices);
                        mesh.UnlockIndexBuffer();

                        mMeshes.Add(mesh);
                        mTextures.Add(mParent.GetTexture(batch.textureID));
                        var name = mParent.DEBUG_GetTextureName(batch.textureID);
                        var mat = mParent.DEBUG_GetMaterial(batch.textureID);
                        mMaterials.Add(mat);
                    }
                }
            );

            return true;
        }

        public void RenderGroup(Matrix transform)
        {
            var dev = Game.GameManager.GraphicsThread.GraphicsManager.Device;
            dev.SetTransform(TransformState.World, transform);

            for (int i = 0; i < mMeshes.Count; ++i)
            {
                dev.SetTexture(0, mTextures[i].Native);
                if (mMaterials[i].blendMode > 0)
                {
                    dev.SetRenderState(RenderState.AlphaBlendEnable, true);
                    dev.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                    dev.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                    dev.SetRenderState(RenderState.AlphaTestEnable, true);
                    dev.SetRenderState(RenderState.AlphaFunc, Compare.Greater);
                    dev.SetRenderState(RenderState.AlphaRef, 0.01f);
                }
                else
                {
                    dev.SetRenderState(RenderState.AlphaBlendEnable, false);
                    dev.SetRenderState(RenderState.AlphaTestEnable, false);
                }

                mMeshes[i].DrawSubset(0);
            }

            dev.SetTransform(TransformState.World, Matrix.Identity);
        }

        private void SeekChunk(string id, bool afterHeader = true)
        {
            mFile.Position = 0;

            if (afterHeader == true)
            {
                SeekChunk("PGOM", false);
                mFile.Position += 4 + System.Runtime.InteropServices.Marshal.SizeOf(typeof(MOGP));
            }

            while (GetChunk() != id)
            {
                uint size = mFile.Read<uint>();
                mFile.Position += size;
            }
        }

        private T[] ReadChunkAs<T>(string signature) where T : struct
        {
            SeekChunk(signature);
            uint size = mFile.Read<uint>();
            var data = mFile.Read(size);
            T[] ret = new T[data.Length / System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Utils.Memory.CopyMemory(data, ret);
            return ret;
        }

        private string GetChunk()
        {
            byte[] sig = mFile.Read(4);
            var str = Encoding.UTF8.GetString(sig);
            return str;
        }

        public string FileName { get; private set; }

        private MOGP mHeader;
        private WMOFile mParent;
        private Stormlib.MPQFile mFile;
        private List<Mesh> mMeshes = new List<Mesh>();
        private List<Video.TextureHandle> mTextures = new List<Video.TextureHandle>();
        private List<MOMT> mMaterials = new List<MOMT>();
    }
}
