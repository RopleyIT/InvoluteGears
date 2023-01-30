namespace TwoDimensionLib
{
    /// <summary>
    /// Representation of a pair of points that form opposite
    /// corners of an X/Y axis aligned rectangle
    /// </summary>
    public readonly record struct Rectangle
    {
        public static readonly Rectangle Empty = new(Coordinate.Empty, 0, 0);

        public Coordinate Location { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }

        /// <summary>
        /// Construct a rectangle from its TLHC, width and height. Note
        /// that if the width is negative or the height, the location
        /// will be changed so that the rectangle is normalised to have
        /// a positive width and height.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        
        public Rectangle(Coordinate location, double width, double height)
            : this(location, new Coordinate(location.X + width, location.Y + height)) { }

        /// <summary>
        /// Create a normalised rectangle from two arbitrary coordinates
        /// </summary>
        /// <param name="tlhc">The notional top left coordinate, but could
        /// be below or right of brhc</param>
        /// <param name="brhc">The notional bottom right coordinate, but
        /// coule be above or left of tlhc</param>
        
        public Rectangle(Coordinate tlhc, Coordinate brhc)
        {
            Location = new(Math.Min(tlhc.X, brhc.X), Math.Min(tlhc.Y, brhc.Y));
            Width = Math.Abs(brhc.X - tlhc.X);
            Height = Math.Abs(brhc.Y - tlhc.Y);
        }

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

        public bool Surrounds(Coordinate c)
        {
            return Top <= c.Y && Bottom >= c.Y 
                && Left <= c.X && Right >= c.X;
        }

        public Rectangle Union(Coordinate c)
        {
            if(Surrounds(c))
                return this;
            else
            {
                double top = Math.Min(Top, c.Y);
                double bottom = Math.Max(Bottom, c.Y);
                double left = Math.Min(Left, c.X);
                double right = Math.Max(Right, c.X);
                return new(new(left, top), right-left, bottom-top);
            }
        }

        public Rectangle Union(Rectangle r) 
            => Union(r.Location).Union(new Coordinate(r.Right, r.Bottom));

        public double Top => Location.Y;

        public double Bottom => Top + Height;

        public double Left => Location.X;

        public double Right => Left + Width;

        public override string ToString()
            => $"{Location}, W={Width}, H={Height}";
    }
}
