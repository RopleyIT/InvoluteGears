namespace TwoDimensionLib
{
    public class Line : IDrawable
    {
        public Line(Coordinate s, Coordinate e)
        {
            Start = s;
            End = e;
        }

        public Line(double xs, double ys, double xe, double ye)
            : this(new Coordinate(xs, ys), new Coordinate(xe, ye)) { }
        public Line() : this(Coordinate.Empty, Coordinate.Empty) { }

        public Coordinate Start { get; set; }
        public Coordinate End { get; set; }

        public Rectangle Bounds
        {
            get
            {
                BoundsTracker b = new BoundsTracker();
                b.Track(Start);
                b.Track(End);
                return b.Bounds;
            }
        }

        public IDrawable RotatedBy(double phi, Coordinate pivot)
            => new Line(Start.RotateAbout(pivot, phi),
                End.RotateAbout(pivot, phi));

        public IDrawable ReflectY()
            => new Line(Start.Conjugate, End.Conjugate);

        public IDrawable Reversed()
            => new Line(End, Start);

        public IDrawable Translated(Coordinate offset)
            => new Line(Start.Offset(offset), End.Offset(offset));
    }
}
