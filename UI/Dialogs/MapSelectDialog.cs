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
    public partial class MapSelectDialog : Form
    {
        public MapSelectDialog()
        {
            InitializeComponent();
            SelectedMap = null;
        }

        private void MapSelectDialog_Load(object sender, EventArgs e)
        {
            foreach (var entry in DBC.DBCStores.Map.Records)
            {
                if (entry.InternalName.IndexOf("Transport") >= 0)
                    continue;

                listBox1.Items.Add(new MapSelectionItem(entry));
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            SelectedMap = listBox1.SelectedItem as MapSelectionItem;
            Close();
        }

        public MapSelectionItem SelectedMap { get; private set; }
    }

    public class MapSelectionItem
    {
        public MapSelectionItem(DBC.MapEntry e)
        {
            Map = e;
        }

        public DBC.MapEntry Map { get; set; }

        public override string ToString()
        {
            return Map.Name;
        }
    }
}
