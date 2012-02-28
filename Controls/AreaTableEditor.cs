using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace SharpWoW.Controls
{
    public partial class AreaTableEditor : UserControl
    {
        public AreaTableEditor()
        {
            InitializeComponent();
        }

        private void AreaTableEditor_Load(object sender, EventArgs e)
        {
            if (DesignMode == false)
            {
                foreach (var rec in DBC.DBCStores.AreaTable.Records)
                {
                   
                    listBox1.Items.Add(new ListBoxItem(rec));
                }
            }
        }

        private class ListBoxItem
        {
            public DBC.AreaTableEntry Entry { get; set; }
            public ListBoxItem(DBC.AreaTableEntry entry)
            {
                Entry = entry;
            }

            public override string ToString()
            {
                return Entry.ID.ToString() + " - " + Entry.AreaName;
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                ListBoxItem i = listBox1.SelectedItem as ListBoxItem;
                if (i == null)
                    return;

                var rec = i.Entry;
                numericUpDown1.Value = rec.ID;
                numericUpDown2.Value = rec.mapid;
                numericUpDown3.Value = rec.parentId;
                if (numericUpDown3.Value != 0)
                    toolTip1.SetToolTip(numericUpDown3, DBC.DBCStores.AreaTable[rec.parentId].AreaName);
                else
                    toolTip1.SetToolTip(numericUpDown3, "No parent area");
                numericUpDown4.Value = rec.exploreFlag;
                textBox1.Text = rec.AreaName;
            }
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (numericUpDown3.Value != 0)
                    toolTip1.SetToolTip(numericUpDown3, DBC.DBCStores.AreaTable[(uint)numericUpDown3.Value].AreaName);
                else
                    toolTip1.SetToolTip(numericUpDown3, "No parent area");
            }
            catch (Exception)
            {
                toolTip1.SetToolTip(numericUpDown3, "No parent area");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var id = (uint)numericUpDown1.Value;
            try
            {
                var entry = DBC.DBCStores.AreaTable[id];
                entry.AreaName = textBox1.Text;
                entry.exploreFlag = (uint)numericUpDown4.Value;
                entry.parentId = (uint)numericUpDown3.Value;
                entry.mapid = (uint)numericUpDown2.Value;
                for (int i = 0; i < listBox1.Items.Count; ++i)
                {
                    var item = listBox1.Items[i] as ListBoxItem;
                    if (item.Entry.ID == id)
                    {
                        MethodInfo method = typeof(ListBox).GetMethod("RefreshItem", BindingFlags.NonPublic | BindingFlags.Instance);
                        method.Invoke(listBox1, new object[] { i });
                    }
                }
                
            }
            catch (Exception)
            {
                DBC.AreaTableEntry ate = new DBC.AreaTableEntry()
                {
                    ambientMultiplier = 1.0f,
                    area_level = 0,
                    AreaName = textBox1.Text,
                    exploreFlag = (uint)numericUpDown4.Value,
                    flags = (uint)0,
                    ID = (uint)numericUpDown1.Value,
                    mapid = (uint)numericUpDown2.Value,
                    parentId = (uint)numericUpDown3.Value,
                    liquidType1 = 0,
                    liquidType2 = 0,
                    liquidType3 = 0,
                    liquidType4 = 0,
                    lightid = 0,
                    minElevation = 0.0f,
                    refFactionGroup = 0,
                    refSoundAmbi = 0,
                    refSoundPref = 0,
                    refSoundPrefUWater = 0,
                    refZoneIntro = 0,
                    refZoneMusic = 0
                };

                DBC.DBCStores.AreaTable.AddEntry((uint)numericUpDown1.Value, ate);
                listBox1.Items.Add(new ListBoxItem(ate));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DBC.DBCStores.AreaTable.SaveDBC();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var chunk = Game.GameManager.WorldManager.HoveredChunk;
            if (chunk == null)
            {
                MessageBox.Show("Currently no chunk is below the point of view!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DBC.AreaTableEntry rec = null;

            try
            {
                rec = DBC.DBCStores.AreaTable[chunk.Header.areaId];
            }
            catch (Exception)
            {
                MessageBox.Show("The area id of the chunk below the point of view is not contained in the DBC!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            numericUpDown1.Value = rec.ID;
            numericUpDown2.Value = rec.mapid;
            numericUpDown3.Value = rec.parentId;
            if (numericUpDown3.Value != 0)
                toolTip1.SetToolTip(numericUpDown3, DBC.DBCStores.AreaTable[rec.parentId].AreaName);
            else
                toolTip1.SetToolTip(numericUpDown3, "No parent area");
            numericUpDown4.Value = rec.exploreFlag;
            textBox1.Text = rec.AreaName;
        }
    }
}
