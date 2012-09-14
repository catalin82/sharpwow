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
                minimapControl1.StaticOverlay = createWorldmapOverlay();
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

        Bitmap createWorldmapOverlay()
        {
            int sizeX = 1002;
            int sizeY = 668;

            Bitmap bmp = new Bitmap(sizeX, sizeY);
            Graphics g = Graphics.FromImage(bmp);
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    int index = i * 4 + j + 1;
                    var tex = Video.TextureManager.GetTexture(@"Interface\Worldmap\Kalimdor\Kalimdor" + index + ".blp");
                    Bitmap subBmp = new Bitmap(256, 256);
                    Video.TextureConverter.SaveTextureToImage(tex.Native, subBmp);
                    g.DrawImage(subBmp, new Point(j * 256, i * 256));
                }
            }

            g.Flush();
            bmp.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\WM.png", System.Drawing.Imaging.ImageFormat.Png);
            return bmp;
        }

        public string ContinentName { get; set; }
        public delegate void MinimapSelectedDlg(uint mapid, string continent, float x, float y);
        public event MinimapSelectedDlg PointSelected;

        private DBC.MapEntry mEntry;
    }
}
