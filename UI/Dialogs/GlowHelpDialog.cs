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
    public partial class GlowHelpDialog : Form
    {
        public GlowHelpDialog()
        {
            InitializeComponent();
        }

        private void GlowHelpDialog_Load(object sender, EventArgs e)
        {
            pictureBox1.BackgroundImage = Resources.Images.Img_glowHigh;
            pictureBox2.BackgroundImage = Resources.Images.Img_glowLow;
        }
    }
}
