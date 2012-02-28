using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.DBC
{
    internal class MapConverter : DBC.IDBCRowConverter<MapEntry>
    {
        public MapEntry Convert(object value)
        {
            MapEntry_4 me = value as MapEntry_4;
            MapEntry ret = new MapEntry()
            {
                ID = me.ID,
                AreaTable = me.AreaTable,
                InternalName = me.InternalName,
                Name = me.MapName
            };

            return ret;
        }
    }

    internal class AreaTableConverter : DBC.IDBCRowConverter<AreaTableEntry>
    {
        public AreaTableEntry Convert(object value)
        {
            AreaTableEntry_4 ae = value as AreaTableEntry_4;
            AreaTableEntry atbl = new AreaTableEntry();
            foreach (var field in atbl.GetType().GetFields())
            {
                field.SetValue(atbl, ae.GetType().GetField(field.Name).GetValue(ae));
            }
            return atbl;
        }
    }

    internal class LightParamsConverter : DBC.IDBCRowConverter<LightParams>
    {
        public LightParams Convert(object value)
        {
            LightParams ret = new LightParams();
            foreach (var field in ret.GetType().GetFields())
            {
                field.SetValue(ret, value.GetType().GetField(field.Name).GetValue(value));
            }

            return ret;
        }
    }
}
