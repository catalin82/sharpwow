using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Video
{
    public class VideoResourceManager
    {
        public void AddVideoResource(VideoResource resource)
        {
            mResources.Add(resource);
        }

        public void RemoveVideoResource(VideoResource resource)
        {
            mResources.RemoveAll((res) => res == resource);
        }

        public void BeforeReset()
        {
            foreach (var res in mResources)
                res.BeforeRelease();
        }

        public void AfterReset()
        {
            foreach (var res in mResources)
                res.AfterRelease();
        }

        private List<VideoResource> mResources = new List<VideoResource>();
    }
}
