using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpWoW.UI.Dialogs
{
    public partial class WMOEditor : Form
    {
        public WMOEditor()
        {
            InitializeComponent();
            Renderer = new SimpleRender.ControlRender(simpleRenderControl1, this);

            FormClosing += (obj, e) =>
                {
                    e.Cancel = true;
                    Visible = false;
                    mCurrentFile = null;
                };

            Renderer.OnFrame += new Action(Renderer_OnFrame);
        }

        void Renderer_OnFrame()
        {
            lock (lockObj)
            {
                if (mCurrentFile != null)
                    mCurrentFile.Draw(SlimDX.Matrix.Identity, true);
            }
        }

        void CallOnThread(Action ac)
        {
            if (InvokeRequired)
                Invoke(ac);
            else
                ac();
        }

        public void SetWMO(string wmo)
        {
            CallOnThread(() => Text = "Editing: " + wmo);
            if (Renderer.RunLoopCreated == false)
            {
                Renderer.RunLoop();
                Renderer.LoadEvent.WaitOne();
                lock (lockObj) mCurrentFile = new Models.WMO.WMOFile(wmo, Renderer.TextureManager);
                WMOLoaded();
                return;
            }
            else
                CallOnThread(() => Visible = true);

            lock (lockObj) mCurrentFile = new Models.WMO.WMOFile(wmo, Renderer.TextureManager);
            WMOLoaded();
        }

        private void WMOLoaded()
        {
            CallOnThread(() =>
                {
                    Renderer.Camera.SetPosition(1.1f * mCurrentFile.BoundingRadius, mCurrentFile.Center.Z);
                    LoadPropertiesPanel();
                }
            );
        }

        public SimpleRender.ControlRender Renderer { get; private set; }
        Models.WMO.WMOFile mCurrentFile;
        object lockObj = new object();

        private void LoadPropertiesPanel()
        {
            propertyGrid1.SelectedObject = null;
            propertyGrid1.SelectedObject = new Models.WMO.WMOEditDescriptor(mCurrentFile, this);
        }
    }
}
