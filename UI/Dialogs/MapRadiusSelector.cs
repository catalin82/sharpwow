using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpWoW.UI.Dialogs
{
    public partial class MapRadiusSelector : Form
    {
        public MapRadiusSelector()
        {
            InitializeComponent();
        }

        public void SetInfo(DBC.MapEntry map, float lightX, float lightY)
        {
            mLightPos = new PointF(lightX, lightY);
            mEntry = map;
            try
            {
                ADT.Minimap minimap = new ADT.Minimap(map.InternalName, map.ID);
                InitialImage = minimap.CreateImage().Clone() as Bitmap;
                DrawLightPoint(lightX, lightY);
                minimapControl1.Minimap = InitialImage.Clone() as Bitmap;
                DrawLightRadius();
                ContinentName = map.InternalName;
                minimapControl1.PointSelected += new SharpWoW.Controls.MinimapControl.PointSelectedDlg(_PointSelected);
            }
            catch (Exception)
            {
                Close();
            }
        }

        private void DrawLightPoint(float lightX, float lightY)
        {
            float realWidth = Utils.Metrics.Tilesize * 64;
            float stepX = realWidth / (64 * 17);
            float stepY = realWidth / (64 * 17);
            Graphics g = Graphics.FromImage(InitialImage);
            g.FillEllipse(new SolidBrush(Color.Black), new RectangleF((lightX / stepX) - 2, (lightY / stepY) - 2, 4, 4));
        }

        private void DrawLightRadius()
        {
            float realWidth = Utils.Metrics.Tilesize * 64;
            float stepX = realWidth / (64 * 17);
            float stepY = realWidth / (64 * 17);

            minimapControl1.Minimap.Dispose();
            var newImg = InitialImage.Clone() as Bitmap;
            Graphics g = Graphics.FromImage(newImg);
            var iRadius = InnerRadius / stepX;
            var oRadius = OuterRadius / stepX;
            g.DrawEllipse(new Pen(Color.Orange, 3), new RectangleF(mLightPos.X / stepX - iRadius, mLightPos.Y / stepY - iRadius, iRadius * 2, iRadius * 2));
            g.DrawEllipse(new Pen(Color.Red, 3), new RectangleF(mLightPos.X / stepX - oRadius, mLightPos.Y / stepY - oRadius, oRadius * 2, oRadius * 2));
            minimapControl1.Minimap = newImg;
        }

        void setInnerRadius(float x, float y)
        {
            float dx = x - mLightPos.X;
            float dy = y - mLightPos.Y;

            float radius = (float)Math.Sqrt(dx * dx + dy * dy);
            InnerRadius = radius;
            if (InnerRadius >= OuterRadius)
                OuterRadius = InnerRadius + 1;

            DrawLightRadius();
            if (RadiusChanged != null)
                RadiusChanged(this);
        }

        void setOuterRadius(float x, float y)
        {
            float dx = x - mLightPos.X;
            float dy = y - mLightPos.Y;

            float radius = (float)Math.Sqrt(dx * dx + dy * dy);
            OuterRadius = radius;
            if (InnerRadius >= OuterRadius)
                InnerRadius = OuterRadius - 1;

            DrawLightRadius();
            if (RadiusChanged != null)
                RadiusChanged(this);
        }

        void _PointSelected(float x, float y)
        {
            if (radioButton1.Checked == true)
                setInnerRadius(x, y);
            else if (radioButton2.Checked == true)
                setOuterRadius(x, y);
        }

        public string ContinentName { get; set; }
        public delegate void MinimapSelectedDlg(uint mapid, string continent, float x, float y);
        public event Action<MapRadiusSelector> RadiusChanged;

        private DBC.MapEntry mEntry;
        private Bitmap InitialImage = null;
        private PointF mLightPos;

        public float InnerRadius { get; set; }
        public float OuterRadius { get; set; }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            minimapControl1.Focus();
        }
    }
}
