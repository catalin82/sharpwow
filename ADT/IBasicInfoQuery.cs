using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT
{
    public enum InfoQuery
    {
        ChunkAreaId,
    }

    public abstract class IBasicInfoQuery
    {
        public abstract T queryValue<T>(InfoQuery property, params object[] args);

        public static IBasicInfoQuery Create()
        {
            if (Game.GameManager.BuildNumber <= 12340)
                return new Wotlk.BasicInfoProvider();

            return new Cataclysm.BasicInfoProvider();
        }
    }
}
