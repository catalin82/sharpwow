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
using MathNet.Numerics.Interpolation;

namespace SharpWoW.Controls
{
    public partial class SplineControl : UserControl
    {
        public SplineControl()
        {
            InitializeComponent();
            mSplineInterpolation = Interpolation.CreateRational(new double[] { 0, mMidPoint.X, 1 }, new double[] { 1, mMidPoint.Y, 0 });
            Paint += new PaintEventHandler(paintControl);
            MouseDown += new MouseEventHandler(mousePressed);
            MouseUp += new MouseEventHandler(mouseReleased);
            MouseMove += new MouseEventHandler(mouseMoved);
        }

        void mouseMoved(object sender, MouseEventArgs e)
        {
            if (IsLeftDown)
            {
                int x = Math.Min(Math.Max(1, e.X), Width - 1);
                int y = Math.Min(Math.Max(1, e.Y), Height - 1);
                mMidPoint = new PointF(x / (float)Width, (Height - y) / (float)Height);
                mSplineInterpolation = Interpolation.CreateRational(new double[] { 0, mMidPoint.X, 1 }, new double[] { 1, mMidPoint.Y, 0 });
                Invalidate();
                if (SplineChanged != null)
                    SplineChanged();
            }
        }

        void mouseReleased(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                IsLeftDown = false;
        }

        void mousePressed(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                IsLeftDown = true;
        }

        void paintControl(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            Point[] pathPoints = new Point[Width];

            for (int i = 0; i < Width; ++i)
            {
                double val = (double)i / Width;
                double h = mSplineInterpolation.Interpolate(val);
                pathPoints[i] = new Point(i, Height - (int)(h * Height));
            }

            e.Graphics.DrawLines(mDrawPen, pathPoints);
            var rcPosX = (int)(mMidPoint.X * Width);
            var rcPosY = Height - (int)(mMidPoint.Y * Height);
            e.Graphics.FillRectangle(mDrawBrush, new Rectangle(rcPosX - 3, rcPosY - 3, 7, 7));
        }

        private IInterpolationMethod mSplineInterpolation = null;
        private bool IsLeftDown = false;
        private PointF mMidPoint = new PointF(0.5f, 0.1f);
        private Pen mDrawPen = new Pen(Color.Red, 3.0f);
        private Brush mDrawBrush = new SolidBrush(Color.Orange);

        public IInterpolationMethod Spline { get { return mSplineInterpolation; } }
        public event Action SplineChanged;
    }
}
