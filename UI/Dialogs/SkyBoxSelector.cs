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
    public partial class SkyBoxSelector : Form
    {
        public SkyBoxSelector()
        {
            InitializeComponent();
            SelectedItem = null;
        }

        private void SkyBoxSelector_Load(object sender, EventArgs e)
        {
            foreach (var skyb in DBC.DBCStores.LightSkyBox.Records)
                listBox1.Items.Add(new SkyboxListBoxItem() { SkyBox = skyb });
        }

        public DBC.LightSkyBox SelectedItem { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectedItem = new DBC.LightSkyBox() { Path = "(none)", ID = 0 };
            Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                SelectedItem = (listBox1.SelectedItem as SkyboxListBoxItem).SkyBox;
                Close();
            }
        }
    }

    public class SkyboxListBoxItem
    {
        public DBC.LightSkyBox SkyBox { get; set; }

        public override string ToString()
        {
            return SkyBox.Path;
        }
    }
}
