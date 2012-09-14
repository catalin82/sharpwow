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
    public partial class LightEditorTab : UserControl
    {
        public LightEditorTab()
        {
            InitializeComponent();
            Game.GameManager.PropertyChanged += new Action<Game.GameProperties>(GamePropertyChanged);
            listBox1.Sorted = false;
            listBox1.DoubleClick += new EventHandler(lightListClick);
        }

        void lightListClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            World.VisualLightEntry vse = listBox1.SelectedItem as World.VisualLightEntry;
            UI.Dialogs.LightCreator lc = UI.Dialogs.LightCreator.InitFromExistingLight(vse.Entry);
            lc.ShowDialog();
        }

        void GamePropertyChanged(Game.GameProperties prop)
        {
            switch (prop)
            {
                case Game.GameProperties.Map:
                    updateMap();
                    break;

                case Game.GameProperties.CameraPosition:
                    updatePosition();
                    break;
            }
        }

        private Bitmap InitialImage = null;

        void updatePosition()
        {
            if (InitialImage == null)
                return;

            uint mapSize = 64 * 17;
            float realSize = 64 * Utils.Metrics.Tilesize;
            float step = realSize / mapSize;

            var pos = Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position;
            float posX = pos.X + Utils.Metrics.MidPoint;
            float posY = pos.Y + Utils.Metrics.MidPoint;
            posX /= step;
            posY /= step;
            minimapControl1.Minimap.Dispose();
            var newImg = InitialImage.Clone() as Bitmap;
            Graphics g = Graphics.FromImage(newImg);
            g.FillRectangle(new SolidBrush(Color.Black), new RectangleF(posX - 6, posY - 6, 12, 12));
            minimapControl1.Minimap = newImg;

            Utils.Reflection.CallMethod(listBox1, "RefreshItems");
        }

        void updateMap()
        {
            button1.Enabled = true;
            uint newId = Game.GameManager.WorldManager.MapID;
            ADT.Minimap minimap = new ADT.Minimap(DBC.DBCStores.Map[newId].InternalName, newId);
            InitialImage = minimap.CreateImage().Clone() as Bitmap;
            Graphics g = Graphics.FromImage(InitialImage);
            insertLights(g);
            minimapControl1.Minimap = InitialImage.Clone() as Bitmap;
            if (Game.GameManager.IsPandaria == false)
                listBox1.DataSource = Game.GameManager.SkyManager.GetSkyForMap(newId).VisualLightEntries;
        }

        void insertLights(Graphics g)
        {
            return;

            uint mapSize = 64 * 17;
            float realSize = 64 * Utils.Metrics.Tilesize;
            float step = realSize / mapSize;
            Pen innerPen = new Pen(Color.Orange, 4);
            Pen outerPen = new Pen(Color.Red, 4);

            foreach (var dbl in DBC.DBCStores.Light.Records)
            {
                if (dbl.MapID == Game.GameManager.WorldManager.MapID)
                {
                    float x = dbl.x / 36;
                    float y = dbl.z / 36;
                    x /= step;
                    y /= step;
                    float iRadius = dbl.falloff / 36;
                    iRadius /= step;
                    float oRadius = dbl.falloffEnd / 36;
                    oRadius /= step;

                    g.DrawEllipse(innerPen, x - iRadius, y - iRadius, 2 * iRadius, 2 * iRadius);
                    g.DrawEllipse(outerPen, x - oRadius, y - oRadius, 2 * oRadius, 2 * oRadius);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UI.Dialogs.LightCreator lc = new UI.Dialogs.LightCreator();
            lc.ShowDialog();
        }
    }
}
