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
            PushOverlay(new UI.TerrainInfoOverlay());
            Game.GameManager.ActiveChangeModeChanged += () =>
                {
                    switch (Game.GameManager.ActiveChangeType)
                    {
                        case Logic.ActiveChangeType.Height:
                            if (GetOverlay<UI.TerrainInfoOverlay>() == null)
                                PushOverlay(new UI.TerrainInfoOverlay());
                            if (GetOverlay<UI.TextureInfoOverlay>() != null)
                                RemoveOverlay<UI.TextureInfoOverlay>();
                            break;

                        case Logic.ActiveChangeType.Texturing:
                            if (GetOverlay<UI.TextureInfoOverlay>() == null)
                                PushOverlay(new UI.TextureInfoOverlay());
                            if (GetOverlay<UI.TerrainInfoOverlay>() != null)
                                RemoveOverlay<UI.TerrainInfoOverlay>();
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

            Game.GameManager.SelectionManager.renderSelection();

            GraphicsManager.UpdateMouseTerrainPos(0, 0);

            UI.FontManager.beginFrame();

            foreach (var overlay in mOverlays)
                overlay.Draw();

            UI.FontManager.endFrame();

            try
            {
                GraphicsManager.Device.EndScene();
                GraphicsManager.Device.Present();
            }
            catch (SlimDX.Direct3D9.Direct3D9Exception e)
            {
                if (e.ResultCode.Code == ((1 << 31) | (0x876 << 16) | 2152))
                    GraphicsManager.DoDeviceReset();
                else
                    throw;
            }

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

        public T GetOverlay<T>() where T : UI.InterfaceOverlay
        {
            lock (mOverlays)
            {
                var qry = from overlay in mOverlays
                          where overlay.GetType() == typeof(T)
                          select overlay;

                if (qry.Count() == 0)
                    return null;

                return (T)qry.First();
            }
        }

        public void PushOverlay(UI.InterfaceOverlay overlay)
        {
            lock (mOverlays)
            {
                if (mOverlays.Contains(overlay) == true)
                    return;

                mOverlays.Add(overlay);
            }
        }

        public void RemoveOverlay<T>() where T : UI.InterfaceOverlay
        {
            lock (mOverlays)
            {
                mOverlays.RemoveAll((overlay) => overlay.GetType() == typeof(T));
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
        private List<UI.InterfaceOverlay> mOverlays = new List<UI.InterfaceOverlay>();
    }
}
