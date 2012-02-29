using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;
using System.Runtime.InteropServices;

namespace SharpWoW.Models.MDX
{
    /// <summary>
    /// Handles everything related to the batch-rendering of a certain M2-Model
    /// </summary>
    public class M2BatchRender
    {
        /// <summary>
        /// Loads a batch renderer for a doodad in a render context
        /// </summary>
        /// <param name="mgr">The render context</param>
        /// <param name="doodadName">Name of the doodad (case insensitive)</param>
        public M2BatchRender(string doodadName)
        {
            M2Info inf = Game.GameManager.M2ModelCache.GetInfo(doodadName);
            Game.GameManager.GraphicsThread.CallOnThread(
                () =>
                {
                    for (int i = 0; i < inf.Passes.Count; ++i)
                    {
                        Models.MDX.M2RenderPass pass = inf.Passes[i];
                        VertexBuffer vb = new VertexBuffer(Game.GameManager.GraphicsThread.GraphicsManager.Device, pass.Vertices.Length * Marshal.SizeOf(typeof(Models.MDX.MdxVertex)), Usage.None,
                            Models.MDX.MdxVertex.FVF, Pool.Managed);
                        DataStream strm = vb.Lock(0, 0, LockFlags.None);
                        strm.WriteRange(pass.Vertices);
                        vb.Unlock();
                        IndexBuffer ib = new IndexBuffer(Game.GameManager.GraphicsThread.GraphicsManager.Device, pass.Vertices.Length * 2, Usage.None, Pool.Managed, true);
                        strm = ib.Lock(0, 0, LockFlags.None);
                        for (int j = 0; j < pass.Vertices.Length; ++j)
                            strm.Write((short)j);
                        ib.Unlock();
                        Textures.Add(Video.TextureManager.GetTexture(pass.Texture));
                        NumTriangles.Add(pass.Vertices.Length / 3);
                        Indices.Add(ib);
                        Meshes.Add(vb);
                    }
                }
            );
            InstanceDeclaration = new VertexDeclaration(Game.GameManager.GraphicsThread.GraphicsManager.Device, ElemDecl);
            InstanceLoader = new M2InstanceLoader(this);

            Game.GameManager.GraphicsThread.OnFrame += new Game.VideoThread.FrameRenderDelegate(_OnFrame);
        }

        void _OnFrame(Device device, TimeSpan deltaTime)
        {
            device.SetRenderState(RenderState.AlphaBlendEnable, true);
            device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

            device.SetRenderState(RenderState.AlphaTestEnable, true);
            device.SetRenderState(RenderState.AlphaFunc, Compare.Greater);
            device.SetRenderState(RenderState.AlphaRef, 0.3f);

            RenderInstances(device);
            
            device.SetRenderState(RenderState.AlphaBlendEnable, false);
            device.SetRenderState(RenderState.AlphaTestEnable, false);
        }

        /// <summary>
        /// Adds an instance to the current collection.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="scale"></param>
        /// <param name="rot"></param>
        public uint AddInstance(float x, float y, float z, float scale, Vector3 rot)
        {
            return InstanceLoader.PushInstance(new Vector3(x, y, z), scale, rot);
        }

        private void RenderInstances(Device dev)
        {
            InstanceLoader.UpdateVisibility();
            if (Meshes.Count == 0 || Textures.Count == 0 || InstanceDataBuffer == null || numInstances == 0)
                return;

            int counter = 0;
            var shdr = Video.ShaderCollection.MDXShader;
            dev.VertexDeclaration = InstanceDeclaration;
            foreach (VertexBuffer vb in Meshes)
            {
                dev.SetStreamSource(0, vb, 0, Marshal.SizeOf(typeof(Models.MDX.MdxVertex)));
                dev.SetStreamSourceFrequency(0, numInstances, StreamSource.IndexedData);

                dev.SetStreamSource(1, InstanceDataBuffer, 0, Marshal.SizeOf(typeof(Models.MDX.MdxInstanceData)));
                dev.SetStreamSourceFrequency(1, 1, StreamSource.InstanceData);

                dev.Indices = Indices[counter];

                shdr.SetTexture("MeshTexture", Textures[counter]);

                shdr.DoRender((device) =>
                    {
                        dev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumTriangles[counter] * 3, 0, NumTriangles[counter]);
                    }
                );

                ++counter;
            }
            dev.VertexDeclaration = null;

