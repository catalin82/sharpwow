using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MathNet.Numerics.Interpolation;

namespace SharpWoW.UI
{
    public unsafe partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Game.GameManager.PropertyChanged += new Action<Game.GameProperties>(PropertyChanged);
            SharpWoW.Controls.ExpanderControl exp = new SharpWoW.Controls.ExpanderControl();
            exp.Host.Child = uiPanel1;
            elementHost1.Child = exp;
            panel1.MouseEnter += (sender, args) => { if(exp.IsStatic == false) exp.Collapse(); };
            menuStrip1.MouseEnter += (sender, args) => { if (exp.IsStatic == false) exp.Collapse(); };
            statusStrip1.MouseEnter += (sender, args) => { if (exp.IsStatic == false) exp.Collapse(); };
            
            exp.ExpandedChanged += (expanded) =>
                {
                    if (expanded)
                    {
                        elementHost1.Width = 360;
                    }
                    else
                    {
                        elementHost1.Width = 31;
                    }
                };

            
        }

        void PropertyChanged(Game.GameProperties prop)
        {
            var chunk = Game.GameManager.WorldManager.HoveredChunk;
            if (chunk == null)
            {
                toolStripStatusLabel3.Text = "Area: (unknown)";
                return;
            }

            try
            {
                var ae = DBC.DBCStores.AreaTable[chunk.Header.areaId];
                toolStripStatusLabel3.Text = "Area: " + ae.AreaName;
            }
            catch (Exception)
            {
                toolStripStatusLabel3.Text = "Area: (unknown)";
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            panel1.Width = this.Width - 350;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Game.GameManager.OnFormLoaded();
        }

        public void OnGameInitialized()
        {
            toolStripStatusLabel1.Text = "Used path: " + Game.GameManager.GamePath;
            Game.GameManager.GraphicsThread.OnFrame += new Game.VideoThread.FrameRenderDelegate(UpdateLogic);
            WMOEditor = new Dialogs.WMOEditor();
        }

        void UpdateLogic(SlimDX.Direct3D9.Device device, TimeSpan deltaTime)
        {
            var pos = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position;
            var str = "Camera: X: " + pos.X.ToString("0.00");
            str += " Y: " + pos.Y.ToString("0.00");
            str += " Z: " + pos.Z.ToString("0.00");
            toolStripStatusLabel2.Text = str;

            float time = (float)((Game.GameManager.GameTime.TotalMilliseconds / 10.0f) % 2880);
            time /= 2.0f;
            TimeSpan span = TimeSpan.FromMinutes(time);
            toolStripStatusLabel4.Text = "Time of day: " + span.Hours.ToString("D2") + ":" + span.Minutes.ToString("D2");
        }

        private void editorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dialogs.DBCEditor dbc = new Dialogs.DBCEditor();
            dbc.ShowDialog();
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            panel1.Focus();
        }

        public MathNet.Numerics.Interpolation.IInterpolationMethod TerrainSpline { get { return uiPanel1.TerrainSpline; } }
        public string SelectedTexture { get { return uiPanel1.SelectedTexture; } }
        public UI.Dialogs.WMOEditor WMOEditor { get; private set; }
        public Controls.UIPanel ToolsPanel { get { return uiPanel1; } }
        public Controls.PropertyTab PropertyPanel { get { return uiPanel1.PropertyPanel; } }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (Video.Input.InputManager.Input.IsAsyncKeyDown(Keys.ShiftKey) == true || Video.Input.InputManager.Input.IsAsyncKeyDown(Keys.ControlKey) == true)
                panel1.Focus();
        }

        private void loadMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dialogs.MapSelectDialog dlg = new Dialogs.MapSelectDialog();
            dlg.ShowDialog();
            if (dlg.SelectedMap == null)
                return;

            Dialogs.MinimapDialog minimap = new Dialogs.MinimapDialog();
            minimap.SetMap(dlg.SelectedMap.Map);
            minimap.PointSelected += new SharpWoW.UI.Dialogs.MinimapDialog.MinimapSelectedDlg(_PointSelected);
            minimap.ShowDialog();
        }

        void _PointSelected(uint mapid, string continent, float x, float y)
        {
            Game.GameManager.WorldManager.EnterWorld(mapid, continent, x, y);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Dialogs.BookmarksDialog bookDialog = new Dialogs.BookmarksDialog();
            bookDialog.ShowDialog();
        }
    }
}
