using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace SharpWoW.Controls
{
    public partial class MinimapControl : UserControl
    {
        public MinimapControl()
        {
            InitializeComponent();
            Paint += new PaintEventHandler(paintMap);
            MouseDown += new MouseEventHandler(_MouseDown);
            MouseUp += new MouseEventHandler(_MouseUp);
            MouseMove += new MouseEventHandler(_MouseMove);
            MouseWheel += new MouseEventHandler(_MouseWheel);
            MouseClick += new MouseEventHandler(_MouseClick);
        }

        void _MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && mMinimap != null)
            {
                float totalRange = 64.0f * Utils.Metrics.Tilesize;
                float stepX = totalRange / mMinimap.Width;
                float stepY = totalRange / mMinimap.Height;
                float offsetX = stepX * mSrcRectangle.X;
                float offsetY = stepY * mSrcRectangle.Y;
                stepX = (totalRange / Width) * mZoomFactor;
                stepY = (totalRange / Height) * mZoomFactor;
                float coordX = offsetX + stepX * e.X;
                float coordY = offsetY + stepY * e.Y;

                if (PointSelected != null)
                    PointSelected(coordX, coordY);
            }
        }

        void _MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                mZoomFactor += 0.1f;
            if (e.Delta < 0)
                mZoomFactor -= 0.1f;

            if (mZoomFactor < 0.1)
                mZoomFactor = 0.1f;

            updateRectangle();
        }

        void _MouseMove(object sender, MouseEventArgs e)
        {
            if (mRightDown)
            {
                mTranslation.X -= (e.X - mLastPoint.X) * mZoomFactor;
                mTranslation.Y -= (e.Y - mLastPoint.Y) * mZoomFactor;
                mLastPoint = new Point(e.X, e.Y);
                updateRectangle();
            }

            float totalRange = 64.0f * Utils.Metrics.Tilesize;
            float stepX = totalRange / mMinimap.Width;
            float stepY = totalRange / mMinimap.Height;
            float offsetX = stepX * mSrcRectangle.X;
            float offsetY = stepY * mSrcRectangle.Y;
            stepX = (totalRange / Width) * mZoomFactor;
            stepY = (totalRange / Height) * mZoomFactor;
            float coordX = offsetX + stepX * e.X;
            float coordY = offsetY + stepY * e.Y;
            label1.Text = "ADT: " + (uint)(coordX / Utils.Metrics.Tilesize) + "/" + (uint)(coordY / Utils.Metrics.Tilesize);
        }

        void _MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                mRightDown = false;
        }

        void _MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                mRightDown = true;
                mLastPoint = new Point(e.X, e.Y);
            }
        }

        void paintMap(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            if (mMinimap != null)
                g.DrawImage(mMinimap, new RectangleF(0, 0, Width, Height), mSrcRectangle, GraphicsUnit.Pixel);
        }

        private void ImageChanged()
        {
            updateRectangle();
        }

        private void updateRectangle()
        {
            if (mMinimap == null)
                return;

            mSrcRectangle = new RectangleF(mTranslation.X + ((1 - mZoomFactor) * mMinimap.Width) / 2.0f, mTranslation.Y + ((1 - mZoomFactor) * mMinimap.Height) / 2.0f, mMinimap.Width * mZoomFactor, mMinimap.Height * mZoomFactor);
            Invalidate();
        }

        public Bitmap Minimap { get { return mMinimap; } set { mMinimap = value; ImageChanged(); } }
        private Bitmap mMinimap = null;
        private PointF mTranslation = new PointF(0, 0);
        private bool mRightDown = false;
        private Point mLastPoint = Point.Empty;
        private float mZoomFactor = 1.0f;
        private RectangleF mSrcRectangle;

        public delegate void PointSelectedDlg(float x, float y);
        public event PointSelectedDlg PointSelected;

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            if (label1.Dock == DockStyle.Left)
                label1.Dock = DockStyle.Right;
            else
                label1.Dock = DockStyle.Left;
        }
    }
}
