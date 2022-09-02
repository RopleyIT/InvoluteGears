namespace TwoDimensionLib
{
    /// <summary>
    /// Implementation of points in a two-dimensional coordinate system
    /// using double precision floating point numbers. The PointF class
    /// in System.Drawing loses too much precision in some scenarios,
    /// and does not support rotation or magnitude operations. It also
    /// suffers from being part of System.Drawing, which Microsoft has
    /// decided not to support on non-Windows platforms.
    /// </summary>
    /// <param name="X">The X cartesian coordinate value</param>
    /// <param name="Y">The Y cartesian coordinate value</param>

    public readonly record struct Coordinate(double X, double Y)
    {
        public static readonly Coordinate Empty = new(0.0, 0.0);

        public bool IsEmpty => this == Empty;

        public Coordinate Offset(Coordinate c)
            => Offset(c.X, c.Y);

        public Coordinate Offset(double x, double y)
            => new(X + x, Y + y);

        public Coordinate Scale(double scaleFactor)
            => Scale(scaleFactor, scaleFactor);

        public Coordinate Scale(Coordinate c)
            => Scale(c.X, c.Y);

        public Coordinate Scale(double x, double y)
            => new(X * x, Y * y);

        public static Coordinate operator +(Coordinate c, Coordinate d)
            => c.Offset(d.X, d.Y);

        public static Coordinate operator -(Coordinate c)
            => new(-c.X, -c.Y);

        public static Coordinate operator -(Coordinate c, Coordinate d)
            => c.Offset(-d);

        public static Coordinate operator *(Coordinate c, double s)
            => c.Scale(s);

        public static Coordinate operator *(Coordinate c, Coordinate d)
            => c.Scale(d);

        public static Coordinate FromPolar(double magnitude, double angle)
            => new Coordinate(Math.Cos(angle), Math.Sin(angle)).Scale(magnitude);

        public Coordinate Conjugate => new(X, -Y);

        public double Magnitude => Geometry.RootSumOfSquares(X, Y);

        public double Phase => Math.Atan2(Y, X);

        public double Gradient => Geometry.SafeDiv(Y, X);

        public Coordinate Rotate(double angle)
        {
            var cosAngle = Math.Cos(angle);
            var sinAngle = Math.Sin(angle);
            return new Coordinate(X * cosAngle - Y * sinAngle, X * sinAngle + Y * cosAngle);
        }

        public Coordinate RotateAbout(Coordinate origin, double angle)
            => origin + (this - origin).Rotate(angle);

        public override string ToString() => $"({X}, {Y})";
    }
}