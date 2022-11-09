using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib
{
    public interface IDrawable
    {
        IDrawable RotatedBy(double phi, Coordinate pivot);
        IDrawable Reversed();
        IDrawable ReflectY();
        Coordinate Start { get; }
        Coordinate End { get; }
        Rectangle Bounds { get; }
    }
}
