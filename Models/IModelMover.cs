using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models
{
    public interface IModelMover
    {
        void rotateModel(SlimDX.Vector3 axis, float amount);
        void moveModel(SlimDX.Vector3 axis, float amount);
        void moveModel(SlimDX.Vector3 newPos);

        event Action<SlimDX.Matrix> ModelChanged;
    }
}
