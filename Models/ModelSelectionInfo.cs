using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models
{
    public class ModelSelectionInfo
    {
        public IModelMover ModelMover { get; set; }
        public string ModelName { get; set; }
        public SlimDX.Vector3 ModelPosition { get; set; }

        public void AlignToGround()
        {
            float h = 0;
            if (Game.GameManager.WorldManager.GetLandHeightFast(ModelPosition.X, ModelPosition.Y, ref h))
                ModelMover.moveModel(new SlimDX.Vector3(ModelPosition.X, ModelPosition.Y, h));
        }
    }
}
