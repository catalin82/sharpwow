using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models
{
    public class SelectionManager
    {
        public void SelectMdxModel(MDX.MdxIntersectionResult result)
        {
            mMdxResult = result;
            mWmoResult = null;
            mSelectionBox.UpdateSelectionBox(result.Model.BoundingBox, result.InstanceData.ModelMatrix);
            
        }

        public void SelectWMOModel(WMO.WMOHitInformation wmoHit)
        {
            mMdxResult = null;
            mWmoResult = wmoHit;
        }

        public void ClearSelection()
        {
            mMdxResult = null;
            mWmoResult = null;
            mSelectionBox.ClearSelectionBox();
        }

        public bool IsMdxInstanceSelected(MDX.M2Info info, uint id)
        {
            if(mMdxResult == null)
                return false;

            return (mMdxResult.Model == info && mMdxResult.InstanceID == id);
        }

        public void renderSelection()
        {
            mSelectionBox.RenderBox();
        }

        MDX.MdxIntersectionResult mMdxResult = null;
        WMO.WMOHitInformation mWmoResult = null;
        SelectionBox mSelectionBox = new SelectionBox();
    }
}
