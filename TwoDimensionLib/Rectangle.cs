namespace TwoDimensionLib
{
    /// <summary>
    /// Representation of a pair of points that form opposite
    /// corners of an X/Y axis aligned rectangle
    /// </summary>
    public readonly record struct Rectangle(Coordinate Location, double Width, double Height)
    {
        public static readonly Rectangle Empty = new(Coordinate.Empty, 0, 0);

        public bool IsEmpty => this == Empty;

        public Rectangle Offset(Coordinate c)
            => Offset(c.X, c.Y);

        public Rectangle Offset(double x, double y)
            => new(Location.Offset(x, y), Width, Height);

        public Rectangle Inflate(Coordinate dc)
            => Inflate(dc.X, dc.Y);

        public Rectangle Inflate(double dx, double dy)
            => new(Location.Offset(-dx, -dy), Width + 2 * dx, Height + 2 * dy);

        public Rectangle Resized(double w, double h)
            => new(Location, w, h);

        public Rectangle Moved(Coordinate newLocation)
            => new(newLocation, Width, Height);

        public double Top => Location.Y;

        public double Bottom => Top + Height;

        public double Left => Location.X;

        public double Right => Left + Width;

        public override string ToString()
            => $"{Location}, W={Width}, H={Height}";

    }
}
