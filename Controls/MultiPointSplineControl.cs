using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.Interpolation;

namespace SharpWoW.Controls
{
    public enum SplineInterpolationMethod
    {
        Polynomial,
        Cubic,
        Linear,
    }

    public partial class MultiPointSplineControl : UserControl
    {
        public MultiPointSplineControl()
        {
            InitializeComponent();
            mPoints.Add(new PointF(0.5f, 0.1f));
            SetInterpolationMethod(SplineInterpolationMethod.Polynomial);
            
            Paint += new PaintEventHandler(paintControl);
            MouseDown += new MouseEventHandler(mousePressed);
            MouseUp += new MouseEventHandler(mouseReleased);
            MouseMove += new MouseEventHandler(mouseMoved);
            
        }

        public void SetInterpolationMethod(SplineInterpolationMethod method)
        {
            switch (method)
            {
                case SplineInterpolationMethod.Polynomial:
                    creationFun = Interpolation.CreatePolynomial;
                    break;
                case SplineInterpolationMethod.Cubic:
                    creationFun = Interpolation.CreateNaturalCubicSpline;
                    break;
                case SplineInterpolationMethod.Linear:
                    creationFun = Interpolation.CreateLinearSpline;
                    break;
                    break;
            }

            createSpline();
        }

        private delegate IInterpolationMethod CreationDlg(IList<double> points, IList<double> values);
        CreationDlg creationFun = Interpolation.CreatePolynomial;

        void createSpline()
        {
            double[] values = new double[mPoints.Count + 2];
            values[0] = mLeft.Y;
            values[mPoints.Count + 1] = mRight.Y;
            double[] locations = new double[mPoints.Count + 2];
            locations[0] = mLeft.X;
            locations[mPoints.Count + 1] = mRight.X;

            for (int i = 0; i < mPoints.Count; ++i)
            {
                values[i + 1] = mPoints[i].Y;
                locations[i + 1] = mPoints[i].X;
            }

            mSplineInterpolation = creationFun(locations, values);
            Invalidate();
            if (SplineChanged != null)
                SplineChanged();
        }

        void mouseMoved(object sender, MouseEventArgs e)
        {
            if (IsLeftDown)
            {
                if (mSelectedPoint >= 0)
                {
                    var pt = mPoints[mSelectedPoint];
                    int x = Math.Min(Math.Max(1, e.X), Width - 1);
                    int y = Math.Min(Math.Max(1, e.Y), Height - 1);

                    pt.X = x / (float)Width;
                    pt.Y = (Height - y) / (float)Height;
                    foreach (var poit in mPoints)
                        if (poit.X == pt.X)
                            return;

                    if (pt.X == 0 || pt.X == 1)
                        return;

                    mPoints[mSelectedPoint] = pt;
                    createSpline();
                }
                if (mSelectedPoint == -2)
                {
                    int y = Math.Min(Math.Max(1, e.Y), Height - 1);
                    mLeft.Y = (Height - y) / (float)Height;
                    createSpline();
                }
                if (mSelectedPoint == -3)
                {
                    int y = Math.Min(Math.Max(1, e.Y), Height - 1);
                    mRight.Y = (Height - y) / (float)Height;
                    createSpline();
                }
            }
        }

        void mouseReleased(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                IsLeftDown = false;
                mSelectedPoint = -1;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                foreach (var pt in mPoints)
                {
                    int x = (int)(pt.X * Width);
                    int y = (int)(Height - Height * pt.Y);
                    if (x >= e.X - 3 && x <= e.X + 4 && y >= e.Y - 3 && y <= e.Y + 4)
                    {
                        mPoints.Remove(pt);
                        createSpline();
                        return;
                    }
                }
            }
        }

        void mousePressed(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                IsLeftDown = true;
                mSelectedPoint = -1;
                if (e.X >= Width - 4)
                {
                    mSelectedPoint = -3;
                    return;
                }
                if (e.X >= 0 && e.X < 4)
                {
                    mSelectedPoint = -2;
                    return;
                }
                int i = 0;
                foreach (var pt in mPoints)
                {
                    int x = (int)(pt.X * Width);
                    int y = (int)(Height - Height * pt.Y);
                    if (x >= e.X - 3 && x <= e.X + 4  && y >= e.Y - 3 && y <= e.Y + 4)
                    {
                        mSelectedPoint = i;
                        break;
                    }
                    ++i;
                }

                if (mSelectedPoint == -1)
                    addNewPoint(e);
            }
        }

        void addNewPoint(MouseEventArgs e)
        {
            PointF pt = new PointF();
            int x = Math.Min(Math.Max(1, e.X), Width - 1);
            int y = Math.Min(Math.Max(1, e.Y), Height - 1);
            pt.X = x / (float)Width;
            pt.Y = (Height - y) / (float)Height;
            mPoints.Add(pt);
            mPoints.Sort((PointF p1, PointF p2) =>
                {
                    if (p1.X < p2.X)
                        return -1;
                    if (p1.X == p2.X)
                        return 0;
                    if (p1.X > p2.X)
                        return 1;

                    throw new InvalidOperationException();
                }
            );

            mSelectedPoint = mPoints.IndexOf(pt);

            createSpline();
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

            foreach (var pt in mPoints)
            {
                drawPoint(e.Graphics, pt);
            }

            var ptL = new PointF(3 / Width, mLeft.Y);
            var ptR = new PointF(((float)Width - 3) / Width, mRight.Y);
            drawPoint(e.Graphics, ptL);
            drawPoint(e.Graphics, ptR);
        }

        void drawPoint(Graphics g, PointF pt)
        {
            var rcPosX = (int)(pt.X * Width);
            var rcPosY = Height - (int)(pt.Y * Height);
            g.FillRectangle(mDrawBrush, new Rectangle(rcPosX - 3, rcPosY - 3, 7, 7));
        }

        private IInterpolationMethod mSplineInterpolation = null;
        private bool IsLeftDown = false;
        private Pen mDrawPen = new Pen(Color.Red, 3.0f);
        private Brush mDrawBrush = new SolidBrush(Color.Orange);
        private List<PointF> mPoints = new List<PointF>();
        private int mSelectedPoint = -1;
        private PointF mLeft = new PointF(0, 1);
        private PointF mRight = new PointF(1, 0);

        public IInterpolationMethod Spline { get { return mSplineInterpolation; } }
        public event Action SplineChanged;
    }
}
