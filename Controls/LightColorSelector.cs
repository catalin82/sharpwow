using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpWoW.Controls
{
    public partial class LightColorSelector : UserControl
    {
        public LightColorSelector()
        {
            Load += LightColorSelector_Load;
            InitializeComponent();
            image1 = new Bitmap(576, 1);
            image2 = new Bitmap(576, 1);
            image3 = new Bitmap(576, 1);
            image4 = new Bitmap(576, 1);
            image5 = new Bitmap(576, 1);
            panel1.BackgroundImageLayout = panel2.BackgroundImageLayout = panel3.BackgroundImageLayout = panel4.BackgroundImageLayout = panel5.BackgroundImageLayout = ImageLayout.Stretch;
            panel2.MouseClick += panel1_MouseClick;
            panel3.MouseClick += panel1_MouseClick;
            panel4.MouseClick += panel1_MouseClick;
            panel5.MouseClick += panel1_MouseClick;
            RecalcColors();
            panel1.BackgroundImage = image1;
            panel2.BackgroundImage = image2;
            panel3.BackgroundImage = image3;
            panel4.BackgroundImage = image4;
            panel5.BackgroundImage = image5;
            Utils.Reflection.CallMethod(panel1, "SetStyle", ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            Utils.Reflection.CallMethod(panel2, "SetStyle", ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            Utils.Reflection.CallMethod(panel3, "SetStyle", ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            Utils.Reflection.CallMethod(panel4, "SetStyle", ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            Utils.Reflection.CallMethod(panel5, "SetStyle", ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            pos0 = new Light.LightMarker(this, 0, panel1.Location, Color.Black);
            pos2880 = new Light.LightMarker(this, 2880, new Point(panel5.Location.X + panel5.Width, panel5.Location.Y), Color.White);
            pos0.Fixed = true;
            pos2880.Fixed = true;
            pos0.ValueChanged += new Action<Light.LightMarker>(MarkerValueChanged);
            pos2880.ValueChanged += MarkerValueChanged;
            panel1.SendToBack();
            panel5.SendToBack();
        }

        private void LightColorSelector_Load(object sender, EventArgs e)
        {
            TimeSpan minStep = TimeSpan.FromMinutes(0.5f);
            TimeSpan maxStep = TimeSpan.FromMinutes(576.0f / 2.0f);
            TimeSpan t = TimeSpan.FromMinutes(575.0f / 2.0f);
            ftext(label2, t);
            ftext(label3, t + minStep);
            t += maxStep;
            ftext(label4, t);
            ftext(label5, t + minStep);
            t += maxStep;
            ftext(label6, t);
            ftext(label7, t + minStep);
            t += maxStep;
            ftext(label8, t);
            ftext(label9, t + minStep);
        }

        void ftext(Label l, TimeSpan s)
        {
            l.Text = s.Hours.ToString("D2") + ":" + s.Minutes.ToString("D2");
        }

        private void colorChanged()
        {
            Invalidate();
        }

        private Light.LightInterpolator mInterpolator = new Light.LightInterpolator();

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var bits = image1.LockBits(new Rectangle(0, 0, 576, 1), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            int[] colors = new int[576];
            for (int i = 0; i < 576; ++i)
            {
                var clr = mInterpolator.GetColorForTime(i);
                var dc = Color.FromArgb((int)(clr.X * 255), (int)(clr.Y * 255), (int)(clr.Z * 255));
                colors[i] = dc.ToArgb();
            }
            Utils.Memory.CopyMemory(colors, bits.Scan0);
            image1.UnlockBits(bits);
            panel1.Invalidate();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            var bits = image2.LockBits(new Rectangle(0, 0, 576, 1), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            int[] colors = new int[576];
            for (int i = 0; i < 576; ++i)
            {
                var clr = mInterpolator.GetColorForTime(i + 576);
                var dc = Color.FromArgb((int)(clr.X * 255), (int)(clr.Y * 255), (int)(clr.Z * 255));
                colors[i] = dc.ToArgb();
            }
            Utils.Memory.CopyMemory(colors, bits.Scan0);
            image2.UnlockBits(bits);
            panel2.Invalidate();
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            var bits = image3.LockBits(new Rectangle(0, 0, 576, 1), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            int[] colors = new int[576];
            for (int i = 0; i < 576; ++i)
            {
                var clr = mInterpolator.GetColorForTime(i + 576 * 2);
                var dc = Color.FromArgb((int)(clr.X * 255), (int)(clr.Y * 255), (int)(clr.Z * 255));
                colors[i] = dc.ToArgb();
            }
            Utils.Memory.CopyMemory(colors, bits.Scan0);
            image3.UnlockBits(bits);
            panel3.Invalidate();
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            var bits = image4.LockBits(new Rectangle(0, 0, 576, 1), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            int[] colors = new int[576];
            for (int i = 0; i < 576; ++i)
            {
                var clr = mInterpolator.GetColorForTime(i + 576 * 3);
                var dc = Color.FromArgb((int)(clr.X * 255), (int)(clr.Y * 255), (int)(clr.Z * 255));
                colors[i] = dc.ToArgb();
            }
            Utils.Memory.CopyMemory(colors, bits.Scan0);
            image4.UnlockBits(bits);
            panel4.Invalidate();
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {
            var bits = image5.LockBits(new Rectangle(0, 0, 576, 1), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            int[] colors = new int[576];
            for (int i = 0; i < 576; ++i)
            {
                var clr = mInterpolator.GetColorForTime(i + 576 * 4);
                var dc = Color.FromArgb((int)(clr.X * 255), (int)(clr.Y * 255), (int)(clr.Z * 255));
                colors[i] = dc.ToArgb();
            }
            Utils.Memory.CopyMemory(colors, bits.Scan0);
            image5.UnlockBits(bits);
            panel5.Invalidate();
        }

        public void InitFromWorldLight(List<uint> times, List<SlimDX.Vector3> colors)
        {
            mInterpolator.InitFromTable(times, colors);
            if (pos0 != null)
                pos0.RemoveMarker();
            if (pos2880 != null)
                pos2880.RemoveMarker();
            pos0 = pos2880 = null;

            for (int i = 0; i < times.Count; ++i)
            {
                var time = times[i];
                uint panel = time / 576;
                if (panel == 5)
                    panel = 4;

                var offset = time - panel * 576;
                var ctrl = Controls["panel" + (panel + 1)];
                var clr = colors[i];
                Color color = Color.FromArgb((int)(clr.X * 255), (int)(clr.Y * 255), (int)(clr.Z * 255));
                Light.LightMarker marker = new Light.LightMarker(this, times[i], new Point((int)offset + ctrl.Location.X, ctrl.Location.Y), color);
                marker.Fixed = false;
                marker.MinTime = panel * 576;
                marker.MaxTime = (panel + 1) * 576;
                marker.RemoveRequested += new Action<Light.LightMarker>(MarkerRemoved);
                marker.ValueChanged += new Action<Light.LightMarker>(MarkerValueChanged);
                marker.TimeChanged += new Action<Light.LightMarker, uint, uint>(MarkerTimeChanged);
            }

            RecalcColors();
        }

        public void RecalcColors()
        {
            this.SuspendLayout();
            panel1_Paint(null, null);
            panel2_Paint(null, null);
            panel3_Paint(null, null);
            panel4_Paint(null, null);
            panel5_Paint(null, null);
            this.ResumeLayout(true);
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            Panel pnl = sender as Panel;
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() != DialogResult.OK)
                return;

            mInterpolator.AddColor(((uint)e.X) + uint.Parse(pnl.Tag as string) * 576, cd.Color);
            RecalcColors();

            Light.LightMarker marker = new Light.LightMarker(this, (uint)e.X + uint.Parse(pnl.Tag as string) * 576, new Point(e.X + pnl.Location.X, pnl.Location.Y), cd.Color);
            marker.MinTime = uint.Parse(pnl.Tag as string) * 576;
            marker.MaxTime = uint.Parse(pnl.Tag as string) * 576 + 576;
            marker.RemoveRequested += new Action<Light.LightMarker>(MarkerRemoved);
            marker.ValueChanged += new Action<Light.LightMarker>(MarkerValueChanged);
            marker.TimeChanged += new Action<Light.LightMarker, uint, uint>(MarkerTimeChanged);
        }

        void MarkerTimeChanged(Light.LightMarker arg1, uint arg2, uint arg3)
        {
            mInterpolator.ChangeTime(arg2, arg3);
            RecalcColors();
        }

        void MarkerValueChanged(Light.LightMarker marker)
        {
            mInterpolator.ChangeColorForTime(marker.Time, marker.Color);
            RecalcColors();
        }

        void MarkerRemoved(Light.LightMarker marker)
        {
            mInterpolator.RemoveTime(marker.Time);
            marker.RemoveMarker();
            RecalcColors();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            Panel pnl = sender as Panel;
            TimeSpan span = TimeSpan.FromMinutes((e.X + uint.Parse(pnl.Tag as string) * 576) / 2.0f);
            ftext(label11, span);
            label11.Location = new Point(pnl.Location.X + e.X, pnl.Location.Y - 15);
            label11.Visible = true;
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            label11.Visible = false;
        }

        private Bitmap image1, image2, image3, image4, image5;
        Light.LightMarker pos0, pos2880;

        public Light.LightInterpolator Interpolator { get { return mInterpolator; } }
    }
}
