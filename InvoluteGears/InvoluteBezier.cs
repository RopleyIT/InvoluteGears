using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoDimensionLib;

namespace InvoluteGears
{
    /// <summary>
    /// Library functions to plot an involute curve
    /// as a small number of Bezier splines. Work
    /// acknowledgement credit to Dr A.R.Collins 
    /// <http://www.arc.id.au/>, also to Higuchi
    /// et al., Approximation of Involute Curves
    /// for CAD System Processing, Engineering
    /// with Computers, September 2007.
    /// </summary>
    
    public static class InvoluteBezier
    {
        /// <summary>
        /// Given a maximum degree for the list of
        /// polynomials, calculate the list of
        /// Chebyshev polynomial coefficients for
        /// all polynomials up to and including
        /// degree 'maxDegree'.
        /// </summary>
        /// <param name="maxDegree">The maximum power of 'x'
        /// in the list of polynomials</param>
        /// <returns>The triangular array of polynomial
        /// coefficients for the Chebyshev polynomials. Each
        /// row (left square bracket) contains the coefficients
        /// for the polynomial whose degree matches the
        /// row index.</returns>
        
        public static int[][] ComputeChebyshevCoeffs(int maxDegree)
        {
            // Each row of the 2D array (left hand square bracket)
            // contains the coefficients for the Chebyshev polynomial
            // of its corresponding degree (row index).
            // Each column index represents the power x is raised to.

            int[][] coeffs = new int[maxDegree + 1][];
            coeffs[0] = new int[1];
            coeffs[0][0] = 1; // T0 = 1
            coeffs[1] = new int[2];
            coeffs[1][0] = 0;
            coeffs[1][1] = 1; // T1 = x

            // Now use the recurrence relation T(k+1) = 2*x*T(k) - T(k-1)
            // to generate subsequence polynomials up the the desired
            // degree. Note this creates a triangular array where row
            // N has N+1 elements in it (N is zero-based).
            
            for(int degree = 2; degree <= maxDegree; degree++)
            {
                coeffs[degree] = new int[degree + 1];
                coeffs[degree][0] = 0;
                for (int i = 1; i <= degree; i++)
                    coeffs[degree][i] = 2 * coeffs[degree - 1][i - 1];
                for (int i = 0; i < degree - 1; i++)
                    coeffs[degree][i] -= coeffs[degree - 2][i];
            }
            return coeffs;
        }

        /// <summary>
        /// Return the jth coefficient of the Chebyshev polynomial that approximates
        /// the function passed as an argument. The accuracy of the approximation is
        /// determined by N, where higher values improve the accuracy significantly.
        /// </summary>
        /// <param name="N">The number of points to include in the summation when
        /// generating the approximation</param>
        /// <param name="j">Which coefficient degree is being generated</param>
        /// <param name="function">The function we are trying to approximate</param>
        /// <returns>The jth coefficient of the approximation polynomial</returns>
        
        public static double ChebyshevExpCoefficient(int N, int j, Func<double, double> function)
        {
            double cj = 0;  
            for (int k = 1; k <= N; k++)
            {
                cj += function(Math.Cos(Math.PI * (k - 0.5)/N)) 
                    * Math.Cos(Math.PI * j * (k - 0.5)/N);
            }
            return 2 * cj / N;
        }

        /// <summary>
        /// Compute the coefficients of the polynomial that approximates
        /// the curve described by 'function'
        /// </summary>
        /// <param name="N">The number of steps in the Chebyshev approximation</param>
        /// <param name="maxDegree">The highest degree for the polynomial</param>
        /// <param name="function">The function we are trying to approximate</param>
        /// <returns></returns>
        public static double[] ChebyshevPoly(int N, int maxDegree, Func<double, double> function)
        {
            int[][] chebyCoeffs = ComputeChebyshevCoeffs(maxDegree);
            double[] coeffs = new double[chebyCoeffs.Length];
            for(int k = 0; k <= maxDegree; k++)
            {
                double coeffk = ChebyshevExpCoefficient(N, k, function);
                coeffs[k] = 0;
                for(int i = 0; i <= k; i++)
                    coeffs[i] += coeffk * chebyCoeffs[k][i];
            }
            coeffs[0] -= ChebyshevExpCoefficient(N, 0, function) / 2;
            return coeffs;
        }

        /// <summary>
        /// Given the Bezier variable 0 <= t <= 1, find the involute
        /// angle that corresponds to it
        /// </summary>
        /// <param name="t">The Bezier parametric variable</param>
        /// <param name="endAngle">The angle for the outermost
        /// end of the involute</param>
        /// <param name="startAngle">The angle for the innermost
        /// end of the involute segment being plotted</param>
        /// <returns>The angle with the X axis to the point at which
        /// the string unwrapping from the circle leaves the circle's
        /// surface and becomes taught</returns>
        
        private static double AngleFromBezier(double t, double startAngle, double endAngle)
        {
            // First map 0 <= t < 1 onto Chebyshev range -1 <= t < 1
            double c = 2 * t - 1;
            // Now map this onto an angle between the start and end angle
            double midAngle = (startAngle + endAngle) / 2;
            double scaleFactor = (endAngle - startAngle) / 2;
            return midAngle + c * scaleFactor;
        }

        /// <summary>
        /// Calculate the X coordinate value of the point on the involute
        /// corresponding to bezier parametric value t
        /// </summary>
        /// <param name="t">The Bezier parametric value</param>
        /// <param name="startAngle">The beginning angle of the part of 
        /// the Bezier curve being approximated</param>
        /// <param name="endAngle">The ending angle</param>
        /// <param name="baseCircleRadius">The radius of the base
        /// circle the involute is being plotted for</param>
        /// <returns>The X coordinate on the involute</returns>

