namespace TwoDimensionLib
{
    /// <summary>
    /// This class creates generalised Bezier splines
    /// that approximate a range of input values onto
    /// 2 dimensional coordinates.
    /// </summary>

    public class Spline
    {
        /// <summary>
        /// The order of the spline. Use 3 for a
        /// cubic spline, and 2 for quadratic splines.
        /// </summary>

        public int Order { get; init; }

        /// <summary>
        /// The generated control points for the
        /// spline, approximating the function
        /// named Curve
        /// </summary>
        public Coordinate[] ControlPoints { get; init; }

        /// <summary>
        /// The parametric function mapping the input
        /// double scalar value onto the 2 dimensional
        /// coordinate that is its return value
        /// </summary>

        private readonly Func<double, Coordinate> Curve;

        /// <summary>
        /// The lower input value in the range to be mapped
        /// </summary>

        private readonly double StartVal;

        /// <summary>
        /// The upper input value in the range to be mapped
        /// </summary>

        private readonly double EndVal;

        /// <summary>
        /// Constructor, creating the approximation to the
        /// parametric function supplied between the start
        /// and end values for the input parameter
        /// </summary>
        /// <param name="order">The order of the generated
        /// polynomial approximating the input function,
        /// also the order of the Bernstein polynomial
        /// </param>
        /// <param name="func">The function mapping the
        /// input scalar parameter onto 2D coordinates</param>
        /// <param name="start">The minimum input value</param>
        /// <param name="end">The maximum input value</param>

        public Spline(int order, Func<double, Coordinate> func,
            double start, double end)
        {
            Order = order;
            Curve = func;
            StartVal = start;
            EndVal = end;
            ControlPoints = new Coordinate[Order + 1];
            ComputeSpline();
        }

        /// <summary>
        /// Given the input range, the function to be
        /// approximated and the order of the spline,
        /// generate the spline polynomial using
        /// Tchebyshev curve fitting, and mapping the
        /// monomial coefficients of the resulting
        /// polynomial onto the Bernstein coefficients
        /// so that the Bezier control points can be
        /// captured.
        /// </summary>

        private void ComputeSpline()
        {
            // Find the polynomials corresponding to the
            // nearest approximation to the input function

            var tax = new TchebyshevApproximator
                (Order, v => Curve(v).X, StartVal, EndVal);
            var tay = new TchebyshevApproximator
                (Order, v => Curve(v).Y, StartVal, EndVal);

            // Now map the polynomials so that they produce
            // the curve within the values 0 to 1

            Polynomial mapVToT = new (StartVal, EndVal - StartVal);
            Polynomial xBezier = tax.ApproximationPolynomial
                .Transform(mapVToT);
            Polynomial yBezier = tay.ApproximationPolynomial
                .Transform(mapVToT);
            for (int i = 0; i <= Order; i++)
            {
                ControlPoints[i] = new Coordinate(
                    xBezier.BernsteinCoefficient(i),
                    yBezier.BernsteinCoefficient(i));
            }
        }
    }
}
