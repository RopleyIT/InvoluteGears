using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib
{
    public class CubicSpline : IDrawable
    {
        public Coordinate[] Points { get; set; } = new Coordinate[4];

        public Coordinate Start => Points[0];

        public Coordinate End => Points[3];

        public Rectangle Bounds
        {
            get
            {
                // An approximation, as the control points
                // could be well outside the true bounds.
                // Here we split the spline into two cubic
                // splines using de Casteljau's algorithm.
                // This increases the control points from
                // four to seven, but pulls those points
                // much closer to the curve, reducing the
                // area of the bounds to be much closer to
                // the actual rectangle containing the curve.

                BoundsTracker b = new BoundsTracker();
                Coordinate midLeft = Geometry.MidPoint(Points[0], Points[1]);
                Coordinate midRight = Geometry.MidPoint(Points[2], Points[3]);
                Coordinate midTop = Geometry.MidPoint(Points[1], Points[2]);
                Coordinate left = Geometry.MidPoint(midLeft, midTop);
                Coordinate right = Geometry.MidPoint(midRight, midTop);
                b.Track(Points[0]);
                b.Track(Points[3]);
                b.Track(midLeft);
                b.Track(midRight);
                b.Track(left);
                b.Track(right);
                return b.Bounds;
            }
        }

        public IDrawable Reversed()
            => new CubicSpline
            {
                Points = this.Points.Reverse().ToArray()
            };

        public IDrawable ReflectY()
            => new CubicSpline
            {
                Points = this.Points.Select(p => p.Conjugate).ToArray()
            };

        public IDrawable RotatedBy(double phi, Coordinate pivot)
            => new CubicSpline
            {
                Points = this.Points
                    .Select(p => p.RotateAbout(pivot, phi))
                    .ToArray()
            };

        public IDrawable Translated(Coordinate offset)
            => new CubicSpline
            {
                Points = this.Points
                    .Select(p => p.Offset(offset))
                .ToArray()
            };
    }
}
