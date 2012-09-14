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
using System.Drawing.Imaging;

namespace SharpWoW.Controls
{
    public partial class MinimapControl : UserControl
    {
        public MinimapControl()
        {
            InitializeComponent();
            checkBox1.Tag = 0;
            checkBox1.MouseHover += checkBox1_MouseHover;
            mInfoQuery = ADT.IBasicInfoQuery.Create();

            Paint += new PaintEventHandler(paintMap);
            MouseDown += new MouseEventHandler(_MouseDown);
            MouseUp += new MouseEventHandler(_MouseUp);
            MouseMove += new MouseEventHandler(_MouseMove);
            MouseWheel += new MouseEventHandler(_MouseWheel);
            MouseClick += new MouseEventHandler(_MouseClick);
            label2.Location = new Point(0, 20);
            label2.Visible = false;
            Load += MinimapControl_HandleCreated;
        }

        void checkBox1_MouseHover(object sender, EventArgs e)
        {
            int oldVal = (int)checkBox1.Tag;
            ++oldVal;
            oldVal %= 4;
            checkBox1.Tag = oldVal;
            switch (oldVal)
            {
                case 0:
                    checkBox1.Location = new Point(0, 0);
                    break;

                case 1:
                    checkBox1.Location = new Point(Parent.ClientSize.Width - checkBox1.Width, 0);
                    break;

                case 2:
                    checkBox1.Location = new Point(Parent.ClientSize.Width - checkBox1.Width, Parent.ClientSize.Height - checkBox1.Height);
                    break;
                    
                case 3:
                    checkBox1.Location = new Point(0, Parent.ClientSize.Height - checkBox1.Height);
                    break;
            }
        }

        void MinimapControl_HandleCreated(object sender, EventArgs e)
        {
            mOrigSize = Parent.Size;
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
            if (DrawOverlay == false)
            {
                if (mMinimap != null)
                    g.DrawImage(mMinimap, new RectangleF(0, 0, Width, Height), mSrcRectangle, GraphicsUnit.Pixel);
            }
            else
            {
                if (mStaticOverlay != null)
                    g.DrawImage(mStaticOverlay, new RectangleF(0, 0, Width, Height), mSrcRectangle, GraphicsUnit.Pixel);

                g.DrawString(Resources.Strings.WorldMapMinimapWarning, mWarnFont, mBrush, new PointF(0, 30));
            }
        }

        private void ImageChanged()
        {
            updateRectangle();
        }

        private void updateRectangle()
        {
            if ((DrawOverlay == false && mMinimap == null) || (DrawOverlay && mStaticOverlay == null))
                return;

            int width = (DrawOverlay ? StaticOverlay.Width : mMinimap.Width);
            int height = (DrawOverlay ? StaticOverlay.Height : mMinimap.Height);

            mSrcRectangle = new RectangleF(mTranslation.X + ((1 - mZoomFactor) * width) / 2.0f, mTranslation.Y + ((1 - mZoomFactor) * height) / 2.0f, width * mZoomFactor, height * mZoomFactor);
            Invalidate();
        }

        public Bitmap StaticOverlay { get { return mStaticOverlay; } set { mStaticOverlay = value; ImageChanged(); } }
        public Bitmap Minimap { get { return mMinimap; } set { mMinimap = value; ImageChanged(); } }
        public DBC.MapEntry MapEntry { get; set; }
        public bool DrawOverlay
        {
            get
            {
                return mDrawOverlay;
            }

            set
            {
                mDrawOverlay = value;
                if (value)
                {
                    float aspect = 1002.0f / 667.0f;
                    Parent.Width = (int)(mOrigSize.Height * aspect);
                }
                else
                {
                    Parent.Width = mOrigSize.Width;
                }

                updateRectangle();
            }
        }

        private bool mDrawOverlay = false;
        private Bitmap mMinimap = null;
        private Bitmap mStaticOverlay = null;
        private PointF mTranslation = new PointF(0, 0);
        private bool mRightDown = false;
        private Point mLastPoint = Point.Empty;
        private float mZoomFactor = 1.0f;
        private RectangleF mSrcRectangle;
        private ADT.IBasicInfoQuery mInfoQuery;
        private Size mOrigSize;
        private Font mWarnFont = new Font(new FontFamily("Lucida Sans Unicode"), 14.0f, FontStyle.Bold);
        private SolidBrush mBrush = new SolidBrush(Color.Red);

        public delegate void PointSelectedDlg(float x, float y);
        public event PointSelectedDlg PointSelected;

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            if (label1.Dock == DockStyle.Left)
                label1.Dock = DockStyle.Right;
            else
                label1.Dock = DockStyle.Left;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            DrawOverlay = checkBox1.Checked;
        }
    }
}