        private static double XCoordFromBezier(double t, double startAngle, 
            double endAngle, double baseCircleRadius)
        {
            double angle = AngleFromBezier(t, startAngle, endAngle);
            return baseCircleRadius * (Math.Cos(angle) + angle * Math.Sin(angle));
        }

        /// <summary>
        /// Calculate the Y coordinate value of the point on the involute
        /// corresponding to bezier parametric value t
        /// </summary>
        /// <param name="t">The Bezier parametric value</param>
        /// <param name="startAngle">The beginning angle of the part of 
        /// the Bezier curve being approximated</param>
        /// <param name="endAngle">The ending angle</param>
        /// <param name="baseCircleRadius">The radius of the base
        /// circle the involute is being plotted for</param>
        /// <returns>The Y coordinate on the involute</returns>

        private static double YCoordFromBezier(double t, double startAngle,
            double endAngle, double baseCircleRadius)
        {
            double angle = AngleFromBezier(t, startAngle, endAngle);
            return baseCircleRadius * (Math.Sin(angle) - angle * Math.Cos(angle));
        }

        /// <summary>
        /// Calculate the binomial coefficient n! / (r! * (n-r)!)
        /// </summary>
        /// <param name="n">The larger combination value</param>
        /// <param name="r">The smaller combination value</param>
        /// <returns>the binomial coefficient</returns>
        
        private static double BinomialCoefficient(int n, int r)
        {
            double coeff = 1;
            for (int i = n - r + 1; i <= n; i++)
                coeff *= i;
            for (int i = 1; i <= r; i++)
                coeff /= i;
            return coeff;
        }

        /// <summary>
        /// Calculate the number of module lengths 
        /// radially between the pitch circle and
        /// the base circle for an involute gear
        /// </summary>
        /// <param name="teeth">Number of teeth on gear</param>
        /// <param name="pressureAngle">Contact pressure
        /// angle for the gear teeth</param>
        /// <returns>Number of modules between the
        /// pitch circle and the base circle</returns>
        
        public static double ModuleFractionFromPitchToBaseCircle
            (int teeth, double pressureAngle) 
            => teeth * (1 - Math.Cos(pressureAngle)) / 2;

        /// <summary>
        /// Calculate the Bezier control points for a particular
        /// section of the involute of a gear tooth
        /// </summary>
        /// <param name="module">The module for the gear</param>
        /// <param name="teeth">Number of teeth on the gear</param>
        /// <param name="pressureAngle">The contact angle between
        /// meshing teeth (radians)</param>
        /// <param name="bezierOrder">The order of the Bezier curve
        /// </param>
        /// <param name="fStart">The fraction between 0 and 1 along
        /// the involute at which the Bezier curve begins</param>
        /// <param name="fStop">The fraction along the involute
        /// from base circle to addendum at which the Bezier
        /// curve ends</param>
        /// <param name="addendum">The number of whole modules
        /// the tooth protrudes past the pitch circle. By convention
        /// this has the value 1.0, but may be smaller or larger
        /// depending on whether there is a positive or negative
        /// profile shift.</param>
        /// <param name="dedendum">The number of whole modules
        /// negative from the pitch circle in to the start of the
        /// involute curve. This is usually -1, or more if the
        /// base circle is too close to the pitch circle as
        /// happens for gears with small numbers of teeth.</param>
        /// <returns>The Bezier control points, 'order' in
        /// number</returns>

        public static Coordinate[] BezierPoints(double module, int teeth, 
            double pressureAngle, int bezierOrder, double dedendum, double addendum)
        {
            const double bezierShift = 1.0001; // To avoid discontinuity at base circle
            double modulesFromPitchToBase 
                = ModuleFractionFromPitchToBaseCircle(teeth, pressureAngle);
            double pitchRadius = module * teeth / 2;
            double baseCircleRadius = pitchRadius - module * modulesFromPitchToBase;
            double addendumRadius = pitchRadius + addendum * module;
            double dedendumRadius = Math.Max
                (pitchRadius + dedendum * module, baseCircleRadius * bezierShift);
            double endAngle = Geometry
                .RootDiffOfSquares(addendumRadius, baseCircleRadius) / baseCircleRadius;
            double startAngle = Geometry
                .RootDiffOfSquares(dedendumRadius, baseCircleRadius) / baseCircleRadius;
            const int N = 64; // Used to set precision of Chebyshev approximation
            var chebyXCoeffs = ChebyshevPoly(N, bezierOrder, 
                t => XCoordFromBezier(t, startAngle, endAngle, baseCircleRadius));
            var chebyYCoeffs = ChebyshevPoly(N, bezierOrder,
                t => YCoordFromBezier(t, startAngle, endAngle, baseCircleRadius));
            Coordinate[] bezierPoints = new Coordinate[bezierOrder + 1];
            for(int i = 0; i <= bezierOrder; i++)
            {
                double x = 0, y = 0;
                for(int j = 0; j <= i; j++)
                {
                    double binomial = BinomialCoefficient(i, j);
                    binomial /= BinomialCoefficient(bezierOrder, j);
                    x += binomial * chebyXCoeffs[j]; 
                    y += binomial * chebyYCoeffs[j];
                }
                bezierPoints[i] = new Coordinate(x, y);
            }
            return bezierPoints;
        }
    }
}
