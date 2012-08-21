using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models.WMO
{
    public class WMOHitInformation
    {
        public string Name { get; set; }
        public ADT.Wotlk.MODF Placement { get; set; }
        public SlimDX.Vector3 HitPoint { get; set; }
        public SlimDX.Matrix ModelMatrix { get; set; }
        public WMOFile Model { get; set; }
    }
}
