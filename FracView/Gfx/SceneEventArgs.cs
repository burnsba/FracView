using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Gfx
{
    /// <summary>
    /// Container for event information.
    /// </summary>
    public class SceneEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the scene associated with the event.
        /// </summary>
        public IScene? Scene { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEventArgs"/> class.
        /// </summary>
        /// <param name="scene">Scene associated with the event.</param>
        public SceneEventArgs(IScene scene)
        {
            Scene = scene;
        }
    }
}
