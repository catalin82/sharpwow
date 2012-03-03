using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkWoW.database.wowDB
{
    /*
     * ArcEmu implementation
    */ 
    class ArcEmu : Core
    {
        public static string CreatureSelect = "SELECT * FROM `creature_names` AS cn JOIN `creature_proto` AS cp ON (cn.entry = cp.entry)";
        public static string CreatureSpawnSelect = "SELECT * FROM `creature_spawns`";

        public CreatureTemplate CreateCreatureTemplate(object[] data)
        {
            long entry = long.Parse(data[0].ToString());
            string name = data[1].ToString();
            string subname = data[2].ToString();
            int minlevel = int.Parse(data[25].ToString());
            int maxlevel = int.Parse(data[26].ToString());
            float mindmg = float.Parse(data[35].ToString());
            float maxdmg = float.Parse(data[36].ToString());
            int health = int.Parse(data[29].ToString());
            int mana = int.Parse(data[30].ToString());
            int[] modelids = new int[4];
            for (int i = 0; i < modelids.Length; i++)
            {
                modelids[i] = int.Parse(data[10+i].ToString());
            }
            CreatureTemplate ct = new CreatureTemplate(entry, name, subname, minlevel, maxlevel, mindmg, maxdmg, health, mana, modelids);
            return ct;
        }

        public string GetCreatureSelect()
        {
            return CreatureSelect;
        }

        public string GetCreatureSelect(int entry)
        {
            return CreatureSelect + " AND cn.entry = '" + entry + "';";
        }

        public CreatureSpawn CreateCreatureSpawn(object[] data)
        {
            long guid = long.Parse(data[0].ToString());
            long id = long.Parse(data[1].ToString());
            int map = int.Parse(data[2].ToString());
            int modelid = int.Parse(data[8].ToString());
            float pos_x = float.Parse(data[3].ToString());
            float pos_y = float.Parse(data[4].ToString());
            float pos_z = float.Parse(data[5].ToString());
            float orientation = float.Parse(data[6].ToString());
            CreatureSpawn cs = new CreatureSpawn(guid, id, map, modelid, pos_x, pos_y, pos_z, orientation, 0, 0);
            return cs;
        }

        public string GetCreatureSpawnSelect()
        {
            return CreatureSpawnSelect;
        }

        public string GetCreatureSpawnSelect(int mapid)
        {
            return CreatureSpawnSelect + " WHERE `map` = '" + mapid + "';";
        }

        public string GetCreatureSpawnInsertQuery(CreatureSpawn cs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO `creature_spawns` (`id`, `entry`, `map`, `displayid`, `position_x`, `position_y`, `position_z`, `orientation`) VALUES (");
            string[] placeholder = GetSQLSpawnPlaceholder();
            for (int i = 0; i < placeholder.Length; i++)
            {
                sb.Append(placeholder[i]);
                if (i + 1 < placeholder.Length)
                {
                    sb.Append(",");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }

        public string GetCreatureSpawnDeleteQuery()
        {
            return "DELETE FROM `creature_spawns` WHERE `id` = ?guid;";
        }

        public string[] GetSQLSpawnPlaceholder()
        {
            return new string[] { "?guid", "?id", "?map", "?modelid", "?pos_x", "?pos_y", "?pos_z", "?orientation" };
        }

        public object[] GetSQLSpawnValues(CreatureSpawn cs)
        {
            return new object[] { cs.guid, cs.id, cs.map, cs.modelid, cs.pos_x, cs.pos_y, cs.pos_z, cs.orientation };
        }

    }
}
