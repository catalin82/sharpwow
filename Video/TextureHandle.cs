using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D9;

namespace SharpWoW.Video
{
    public class TextureHandle
    {
        public TextureHandle(Texture texture)
        {
            mTexture = texture;
        }

        public Texture Native { get { return mTexture; } }
        public string Name { get; set; }

        private Texture mTexture;
    }
}