            dev.ResetStreamSourceFrequency(0);
            dev.ResetStreamSourceFrequency(1);
        }

        internal List<VertexBuffer> Meshes = new List<VertexBuffer>();
        internal List<IndexBuffer> Indices = new List<IndexBuffer>();
        internal List<int> NumTriangles = new List<int>();
        internal List<Video.TextureHandle> Textures = new List<Video.TextureHandle>();
        internal VertexBuffer InstanceDataBuffer;
        internal int numInstances = 0;
        internal M2InstanceLoader InstanceLoader;
        internal VertexDeclaration InstanceDeclaration;

        private static VertexElement[] ElemDecl = new VertexElement[]
        {
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
            new VertexElement(0, 24, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
            new VertexElement(1, 0, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
            new VertexElement(1, 4, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 1),
            new VertexElement(1, 20, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 2),
            new VertexElement(1, 36, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 3),
            new VertexElement(1, 52, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 4),
            VertexElement.VertexDeclarationEnd,
        };
    }

    internal class M2InstanceLoader
    {
        internal M2InstanceLoader( M2BatchRender render)
        {
            Renderer = render;
        }

        public bool ExecuteTask()
        {
            mInstLock.WaitOne();
            if (WaitingInstances.Count > 0)
            {
                for (int i = 0; WaitingInstances.Count > 0; ++i)
                {
                    var data = WaitingInstances.First();
                    ActiveInstances.Add(data.Key, data.Value);
                    WaitingInstances.Remove(data.Key);
                }
                if (Renderer.InstanceDataBuffer != null)
                    Renderer.InstanceDataBuffer.Dispose();
                int size = Marshal.SizeOf(typeof(Models.MDX.MdxInstanceData));
                Renderer.InstanceDataBuffer = new VertexBuffer(Game.GameManager.GraphicsThread.GraphicsManager.Device, ActiveInstances.Count * size, Usage.WriteOnly, VertexFormat.Texture4, Pool.Managed);
                DataStream strm = Renderer.InstanceDataBuffer.Lock(0, 0, LockFlags.None);
                Models.MDX.MdxInstanceData[] InstData = ActiveInstances.Values.ToArray();
                strm.WriteRange(InstData);
                Renderer.InstanceDataBuffer.Unlock();
                Renderer.numInstances = ActiveInstances.Count;
            }
            mInstLock.ReleaseMutex();
            return WaitingInstances.Count == 0;
        }

        internal void UpdateVisibility()
        {
            lock (mInstLock)
            {
                Dictionary<uint, Models.MDX.MdxInstanceData> VisibleInstances = new Dictionary<uint, Models.MDX.MdxInstanceData>();
                Dictionary<uint, Models.MDX.MdxInstanceData> TmpInvis = new Dictionary<uint, Models.MDX.MdxInstanceData>();
                bool changed = false;
                foreach (var inst in InvisibleInstances)
                {
                    if (IsInstanceVisible(new Vector3(inst.Value.ModelMatrix.M41, inst.Value.ModelMatrix.M42, inst.Value.ModelMatrix.M43)))
                    {
                        changed = true;
                        VisibleInstances.Add(inst.Key, inst.Value);
                    }
                    else
                        TmpInvis.Add(inst.Key, inst.Value);
                }

                foreach (var inst in ActiveInstances)
                {
                    if (IsInstanceVisible(new Vector3(inst.Value.ModelMatrix.M41, inst.Value.ModelMatrix.M42, inst.Value.ModelMatrix.M43)))
                        VisibleInstances.Add(inst.Key, inst.Value);
                    else
                    {
                        changed = true;
                        TmpInvis.Add(inst.Key, inst.Value);
                    }
                }

                ActiveInstances = VisibleInstances;
                InvisibleInstances = TmpInvis;
                Renderer.numInstances = ActiveInstances.Count;
                if (ActiveInstances.Count == 0)
                    return;

                if (changed == false)
                    return;

                if (Renderer.InstanceDataBuffer != null)
                    Renderer.InstanceDataBuffer.Dispose();

                int size = Marshal.SizeOf(typeof(Models.MDX.MdxInstanceData));
                Renderer.InstanceDataBuffer = new VertexBuffer(Game.GameManager.GraphicsThread.GraphicsManager.Device, ActiveInstances.Count * size, Usage.WriteOnly, VertexFormat.Diffuse | VertexFormat.Texture4, Pool.Managed);
                DataStream strm = Renderer.InstanceDataBuffer.Lock(0, 0, LockFlags.None);
                Models.MDX.MdxInstanceData[] InstData = ActiveInstances.Values.ToArray();
                strm.WriteRange(InstData);
                Renderer.InstanceDataBuffer.Unlock();
                Renderer.numInstances = ActiveInstances.Count;
            }
        }

        internal bool IsInstanceVisible(Vector3 pos)
        {
            Vector3 dir = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Front;
            float midX = pos.X;
            float midZ = pos.Z;
            var camPos = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position;

            Vector3 destV = new Vector3(midX - camPos.X, pos.Y - camPos.Y, midZ - camPos.Z);
            float lengths = destV.Length() * dir.Length();
            float skalar = dir.X * destV.X + dir.Y * destV.Y + dir.Z * destV.Z;

            float cos = skalar / lengths;
            float angle = (float)Math.Acos((double)cos);
            float angleD = (angle * 180) / (float)Math.PI;
            if (angleD > 60 || (destV.Length() > 1.4f * Utils.Metrics.Tilesize))
                return false;
            return true;
        }

        public uint PushInstance(Vector3 Position, float scale, Vector3 rotation)
        {
            Models.MDX.MdxInstanceData inst = new Models.MDX.MdxInstanceData()
            {
                ModelMatrix =
                Matrix.Scaling(scale, scale, scale)
                * Matrix.RotationYawPitchRoll(rotation.X, rotation.Y, rotation.Z)
                * Matrix.Translation(Position),
            };
            mInstLock.WaitOne();
            var id = RequestInstanceId();
            WaitingInstances.Add(id, inst);
            mInstLock.ReleaseMutex();

            Game.GameManager.GraphicsThread.CallOnThread(
                () =>
                {
                    ExecuteTask();
                },
                true
            );

            return id;
        }

        public void RemoveInstance(uint instanceId)
        {
            mInstLock.WaitOne();

            if (WaitingInstances.ContainsKey(instanceId))
                WaitingInstances.Remove(instanceId);

            if (ActiveInstances.ContainsKey(instanceId))
                ActiveInstances.Remove(instanceId);

            if (InvisibleInstances.ContainsKey(instanceId))
                InvisibleInstances.ContainsKey(instanceId);

            mInstLock.ReleaseMutex();

            lock (mIdLock)
            {
                mUsedInstances.Remove(instanceId);
                mFreeInstances.Add(instanceId);
            }
        }

        private uint RequestInstanceId()
        {
            lock (mIdLock)
            {
                if (mFreeInstances.Count > 0)
                {
                    var ret = mFreeInstances.First();
                    mFreeInstances.RemoveAt(0);
                    mUsedInstances.Add(ret);
                    return ret;
                }

                if (mUsedInstances.Count > 0)
                {
                    var ret = mUsedInstances.Max() + 1;
                    mUsedInstances.Add(ret);
                    return ret;
                }

                mUsedInstances.Add(0);
                return 0;
            }
        }

        private Dictionary<uint, Models.MDX.MdxInstanceData> WaitingInstances = new Dictionary<uint, Models.MDX.MdxInstanceData>();
        private Dictionary<uint, Models.MDX.MdxInstanceData> ActiveInstances = new Dictionary<uint, Models.MDX.MdxInstanceData>();
        private Dictionary<uint, Models.MDX.MdxInstanceData> InvisibleInstances = new Dictionary<uint, Models.MDX.MdxInstanceData>();
        private M2BatchRender Renderer;
        private System.Threading.Mutex mInstLock = new System.Threading.Mutex();
        private List<uint> mFreeInstances = new List<uint>();
        private List<uint> mUsedInstances = new List<uint>();
        private object mIdLock = new object();
    }
}
