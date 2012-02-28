using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectInput;

namespace SharpWoW.Video.Input
{
    public class Mouse
    {
        public Mouse(DirectInput input, IntPtr wndHandle)
        {
            mMouse = new SlimDX.DirectInput.Mouse(input);
            mMouse.SetCooperativeLevel(wndHandle, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);
            mMouse.Acquire();
            State = mMouse.GetCurrentState();
        }

        public void Update()
        {
            State = mMouse.GetCurrentState();
        }

        private SlimDX.DirectInput.Mouse mMouse = null;
        public MouseState State { get; private set; }
    }
}
