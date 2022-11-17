﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib
{
    public class TchebyshevApproximator
    {
        /// <summary>
        /// The function we wish to approximate
        /// </summary>
        
        public Func<double, double> Function { get; init; }
        
        /// <summary>
        /// The lower bound of the range of values that
        /// will be passed to the function for it to 
        /// approximate
        /// </summary>
    
        public double MinimumValue { get; init; }

        /// <summary>
        /// The upper bound of the range of values that
        /// will be passed to the function for it to 
        /// approximate
        /// </summary>

        public double MaximumValue { get; init; }

        /// <summary>
        /// The highest degree desired in the
        /// approximating polynomial. The lower
        /// the degree, the worse the approximation.
        /// </summary>
        
        public int Degree { get; init; }

        /// <summary>
        /// Construct a TchebyshevApproximator 
        /// </summary>
        /// <param name="degree">The degree of the Tchebyshev
        /// approximation polynomial to be generated</param>
        /// <param name="func">The function to approximate</param>
        /// <param name="min">The minimum value for the function 
        /// variable (i.e. the 'x' in f(x))</param>
        /// <param name="max">The maximum value for the
        /// function variable</param>
        
        public TchebyshevApproximator
            (int degree, Func<double, double> func, double min, double max)
        {
            Function = func;
            MinimumValue = min;
            MaximumValue = max;
            Degree = degree;
            TchebyshevPolys = CalcTchebyshevPolys(Degree);
            Coefficients = CalcCoefficients();
        }

        /// <summary>
        /// Given an input value x, calculate the
        /// approximated value of f(x)
        /// </summary>

        public double Approximate(double x)
        {
            double result = 0;
            for (int i = 0; i <= Degree; i++)
            {
                result += Coefficients[i] * TchebyshevPolys[i]
                    .Evaluate(MapXToU(x));
            }
            return result;
        }

        /// <summary>
        /// Compute the polynomial that is used for the
        /// approximation of the input function. Note that
        /// the only reason you'd want to use this rather
        /// than the Approximate() function above would be
        /// if you needed the coefficients of this polynomial
        /// rather than the computed result. That might be
        /// the case if you were looking for degree 3
        /// polynomial approximations for example, because
        /// you were planning to use them to construct
        /// Bezier cubic splines that fit the original
        /// function.
        /// </summary>
        
        public Polynomial ApproximationPolynomial
        {
            get
            {
                Polynomial mapXToU = new Polynomial
                    ((MinimumValue + MaximumValue)
                    / (MinimumValue - MaximumValue),
                    2 / (MaximumValue - MinimumValue));
                Polynomial result = Polynomial.Zero();
                for(int i = 0; i <= Degree; i++)
                {
                    result = result.Plus(TchebyshevPolys[i]
                        .Transform(mapXToU)
                        .Scale(Coefficients[i]));
                }
                return result;
            }
        }

        /* IMPLEMENTATION */

        /// <summary>
        /// Linear mapping of a variable in the range
        /// min to max onto the range -1 to + 1
        /// </summary>
        /// <param name="val">The value to be mapped</param>
        /// <returns>The mapped value in the range
        /// -1 to +1</returns>

        private double MapXToU(double val)
        {
            return (2 * val - MaximumValue - MinimumValue) 
                / (MaximumValue - MinimumValue);
        }

        /// <summary>
        /// Reverse mapping to a variable in the range
        /// min to max from the range -1 to + 1
        /// </summary>
        /// <param name="val">The value to be unmapped</param>
        /// <returns>The unmapped value in the range
        /// min to max</returns>

        private double MapUToX(double u)
        {
            return u * (MaximumValue - MinimumValue) / 2 
                + (MaximumValue + MinimumValue) / 2;
        }

        /// <summary>
        /// Lists all the Tchebyshev polynomials from 0 up to
        /// and including the degree of this approximator
        /// </summary>
        
        private IList<Polynomial> TchebyshevPolys { get; init; }

        private IList<double> Coefficients { get; init; }

        /// <summary>
        /// Compute the first 'degree' Tchebyshev polynomials
        /// </summary>
        /// <param name="degree">The degree of the highest
        /// degree polynomial to be generated</param>
        /// <returns>The list of polynomials</returns>
        
        private static IList<Polynomial> CalcTchebyshevPolys(int degree)
        {
            Polynomial twox = new Polynomial(0.0, 2.0);
            List<Polynomial> polys = new List<Polynomial>(degree + 1);
            polys.Add(new Polynomial(1.0)); // 1.0
            polys.Add(new Polynomial(0.0, 1.0)); // x
            for(int i = 2; i <= degree; i++)

            {
                polys.Add(polys[i - 1]
                    .MultiplyBy(twox)
                    .Minus(polys[i - 2])); ;
            }
            return polys;
        }

        private static double Root(int root, int degree)
        {
            if (root < 0 || root > degree)
                throw new ArgumentException("Root index out of range");
            else
                return Math.Cos(Math.PI * (root + 0.5) / degree);
        }

        /// <summary>
        /// Compute one of the coefficients of the
        /// approximation polynomial
        /// </summary>
        /// <param name="k">The degree of the Tchebyshev
        /// polynomial this computed coefficient will
        /// multiply in the evaluation of the
        /// polynomial</param>
        /// <returns>The kth coefficient</returns>
        
        private double CalcCoefficient(int k)
        {
            double ck = 0;
            for(int i = 0; i <= Degree; i++)
            {
                double root = Root(i, Degree + 1);
                ck += Function(MapUToX(root)) 
                    * TchebyshevPolys[k].Evaluate(root);
            }
            ck /= (Degree + 1);
            return k == 0 ? ck : 2 * ck;
        }

        /// <summary>
        /// Precompute the set of coefficients to be used
        /// for this polynomial approximation
        /// </summary>
        /// <returns>The set of coefficients</returns>
        
        private IList<double> CalcCoefficients() 
            => new List<double>(Enumerable
                .Range(0, Degree + 1)
                .Select(i => CalcCoefficient(i)));
    }
}