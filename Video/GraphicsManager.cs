using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using System.Runtime.InteropServices;
using SlimDX;

namespace SharpWoW.Video
{
    public class GraphicsManager
    {
        public GraphicsManager(Control dstWindow)
        {
            mRenderWindow = dstWindow;
            mRenderWindow.MouseMove += new MouseEventHandler(MouseMoved);
            Input.InputManager.Input.InputWindow = mRenderWindow;
        }

        void MouseMoved(object sender, MouseEventArgs e)
        {
            //UpdateMouseTerrainPos(e.X, e.Y);
        }

        public void CreateDevice(bool useRegistry, bool showDialog = true)
        {
            Direct3D = new SlimDX.Direct3D9.Direct3D();
            VideoConfig cfg = VideoConfig.Load(useRegistry, showDialog);

            Device = new Device(Direct3D, cfg.Adapter.Adapter, DeviceType.Hardware, mRenderWindow.Handle,
                CreateFlags.HardwareVertexProcessing,
                new PresentParameters()
                {
                    AutoDepthStencilFormat = cfg.DepthStencilFormat,
                    EnableAutoDepthStencil = true,
                    BackBufferFormat = Format.A8R8G8B8,
                    BackBufferHeight = mRenderWindow.ClientSize.Height,
                    BackBufferWidth = mRenderWindow.ClientSize.Width,
                    DeviceWindowHandle = mRenderWindow.Handle,
                    Windowed = true,
                    Multisample = cfg.Multisampling,
                    MultisampleQuality = (int)cfg.MultisampleQuality
                }
            );

            Camera = new Camera();
            Device.SetTransform(TransformState.Projection, SlimDX.Matrix.PerspectiveFovLH(
                45.0f * ((float)Math.PI / 180.0f), 
                (float)mRenderWindow.ClientSize.Width / (float)mRenderWindow.ClientSize.Height,
                0.1f,
                1000.0f
            ));
            Device.SetTransform(TransformState.World, Matrix.Identity);

        }

        public void UpdateMouseTerrainPos(int x, int y)
        {
            System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
            pt = mRenderWindow.PointToClient(pt);

            Vector3 screenCoord = new Vector3();
            screenCoord.X = (((2.0f * pt.X) / Device.Viewport.Width) - 1);
            screenCoord.Y = -(((2.0f * pt.Y) / Device.Viewport.Height) - 1);

            var invProj = Matrix.Invert(Device.GetTransform(TransformState.Projection));
            var invView = Matrix.Invert(Device.GetTransform(TransformState.View));

            var nearPos = new Vector3(screenCoord.X, screenCoord.Y, 0);
            var farPos = new Vector3(screenCoord.X, screenCoord.Y, 1);

            nearPos = Vector3.TransformCoordinate(nearPos, invProj * invView);
            farPos = Vector3.TransformCoordinate(farPos, invProj * invView);

            Ray ray = new Ray(nearPos, Vector3.Normalize((farPos - nearPos)));
            float distance = 0;
            bool hit = ADT.ADTManager.Intersect(ray, ref distance);
            ShaderCollection.TerrainShader.SetValue("DrawMouse", hit);
            if (hit)
            {
                ShaderCollection.TerrainShader.SetValue("MousePosition", ray.Position + distance * ray.Direction);
            }
        }

        public Vector3 MousePosition
        {
            get
            {
                System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
                pt = mRenderWindow.PointToClient(pt);

                Vector3 screenCoord = new Vector3();
                screenCoord.X = (((2.0f * pt.X) / Device.Viewport.Width) - 1);
                screenCoord.Y = -(((2.0f * pt.Y) / Device.Viewport.Height) - 1);

                var invProj = Matrix.Invert(Device.GetTransform(TransformState.Projection));
                var invView = Matrix.Invert(Device.GetTransform(TransformState.View));

                var nearPos = new Vector3(screenCoord.X, screenCoord.Y, 0);
                var farPos = new Vector3(screenCoord.X, screenCoord.Y, 1);

                nearPos = Vector3.TransformCoordinate(nearPos, invProj * invView);
                farPos = Vector3.TransformCoordinate(farPos, invProj * invView);

                Ray ray = new Ray(nearPos, Vector3.Normalize((farPos - nearPos)));
                float distance = 0;
                bool hit = ADT.ADTManager.Intersect(ray, ref distance);
                if (!hit)
                    return new Vector3(999999, 999999, 999999);

                return ray.Position + distance * ray.Direction;
            }
        }

        Control mRenderWindow;

        public Device Device { get; private set; }
        public Direct3D Direct3D { get; private set; }
        public Camera Camera { get; private set; }
    }
}
