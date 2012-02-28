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
    public partial class LightMarkerDialog : Form
    {
        public LightMarkerDialog(Controls.Light.LightMarker marker)
        {
            InitializeComponent();
            panel1.BackColor = marker.Color;
            mMarker = marker;
            if (mMarker.Fixed)
                numericUpDown1.Enabled = false;
            else
            {
                numericUpDown1.Minimum = mMarker.MinTime;
                numericUpDown1.Maximum = mMarker.MaxTime;
                numericUpDown1.Value = marker.Time;
                numericUpDown1.ValueChanged += new EventHandler(ValueChanged);
            }
        }

        void ValueChanged(object sender, EventArgs e)
        {
            mMarker.ChangeTime((uint)numericUpDown1.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            panel1.BackColor = colorDialog1.Color;
            mMarker.ChangeColor(colorDialog1.Color);
        }

        private Controls.Light.LightMarker mMarker;
    }
}
