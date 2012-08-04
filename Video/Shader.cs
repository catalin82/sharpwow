using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D9;

namespace SharpWoW.Video
{
    public class Shader
    {
        public Shader(byte[] data)
        {
            try
            {
                mEffect = Effect.FromMemory(Game.GameManager.GraphicsThread.GraphicsManager.Device, data, ShaderFlags.OptimizationLevel3);
            }
            catch (Exception e)
            {
            }
        }

        public Shader(string fileName)
            : this(System.IO.File.ReadAllBytes(fileName))
        {

        }

        public void SetTexture(string name, TextureHandle handle)
        {
            mEffect.SetTexture(GetHandle(name), handle.Native);
        }

        public void SetTexture(string name, Texture texture)
        {
            mEffect.SetTexture(GetHandle(name), texture);
        }

        public void SetValue<T>(string name, T value) where T : struct
        {
            mEffect.SetValue(GetHandle(name), value);
        }

        public void DoRender(Action<Device> render)
        {
            var passes = mEffect.Begin();
            for (int i = 0; i < passes; ++i)
            {
                mEffect.BeginPass(i);
                render(mEffect.Device);
                mEffect.EndPass();
            }
            mEffect.End();
        }

        public void SetTechnique(uint index)
        {
            mEffect.Technique = mEffect.GetTechnique((int)index);
        }

        EffectHandle GetHandle(string name)
        {
            if (mHandles.ContainsKey(name.GetHashCode()))
                return mHandles[name.GetHashCode()];

            var handle = mEffect.GetParameter(null, name);
            mHandles.Add(name.GetHashCode(), handle);
            return handle;
        }

        Effect mEffect = null;
        Dictionary<int, EffectHandle> mHandles = new Dictionary<int, EffectHandle>();
    }
}
