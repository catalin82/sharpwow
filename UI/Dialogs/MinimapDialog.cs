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
    public partial class MinimapDialog : Form
    {
        public MinimapDialog()
        {
            InitializeComponent();
        }

        public void SetMap(DBC.MapEntry map)
        {
            mEntry = map;
            try
            {
                ADT.Minimap minimap = new ADT.Minimap(map.InternalName, map.ID);
                minimapControl1.Minimap = minimap.CreateImage();
                minimapControl1.MapEntry = mEntry;
                ContinentName = map.InternalName;
                minimapControl1.PointSelected += new SharpWoW.Controls.MinimapControl.PointSelectedDlg(_PointSelected);
                Text = "Select your entry point on " + map.Name;
            }
            catch (Exception)
            {
                Close();
            }
        }

        void _PointSelected(float x, float y)
        {
            Close();
            if (PointSelected != null)
                PointSelected(mEntry.ID, ContinentName, x, y);
        }

        public string ContinentName { get; set; }
        public delegate void MinimapSelectedDlg(uint mapid, string continent, float x, float y);
        public event MinimapSelectedDlg PointSelected;

        private DBC.MapEntry mEntry;
    }
}
