using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT.Cataclysm
{
    class BasicInfoProvider : IBasicInfoQuery
    {
        public BasicInfoProvider()
        {
        }

        public override T queryValue<T>(InfoQuery property, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
