using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkWoW.database.wowDB
{
    /*
     * MangosCore implementation, uses Trinity Implementation for the most methods
    */ 
    class Mangos : Trinity
    {
        public Mangos()
        {
        }

        public override CreatureTemplate CreateCreatureTemplate(object[] data)
        {
            long entry = long.Parse(data[0].ToString());
            string name = data[10].ToString();
            string subname = data[11].ToString();
            int minlevel = int.Parse(data[14].ToString());
            int maxlevel = int.Parse(data[15].ToString());
            float mindmg = float.Parse(data[24].ToString());
            float maxdmg = float.Parse(data[25].ToString());
            int[] modelids = new int[4];
            int health = int.Parse(data[17].ToString());
            int mana = int.Parse(data[18].ToString());
            for (int i = 0; i < modelids.Length; i++)
            {
                modelids[i] = int.Parse(data[6 + i].ToString());
            }
            CreatureTemplate ct = new CreatureTemplate(entry, name, subname, minlevel, maxlevel, mindmg, maxdmg, health, mana, modelids);
            return ct;
        }
    }
}
