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
            Intersector = new ModelIntersector(this);
            ModelName = doodadName;
            M2Info inf = Game.GameManager.M2ModelCache.GetInfo(doodadName);
            mModelInfo = inf;
            Game.GameManager.GraphicsThread.CallOnThread(
                () =>
                {
                    for (int i = 0; i < inf.Passes.Count; ++i)
                    {
                        Models.MDX.M2RenderPass pass = inf.Passes[i];
                        Mesh mesh = new Mesh(
                           Game.GameManager.GraphicsThread.GraphicsManager.Device,
                           pass.Vertices.Length / 3,
                           pass.Vertices.Length,
                           MeshFlags.Managed,
                           MdxVertex.FVF
                       );

                        VertexBuffer vb = new VertexBuffer(Game.GameManager.GraphicsThread.GraphicsManager.Device, pass.Vertices.Length * Marshal.SizeOf(typeof(Models.MDX.MdxVertex)), Usage.None,
                            Models.MDX.MdxVertex.FVF, Pool.Managed);
                        DataStream strm = vb.Lock(0, 0, LockFlags.None);
                        strm.WriteRange(pass.Vertices);
                        vb.Unlock();

                        strm = mesh.VertexBuffer.Lock(0, 0, LockFlags.None);
                        strm.WriteRange(pass.Vertices);
                        mesh.VertexBuffer.Unlock();

                        IndexBuffer ib = new IndexBuffer(Game.GameManager.GraphicsThread.GraphicsManager.Device, pass.Vertices.Length * 2, Usage.None, Pool.Managed, true);
                        strm = ib.Lock(0, 0, LockFlags.None);
                        for (int j = 0; j < pass.Vertices.Length; ++j)
                            strm.Write((short)j);
                        ib.Unlock();

                        strm = mesh.IndexBuffer.Lock(0, 0, LockFlags.None);
                        for (int j = 0; j < pass.Vertices.Length; ++j)
                            strm.Write((short)j);
                        mesh.IndexBuffer.Unlock();

                        try
                        {
                            Textures.Add(Video.TextureManager.GetTexture(pass.Texture));
                        }
                        catch (Exception)
                        {
                            Textures.Add(Video.TextureManager.Default.ErrorTexture);
                        }
                        NumTriangles.Add(pass.Vertices.Length / 3);
                        Indices.Add(ib);
                        Meshes.Add(vb);
                        mMeshPasses.Add(mesh);
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

        public void RemoveInstance(uint id)
        {
            InstanceLoader.RemoveInstance(id);
        }

        public void Unload()
        {
            InstanceLoader.Unload();
            numInstances = 0;
        }

        public List<MdxInstanceData> LockInstances()
        {
            return InstanceLoader.LockInstances();
        }

        public List<Mesh> MeshPasses { get { return mMeshPasses; } }

        public uint NumInstances { get { return InstanceLoader.GetNumInstances(); } }

        private void RenderInstances(Device dev)
        {
            InstanceLoader.UpdateVisibility();
            if (Meshes.Count == 0 || Textures.Count == 0 || InstanceDataBuffer == null || numInstances == 0)
                return;

            mModelInfo.BoneAnimator.OnFrame();

            int counter = 0;
            var shdr = Video.ShaderCollection.MDXShader;
            dev.VertexDeclaration = InstanceDeclaration;
            dev.SetRenderState(RenderState.VertexBlend, VertexBlend.Weights3);

            foreach (VertexBuffer vb in Meshes)
            {
                dev.SetStreamSource(0, vb, 0, Marshal.SizeOf(typeof(Models.MDX.MdxVertex)));
                dev.SetStreamSourceFrequency(0, numInstances, StreamSource.IndexedData);

                dev.SetStreamSource(1, InstanceDataBuffer, 0, Marshal.SizeOf(typeof(Models.MDX.MdxInstanceData)));
                dev.SetStreamSourceFrequency(1, 1, StreamSource.InstanceData);

                setRenderValues(counter);

                dev.Indices = Indices[counter];

                shdr.SetTexture("MeshTexture", Textures[counter]);
                if (mModelInfo.Passes[counter].BoneMatrices.Length == 0 || mModelInfo.Passes[counter].BoneMatrices.Length > 50)
                {
                    shdr.SetValue("useAnimation", false);
                }
                else
                {
                    shdr.SetValue<Matrix>("BoneMatrices", mModelInfo.Passes[counter].BoneMatrices);
                    shdr.SetValue("useAnimation", mModelInfo.Passes[counter].BoneMatrices.Length > 0);
                }

                shdr.DoRender((device) =>
                    {
                        dev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumTriangles[counter] * 3, 0, NumTriangles[counter]);
                    }
                );

                unsetRenderValues();

                ++counter;
            }
            dev.VertexDeclaration = null;

            dev.ResetStreamSourceFrequency(0);
            dev.ResetStreamSourceFrequency(1);
            dev.SetRenderState(RenderState.VertexBlend, VertexBlend.Disable);
        }

        private void setRenderValues(int pass)
        {
            if (pass >= mModelInfo.Passes.Count)
                throw new Exception("pass >= mModelInfo.Passes.Count");

            mModelInfo.Passes[pass].UpdatePass();

            Device dev = Game.GameManager.GraphicsThread.GraphicsManager.Device;

            var flags = mModelInfo.Passes[pass].BlendMode;
            switch (flags.blend)
            {
                case 0:
                    {
                        dev.SetRenderState(RenderState.AlphaTestEnable, true);
                        dev.SetRenderState(RenderState.AlphaBlendEnable, true);
                        dev.SetRenderState(RenderState.AlphaFunc, Compare.Greater);
                        dev.SetRenderState(RenderState.AlphaRef, 0.01f);
                        break;
                    }
                case 1:
                    {
                        dev.SetRenderState(RenderState.AlphaTestEnable, true);
                        dev.SetRenderState(RenderState.AlphaBlendEnable, true);                        
                        break;
                    }
                case 2:
                    {
                        dev.SetRenderState(RenderState.AlphaTestEnable, true);
                        dev.SetRenderState(RenderState.AlphaBlendEnable, true);
                        dev.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                        dev.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                        break;
                    }
                case 3:
                    {
                        dev.SetRenderState(RenderState.AlphaTestEnable, true);
                        dev.SetRenderState(RenderState.DestinationBlend, Blend.One);
                        dev.SetRenderState(RenderState.SourceBlend, Blend.SourceColor);
                        break;
                    }
                case 4:
                    {
                        dev.SetRenderState(RenderState.AlphaTestEnable, true);
                        dev.SetRenderState(RenderState.DestinationBlend, Blend.One);
                        dev.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                        break;
                    }

                case 6:
                case 5:
                    {
                        dev.SetRenderState(RenderState.AlphaBlendEnable, true);
                        dev.SetRenderState(RenderState.SourceBlend, Blend.DestinationColor);
                        dev.SetRenderState(RenderState.DestinationBlend, Blend.SourceColor);
                    }
                    break;

                default:
                    {
                        dev.SetRenderState(RenderState.AlphaBlendEnable, true);
                        dev.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                        dev.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                    }
                    break;
            }
        }

        private void unsetRenderValues()
        {
            Device dev = Game.GameManager.GraphicsThread.GraphicsManager.Device;
            dev.SetRenderState(RenderState.AlphaTestEnable, false);
            dev.SetRenderState(RenderState.AlphaBlendEnable, true);
            dev.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            dev.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        }

        internal List<Mesh> mMeshPasses = new List<Mesh>();
        internal List<VertexBuffer> Meshes = new List<VertexBuffer>();
        internal List<IndexBuffer> Indices = new List<IndexBuffer>();
        internal List<int> NumTriangles = new List<int>();
        internal List<Video.TextureHandle> Textures = new List<Video.TextureHandle>();
        internal VertexBuffer InstanceDataBuffer;
        internal int numInstances = 0;
        internal M2InstanceLoader InstanceLoader;
        internal VertexDeclaration InstanceDeclaration;
        internal M2Info mModelInfo;

        public string ModelName { get; set; }
        public ModelIntersector Intersector { get; private set; }

        private static VertexElement[] ElemDecl = new VertexElement[]
        {
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.BlendWeight, 0),
            new VertexElement(0, 28, DeclarationType.Ubyte4, DeclarationMethod.Default, DeclarationUsage.BlendIndices, 0),
            new VertexElement(0, 32, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
            new VertexElement(0, 44, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
            new VertexElement(1, 0, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
            new VertexElement(1, 4, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 1),
            new VertexElement(1, 8, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 1),
            new VertexElement(1, 24, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 2),
            new VertexElement(1, 40, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 3),
            new VertexElement(1, 56, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 4),            
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
            lock (mInstLock)
            {
                if (mIsHalted)
                    return true;

                if (WaitingInstances.Count > 0)
                {
                    for (int i = 0; WaitingInstances.Count > 0; ++i)
                    {
                        var data = WaitingInstances.First();
                        ActiveInstances.Add(data);
                        WaitingInstances.Remove(data);
                    }
                    if (Renderer.InstanceDataBuffer != null)
                        Renderer.InstanceDataBuffer.Dispose();
                    int size = Marshal.SizeOf(typeof(Models.MDX.MdxInstanceData));
                    Renderer.InstanceDataBuffer = new VertexBuffer(Game.GameManager.GraphicsThread.GraphicsManager.Device, ActiveInstances.Count * size, Usage.WriteOnly, VertexFormat.Diffuse, Pool.Managed);
                    DataStream strm = Renderer.InstanceDataBuffer.Lock(0, 0, LockFlags.None);
                    Models.MDX.MdxInstanceData[] InstData = ActiveInstances.ToArray();
                    strm.WriteRange(InstData);
                    Renderer.InstanceDataBuffer.Unlock();
                    Renderer.numInstances = ActiveInstances.Count;
                }
            }
            return WaitingInstances.Count == 0;
        }

        internal void UpdateVisibility()
        {
            lock (mInstLock)
            {
                if (mIsHalted)
                    return;

                List<Models.MDX.MdxInstanceData> VisibleInstances = new List<Models.MDX.MdxInstanceData>();
                List<Models.MDX.MdxInstanceData> TmpInvis = new List<Models.MDX.MdxInstanceData>();
                bool changed = false;
                foreach (var inst in InvisibleInstances)
                {
                    if (IsInstanceVisible(new Vector3(inst.ModelMatrix.M41, inst.ModelMatrix.M42, inst.ModelMatrix.M43)))
                    {
                        changed = true;
                        VisibleInstances.Add(inst);
                    }
                    else
                        TmpInvis.Add(inst);
                }

                foreach (var inst in ActiveInstances)
                {
                    if (IsInstanceVisible(new Vector3(inst.ModelMatrix.M41, inst.ModelMatrix.M42, inst.ModelMatrix.M43)))
                        VisibleInstances.Add(inst);
                    else
                    {
                        changed = true;
                        TmpInvis.Add(inst);
                    }
                }

                ActiveInstances = VisibleInstances;
                InvisibleInstances = TmpInvis;
                Renderer.numInstances = ActiveInstances.Count;
                if (ActiveInstances.Count == 0)
                    return;

                if (changed == false && mUpdateLists == false)
                    return;

                if (Renderer.InstanceDataBuffer != null)
                    Renderer.InstanceDataBuffer.Dispose();

                int size = Marshal.SizeOf(typeof(Models.MDX.MdxInstanceData));
                Renderer.InstanceDataBuffer = new VertexBuffer(Game.GameManager.GraphicsThread.GraphicsManager.Device, ActiveInstances.Count * size, Usage.WriteOnly, VertexFormat.Diffuse, Pool.Managed);
                DataStream strm = Renderer.InstanceDataBuffer.Lock(0, 0, LockFlags.None);
                Models.MDX.MdxInstanceData[] InstData = ActiveInstances.ToArray();
                strm.WriteRange(InstData);
                Renderer.InstanceDataBuffer.Unlock();
                Renderer.numInstances = ActiveInstances.Count;

                mUpdateLists = false;
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

        public List<MdxInstanceData> LockInstances()
        {
            List<MdxInstanceData> retVal = new List<MdxInstanceData>();
            lock (mInstLock)
            {
                retVal.AddRange(WaitingInstances);
                retVal.AddRange(ActiveInstances);
                retVal.AddRange(InvisibleInstances);
            }

            return retVal;
        }

        public uint PushInstance(Vector3 Position, float scale, Vector3 rotation)
        {
            Matrix matRot = Matrix.Identity;
            matRot *= Matrix.RotationX(rotation.X);
            matRot *= Matrix.RotationY(rotation.Y);
            matRot *= Matrix.RotationZ(rotation.Z);

            Matrix matTrans = Matrix.Translation(Position);
            Matrix matScale = Matrix.Scaling(scale, scale, scale);

            Models.MDX.MdxInstanceData inst = new Models.MDX.MdxInstanceData()
            {
                ModelMatrix =
                matRot * matScale * matTrans,
                IsSelected = 0xFFFFFFFF,
            };

            uint id = 0;

            lock (mInstLock)
            {
                id = RequestInstanceId();
                inst.InstanceId = id;
                WaitingInstances.Add(inst);
            }

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

            lock (mInstLock)
            {
                WaitingInstances.RemoveAll((inst) => inst.InstanceId == instanceId);
                ActiveInstances.RemoveAll((inst) => inst.InstanceId == instanceId);
                InvisibleInstances.RemoveAll((inst) => inst.InstanceId == instanceId);
            }

            lock (mIdLock)
            {
                mUsedInstances.Remove(instanceId);
                mFreeInstances.Add(instanceId);
            }

        }

        public void Unload()
        {
            lock (mInstLock)
            {
                mIsHalted = true;

                Game.GameManager.GraphicsThread.CallOnThread(
                    () =>
                    {
                        Renderer.InstanceDataBuffer.Dispose();
                        Renderer.InstanceDeclaration.Dispose();

                        foreach (var mesh in Renderer.Meshes)
                            mesh.Dispose();

                        foreach (var ib in Renderer.Indices)
                            ib.Dispose();

                        foreach (var tex in Renderer.Textures)
                            Video.TextureManager.RemoveTexture(tex);
                    },
                    true
                );

                Renderer.Meshes.Clear();
                Renderer.Indices.Clear();
            }
        }

        public uint GetNumInstances()
        {
            lock (mInstLock)
            {
                var ret = WaitingInstances.Count + ActiveInstances.Count + InvisibleInstances.Count;

                return (uint)ret;
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

        public MdxInstanceData GetInstanceById(uint id)
        {
            lock (mInstLock)
            {
                var sel = from file in WaitingInstances.Union(ActiveInstances).Union(InvisibleInstances)
                          where file.InstanceId == id
                          select file;

                if (sel.Count() == 0)
                    throw new Exception();

                return sel.First();
            }
        }

        public void setInstance(uint id, MdxInstanceData data)
        {
            lock (mInstLock)
            {
                for (int i = 0; i < WaitingInstances.Count; ++i)
                {
                    if (WaitingInstances[i].InstanceId == id)
                    {
                        WaitingInstances[i] = data;
                        return;
                    }
                }

                for (int i = 0; i < ActiveInstances.Count; ++i)
                {
                    if (ActiveInstances[i].InstanceId == id)
                    {
                        ActiveInstances[i] = data;
                        mUpdateLists = true;
                        return;
                    }
                }

                for (int i = 0; i < InvisibleInstances.Count; ++i)
                {
                    if (InvisibleInstances[i].InstanceId == id)
                    {
                        InvisibleInstances[i] = data;
                        return;
                    }
                }
            }
        }

        private List<Models.MDX.MdxInstanceData> WaitingInstances = new List<Models.MDX.MdxInstanceData>();
        private List<Models.MDX.MdxInstanceData> ActiveInstances = new List<Models.MDX.MdxInstanceData>();
        private List<Models.MDX.MdxInstanceData> InvisibleInstances = new List<Models.MDX.MdxInstanceData>();
        private M2BatchRender Renderer;
        private System.Threading.Mutex mInstLock = new System.Threading.Mutex();
        private List<uint> mFreeInstances = new List<uint>();
        private List<uint> mUsedInstances = new List<uint>();
        private object mIdLock = new object();
        private bool mIsHalted = false;
        private bool mUpdateLists = false;
    }
}
