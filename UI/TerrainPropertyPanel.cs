using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

namespace SharpWoW.UI
{
    public class TerrainPropertyPanel
    {
        public TerrainPropertyPanel(PropertyGrid grid)
        {
            mGrid = grid;
            Game.GameManager.PropertyChanged += new Action<Game.GameProperties>(OnPropertyChanged);
        }

        void OnPropertyChanged(Game.GameProperties prop)
        {
            mGrid.Refresh();
        }

        private PropertyGrid mGrid = null;

        [Category("Tile"), Description("The name of the file inside the archives of wow.")]
        public string TileName
        {
            get
            {
                var tile = Game.GameManager.WorldManager.HoveredTile;
                if (tile == null)
                    return "Not a tile!";

                return tile.FileName;
            }
        }

        [Category("Tile"), Description("The set of textures in the ADT that can be used by its chunks.")]
        public string[] Textures
        {
            get
            {
                var tile = Game.GameManager.WorldManager.HoveredTile;
                if (tile == null)
                    return new string[0];

                return tile.TextureNames.ToArray();
            }
        }
    }
}
