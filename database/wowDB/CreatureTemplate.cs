using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkWoW.database.wowDB
{
    //needed data
    public class CreatureTemplate
    {
        public long entry;
        public string name;
        public string subname;
        public int minlevel;
        public int maxlevel;
        public float mindmg;
        public float maxdmg;
        public int health;
        public int mana;
        //maybe randomly pick a id
        public int[] modelids;

        public CreatureTemplate(long entry, string name, string subname, int minlevel, int maxlevel, float mindmg, float maxdmg, int health, int mana, int[] modelids)
        {
            this.entry = entry;
            this.name = name;
            this.subname = subname;
            this.minlevel = minlevel;
            this.maxlevel = maxlevel;
            this.mindmg = mindmg;
            this.maxdmg = maxdmg;
            this.health = health;
            this.mana = mana;
            this.modelids = modelids;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("entry=").Append(entry).Append("\n");
            sb.Append("name=").Append(name).Append("\n");
            sb.Append("subname=").Append(subname).Append("\n");
            sb.Append("minlevel=").Append(minlevel).Append("\n");
            sb.Append("maxlevel=").Append(maxlevel).Append("\n");
            sb.Append("mindmg=").Append(mindmg).Append("\n");
            sb.Append("maxdmg=").Append(maxdmg).Append("\n");
            sb.Append("health=").Append(health).Append("\n");
            sb.Append("mana=").Append(mana).Append("\n");
            for (int i = 0; i < modelids.Length; i++ )
            {
                int modelid = modelids[i];
                sb.Append("modelid").Append(i).Append("=").Append(modelid).Append("\n");
            }
            return sb.ToString();
        }
    }
}
