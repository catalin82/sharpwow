using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Video
{
    public class VideoResource
    {
        public VideoResource(Func<SlimDX.Result> onRelease, Func<SlimDX.Result> afterRelease)
        {
            BeforeRelease = onRelease;
            AfterRelease = afterRelease;
        }

        public Func<SlimDX.Result> BeforeRelease, AfterRelease;
    }
}
