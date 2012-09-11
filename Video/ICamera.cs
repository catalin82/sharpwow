using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;

namespace SharpWoW.Video
{
    public interface ICamera
    {
        Vector3 Position { get; }
        Vector3 Target { get; }
        Vector3 Front { get; }
        Vector3 Up { get; }
        Matrix ViewProj { get; }
        Frustum ViewFrustum { get; }
        void UpdateCamera(Device dev, TimeSpan diff);
        void SetPosition(Vector3 position, bool nonWorldUpdate = false);
        void DeviceAttached(Device dev);
    }
}
