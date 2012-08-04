using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SharpWoW.SimpleRender
{
    public class ControlRender
    {
        private Device mDevice;
        private Direct3D mDriver;
        private Control mRenderWindow;
        private DateTime mLastFrame = DateTime.Now;

        public Form ParentForm { get; set; }
        public ModelCamera Camera { get; private set; }
        public Video.Input.InputManager InputManager { get; private set; }
        public Video.TextureManager TextureManager { get; private set; }
        public event Action OnFrame;
        public bool RunLoopCreated = false;
        public Device Device { get { return mDevice; } }
        public event Action LoadDone;
        public System.Threading.AutoResetEvent LoadEvent = new System.Threading.AutoResetEvent(false);

        public ControlRender(Control ctrl, Form parent)
        {
            ParentForm = parent;
            mRenderWindow = ctrl;
        }

        public void RenderFrame()
        {
            mDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.Black, 1, 0);
            mDevice.BeginScene();

            var now = DateTime.Now;
            var diff = now - mLastFrame;
            mLastFrame = now;

            Camera.UpdateCamera(diff);

            OnFrame();

            mDevice.EndScene();
            mDevice.Present();
        }

        public void RunLoop()
        {
            RunLoopCreated = true;
            mRunThread = new System.Threading.Thread(() =>
                {
                    ParentForm.Visible = true;
                    ParentForm.Visible = false;

                    var ctrl = mRenderWindow;
                    mDriver = new Direct3D();
                    mDevice = new Device(mDriver, 0, DeviceType.Hardware, ctrl.Handle, CreateFlags.HardwareVertexProcessing
                        | CreateFlags.Multithreaded,
                        new PresentParameters()
                        {
                            Windowed = true,
                            BackBufferFormat = Format.A8R8G8B8,
                            BackBufferHeight = ctrl.ClientSize.Height,
                            BackBufferWidth = ctrl.ClientSize.Width,
                            EnableAutoDepthStencil = true,
                            AutoDepthStencilFormat = Format.D24S8,
                            SwapEffect = SwapEffect.Discard,
                            DeviceWindowHandle = ctrl.Handle,
                        }
                        );

                    mDevice.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH((45.0f * (float)Math.PI) / 180.0f, ctrl.ClientSize.Width / (float)ctrl.ClientSize.Height, 0.1f, 100000.0f));
                    mDevice.SetRenderState(RenderState.Lighting, false);
                    mDevice.SetRenderState(RenderState.ZEnable, true);
                    mDevice.SetRenderState(RenderState.CullMode, Cull.None);
                    mDevice.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
                    mDevice.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
                    mDevice.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Linear);

                    InputManager = new Video.Input.InputManager();
                    InputManager.InputWindow = ctrl;
                    Camera = new ModelCamera(ctrl, this);
                    TextureManager = new Video.TextureManager(mDevice);

                    Application.Idle += (sender, args) =>
                        {
                            if (ParentForm.Visible == false)
                                return;

                            MSG msg = new MSG();
                            while (!PeekMessage(ref msg, IntPtr.Zero, 0, 0, 0))
                                RenderFrame();
                        };

                    if (LoadDone != null)
                        LoadDone();

                    LoadEvent.Set();

                    ParentForm.Visible = true;
                    Application.Run(ParentForm);
                }
            );

            mRunThread.Start();
            Game.GameManager.GameTerminated += () =>
                {
                    if (ParentForm.IsDisposed == false)
                        ParentForm.Invoke(new Action(ParentForm.Dispose));
                };
        }

        private System.Threading.Thread mRunThread = null;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            IntPtr hwnd;
            uint message, wParam, lParam, time;
            int ptX, ptY;
        }

        [DllImport("User32.dll")]
        private static extern bool PeekMessage(ref MSG msg, IntPtr handle, uint minMsg, uint maxMsg, uint remove);
    }
}
