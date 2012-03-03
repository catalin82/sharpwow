using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkWoW.database.wowDB
{
    class CreatureSpawn
    {
        public long guid;
        public long id;
        public int map;
        public int modelid;
        public float pos_x;
        public float pos_y;
        public float pos_z;
        public float orientation;
        public int curHealth;
        public int curMana;

        public CreatureSpawn(long guid, long id, int map, int modelid, float pos_x, float pos_y, float pos_z, float orientation, int curHealth, int curMana)
        {
            this.guid = guid;
            this.id = id;
            this.map = map;
            this.modelid = modelid;
            this.pos_x = pos_x;
            this.pos_y = pos_y;
            this.pos_z = pos_z;
            this.orientation = orientation;
            this.curHealth = curHealth;
            this.curMana = curMana;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("guid=").Append(guid).Append("\n");
            sb.Append("id=").Append(id).Append("\n");
            sb.Append("map=").Append(map).Append("\n");
            sb.Append("modelid=").Append(modelid).Append("\n");
            sb.Append("pos_x=").Append(pos_x).Append("\n");
            sb.Append("pos_y=").Append(pos_y).Append("\n");
            sb.Append("pos_z=").Append(pos_z).Append("\n");
            sb.Append("orientation=").Append(orientation).Append("\n");
            sb.Append("curHealth=").Append(curHealth).Append("\n");
            sb.Append("curMana=").Append(curMana).Append("\n");
            return sb.ToString();
        }
    }
}
