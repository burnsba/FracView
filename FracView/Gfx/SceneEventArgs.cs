using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Gfx
{
    public class SceneEventArgs : EventArgs
    {
        public IScene? Scene { get; set; }

        public SceneEventArgs(IScene scene)
        {
            Scene = scene;
        }
    }
}
