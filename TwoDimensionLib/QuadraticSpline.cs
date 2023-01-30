namespace TwoDimensionLib
{
    public class QuadraticSpline : IDrawable
    {
        public Coordinate[] Points { get; set; } = new Coordinate[3];

        public Coordinate Start => Points[0];

        public Coordinate End => Points[2];

        public Rectangle Bounds
        {
            get
            {
                // An approximation as the control point
                // could be well outside the true bounds.
                // Here we apply de Casteljau's bisection
                // once to pull the control points closer
                // to the spline.

                Coordinate leftMid =
                    Geometry.MidPoint(Points[0], Points[1]);
                Coordinate rightMid =
                    Geometry.MidPoint(Points[1], Points[2]);
                BoundsTracker b = new BoundsTracker();
                b.Track(Start);
                b.Track(End);
                b.Track(leftMid);
                b.Track(rightMid);
                return b.Bounds;
            }
        }

        public IDrawable Reversed()
            => new QuadraticSpline
            {
                Points = this.Points.Reverse().ToArray()
            };

        public IDrawable ReflectY()
            => new QuadraticSpline
            {
                Points = this.Points.Select(p => p.Conjugate).ToArray()
            };

        public IDrawable RotatedBy(double phi, Coordinate pivot)
            => new QuadraticSpline
            {
                Points = this.Points
                    .Select(p => p.RotateAbout(pivot, phi))
                    .ToArray()
            };

        public IDrawable Translated(Coordinate offset)
            => new QuadraticSpline
            {
                Points = this.Points
                    .Select(P => P.Offset(offset))
                    .ToArray()
            };
    }
}
