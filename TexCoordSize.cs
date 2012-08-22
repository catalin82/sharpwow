using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW
{
    class TexCoordSize3
    {
        public TexCoordSize3(int index)
        {
            mIndex = index;
        }

        public static implicit operator SlimDX.Direct3D9.VertexFormat(TexCoordSize3 coord)
        {
            return (SlimDX.Direct3D9.VertexFormat)(1 << (coord.mIndex * 2 + 16));
        }

        int mIndex;
    }
}
