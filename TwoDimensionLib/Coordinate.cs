namespace TwoDimensionLib
{
    /// <summary>
    /// Implementation of points in a two-dimensional coordinate system
    /// using double precision floating point numbers. The PointF class
    /// in System.Drawing loses too much precision in some scenarios,
    /// and does not support rotation or magnitude operations.
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    public readonly record struct Coordinate(double X, double Y)
    {
        public static readonly Coordinate Empty = new(0.0, 0.0);

        public bool IsEmpty => this == Empty;

        public Coordinate Offset(Coordinate c)
            => Offset(c.X, c.Y);

        public Coordinate Offset(double X, double Y)
            => new(this.X + X, this.Y + Y);

        public Coordinate Scale(double scaleFactor)
            => new(scaleFactor * X, scaleFactor * Y);

        public static Coordinate operator +(Coordinate c, Coordinate d)
            => c.Offset(d.X, d.Y);

        public static Coordinate operator -(Coordinate c)
            => new(-c.X, -c.Y);

        public static Coordinate operator -(Coordinate c, Coordinate d)
            => c.Offset(-d);

        public static Coordinate FromPolar(double magnitude, double angle)
            => new Coordinate(Math.Cos(angle), Math.Sin(angle)).Scale(magnitude);

        public Coordinate Conjugate => new(X, -Y);

        public double Magnitude => Math.Sqrt(SumOfSquares(X, Y));

        public static double SumOfSquares(double x, double y) => x * x + y * y;

        public static double DiffOfSquares(double x, double y) => x * x - y * y;

        public double Phase => Math.Atan2(Y, X);

        public double Gradient =>
            X == 0 ? (Y < 0 ? double.MinValue : double.MaxValue) : Y / X;

        public Coordinate Rotate(double angle)
        {
            var cosAngle = Math.Cos(angle);
            var sinAngle = Math.Sin(angle);
            return new Coordinate(X * cosAngle - Y * sinAngle, X * sinAngle + Y * cosAngle);
        }

        public Coordinate RotateAbout(Coordinate origin, double angle)
            => origin + (this - origin).Rotate(angle);

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}