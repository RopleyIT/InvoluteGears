using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib
{
    public class PolyLine : IDrawable
    {
        public IList<Coordinate> Vertices { get; set; } = new List<Coordinate>();

        public Coordinate Start => Vertices.First();

        public Coordinate End => Vertices.Last();

        public Rectangle Bounds
        {
            get
            {
                BoundsTracker b = new BoundsTracker();
                b.Track(Vertices);
                return b.Bounds;
            }
        }

        public void Append(PolyLine p) 
            => p.Vertices.AddRange(p.Vertices);

        public IDrawable RotatedBy(double phi, Coordinate pivot)
            => new PolyLine
            {
                Vertices = this.Vertices
                    .Select(p => p.RotateAbout(pivot, phi))
                    .ToList()
            };

        public IDrawable ReflectY()
            => new PolyLine
            {
                Vertices = this.Vertices
                    .Select(p => p.Conjugate)
                    .ToList()
            };

        public IDrawable Reversed()
            => new PolyLine
            {
                Vertices = this.Vertices.Reverse().ToList()
            };

        public IDrawable Translated(Coordinate offset)
            => new PolyLine
            { 
                Vertices = this.Vertices 
                    .Select(P => P.Offset(offset))
                    .ToList()
            };
    }
}
