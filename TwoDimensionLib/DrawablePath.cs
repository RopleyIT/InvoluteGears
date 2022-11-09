using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib
{
    public class DrawablePath
    {
        public IList<IDrawable> Curves { get; set; }
        public bool Closed { get; set; }

        public Rectangle Bounds
        {
            get
            {
                BoundsTracker b = new BoundsTracker();
                foreach (IDrawable d in Curves)
                    b.Track(d.Bounds);
                return b.Bounds;
            }
        }
    }
}
