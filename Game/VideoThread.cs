using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Forms;
using SlimDX.Windows;
using SlimDX.Direct3D9;
using SlimDX;
using System.Drawing;

namespace SharpWoW.Game
{
    public class VideoThread
    {
        public VideoThread(Form form, Control destWindow = null)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            mWindow = form;
            mRenderWindow = (destWindow == null) ? form : destWindow;
            GraphicsManager = new Video.GraphicsManager(mRenderWindow);
        }

        /// <summary>
        /// Called by the GameManager on the startup of the application to initiate the modal game loop.
        /// Do not call this.
        /// </summary>
        public void RunLoop()
        {
            mMainThread = System.Threading.Thread.CurrentThread;
            if (isRunning == true)
                throw new Exception("VideoThread.RunLoop was already called! You cannot run the main loop twice!");

            isRunning = true;

            GraphicsManager.CreateDevice(true, true);
            UI.FontManager.init();
            CurrentOverlay = new UI.TerrainInfoOverlay();
            Game.GameManager.ActiveChangeModeChanged += () =>
                {
                    switch (Game.GameManager.ActiveChangeType)
                    {
                        case Logic.ActiveChangeType.Height:
                            CurrentOverlay = new UI.TerrainInfoOverlay();
                            break;

                        case Logic.ActiveChangeType.Texturing:
                            CurrentOverlay = new UI.TextureInfoOverlay();
                            break;
                    }
                };

            MessagePump.Run(mWindow, FrameLoop);
        }

        private void FrameLoop()
        {
            if (mLastFrame == null)
            {
                mLastFPS = DateTime.Now;
                mLastFrame = DateTime.Now;
                return;
            }

            DateTime thisFrame = DateTime.Now;
            var diff = (thisFrame - mLastFrame);

            TimeSpan diffFps = DateTime.Now - mLastFPS;
            if (diffFps.TotalSeconds >= 1.0)
            {
                double fps = mLastNumFrames * diffFps.TotalSeconds;
                string newTitle = "SharpWoW - " + fps.ToString("0.00");
                mWindow.Text = newTitle;
                mLastFPS = DateTime.Now;
                mLastNumFrames = 0;
            }

            Video.Input.InputManager.Input.Update();
            Video.ShaderCollection.UpdateTime(Game.GameManager.GameTime);

            GraphicsManager.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            GraphicsManager.Device.BeginScene();

            var dev = GraphicsManager.Device;
            dev.SetRenderState(RenderState.ZEnable, true);
            dev.SetRenderState(RenderState.Lighting, false);
            dev.SetRenderState(RenderState.CullMode, Cull.None);

            GraphicsManager.Camera.UpdateCamera(GraphicsManager.Device, diff.Value);

            ADT.ADTManager.Render();

            if (OnFrame != null)
                OnFrame(GraphicsManager.Device, diff.Value);

            GraphicsManager.UpdateMouseTerrainPos(0, 0);

            UI.FontManager.beginFrame();
            if (CurrentOverlay != null)
                CurrentOverlay.Draw();
            UI.FontManager.endFrame();

            GraphicsManager.Device.EndScene();
            GraphicsManager.Device.Present();

            mLastFrame = thisFrame;
            ++mLastNumFrames;
        }

        public void CallOnThread(Action action, bool async = false)
        {
            if (!mWindow.InvokeRequired)
                action();
            else
            {
                if (async == false)
                    mWindow.Invoke(action);
                else
                    mWindow.BeginInvoke(new Action(action));
            }
        }

        public delegate void FrameRenderDelegate(Device device, TimeSpan deltaTime);
        public event FrameRenderDelegate OnFrame;

        public Video.GraphicsManager GraphicsManager { get; private set; }

        private Form mWindow;
        private bool isRunning = false;
        private Control mRenderWindow;
        private DateTime? mLastFrame = null;
        private DateTime mLastFPS;
        private System.Threading.Thread mMainThread;
        private uint mLastNumFrames = 0;
        private UI.InterfaceOverlay CurrentOverlay = null;
    }
}
