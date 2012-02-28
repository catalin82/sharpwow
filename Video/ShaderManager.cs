using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Reflection;

namespace SharpWoW.Video
{
    public class ShaderManager
    {
        public ShaderManager()
        {
            mAssembly = Assembly.GetExecutingAssembly();
        }

        public void PushResourceLocation(string name, Assembly asm = null)
        {
            if (asm == null)
                asm = mAssembly;

            mManagers.Add(new ResourceManager(name, asm));
        }

        public Shader GetShader(string name)
        {
            foreach (var mgr in mManagers)
            {
                try
                {
                    return new Shader(mgr.GetObject(name) as byte[]);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return null;
        }

        public bool HasShader(string name)
        {
            foreach (var mgr in mManagers)
            {
                try
                {
                    var strm = mgr.GetStream(name);
                }
                catch (Exception)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private List<ResourceManager> mManagers = new List<ResourceManager>();
        private Assembly mAssembly;

        public static ShaderManager Shaders { get; private set; }

        static ShaderManager()
        {
            Shaders = new ShaderManager();
            Shaders.PushResourceLocation("SharpWoW.Resources.Shaders");
        }
    }
}
