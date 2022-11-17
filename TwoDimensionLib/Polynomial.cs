using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib
{
    /// <summary>
    /// Manage a scalar polynomial
    /// </summary>
    
    public class Polynomial
    {
        /// <summary>
        /// The scalar coefficients for the polynomial
        /// </summary>
        
        private IList<double> Coefficients { get; set; }

        /// <summary>
        /// Index into the coefficients for the polynomial.
        /// This implementation hides the allocation of
        /// space in the list collection holding the
        /// coefficients, as well as the removal of zero
        /// coefficients at the higher powers.
        /// </summary>
        /// <param name="index">The power of 'x' the
        /// coefficient applies to</param>
        /// <returns>The coefficient for the selected
        /// power of 'x'</returns>
        
        public double this[int index] 
        {
            get
            {
                if (index >= 0 && index < Coefficients.Count)
                    return Coefficients[index];
                else
                    return 0.0;
            }

            set
            {
                if (value == 0.0)
                {
                    if (index < Coefficients.Count)
                    {
                        Coefficients[index] = value;
                        NormaliseOrder();
                    }
                }
                else
                {
                    while (Coefficients.Count <= index)
                        Coefficients.Add(0.0);
                    Coefficients[index] = (value);
                }
            } 
        }

        /// <summary>
        /// If the polynomial were redescribed as a
        /// Bernstein polynomial, capture the Bernstein
        /// coefficients.
        /// </summary>
        /// <param name="index">The index of the
        /// Bernstein coeffiient to calculate</param>
        /// <returns>The indexth Bernstein coefficient
        /// </returns>
        
        public double BernsteinCoefficient(int index)
        {
            if (index < 0 || index > Order)
                return 0;
            double coeff = 0;
            for(int i = 0; i <= index; i++)
            {
                coeff += this[i] * Combination(i, index) 
                    / Combination(i, Order);
            }
            return coeff;
        }

        /// <summary>
        /// Calculate the combination value or the
        /// binomial coefficient nCr. This would be
        /// the number of combinations of k cards
        /// drawn from a deck of n cards.
        /// </summary>
        /// <param name="n">The upper value</param>
        /// <param name="k">The lower value</param>
        /// <returns>The binomial coefficient</returns>
        
        public static int Combination(int k, int n) 
            => RangeProduct(n - k + 1, n) / RangeProduct(1, k);

        /// <summary>
        /// For the values k and n where k < n,
        /// calculate the value n(n-1)(n-2)...(k+1)k
        /// </summary>
        /// <param name="lower">The lower number in
        /// the product range</param>
        /// <param name="upper">The upper number in
        /// the product range</param>
        /// <returns>The product of all integers
        /// in the range</returns>
        
        private static int RangeProduct(int lower, int upper)
        {
            int result = 1;
            while (lower <= upper)
                result *= lower++;
            return result;
        }

        /// <summary>
        /// Any high order zero coefficients are removed
        /// to ensure the degree of the polynomial matches
        /// the index of the last element in the coefficient
        /// list.
        /// </summary>

        private void NormaliseOrder()
        {
            for (int i = Order; i >= 0; i--)
                if (Coefficients[i] == 0)
                    Coefficients.RemoveAt(i);
                else
                    break;
        }

        /// <summary>
        /// The order of the polynomial
        /// </summary>

        public int Order => Coefficients.Count - 1;

        /// <summary>
        /// Given an input value, calculate the value of
        /// the polynomial at that input value
        /// </summary>
        /// <param name="x">The input value</param>
        /// <returns>The value of f(x)</returns>
        
        public double Evaluate(double x)
        {
            double y = 0;
            for (int i = Order; i >= 0; i--)
            {
                y *= x;
                y += this[i];
            }
            return y;
        }

        /// <summary>
        /// Apply a transformation to the current 
        /// polynomial f(x) by substituting a
        /// polynomial 'map(x)' for x to generate
        /// the new polynomial f(map(x))
        /// </summary>
        /// <param name="map">The polynomial
        /// that applies the mapping</param>
        /// <returns>The newly mapped polynomial
        /// </returns>
        
        public Polynomial Transform(Polynomial map)
        {
            Polynomial result = Zero();
            Polynomial powMap = One();
            for(int i = 0; i <= Order; i++)
            {
                result = result.Plus
                    (powMap.Scale(this[i]));
                if(i < Order)
                    powMap = powMap.MultiplyBy(map);
            }
            return result;
        }
        
        /// <summary>
        /// Create a new polynomial that is the product
        /// of this and another polynomial
        /// </summary>
        /// <param name="other">The polynomial to multiply
        /// this polynomial by</param>
        /// <returns>The new product polynomial</returns>
        
        public Polynomial MultiplyBy(Polynomial other)
        {
            Polynomial result = Zero();

            // Ensure the power terms are collected correctly

            for (int i = 0; i <= Order; i++)
                for (int j = 0; j <= other.Order; j++)
                    result[i + j] += this[i] * other[j];
            return result;
        }

        /// <summary>
        /// Generate a polynomial that
        /// is the current polynomial 
        /// raised to the power p
        /// </summary>
        /// <param name="p">The integer power to
        /// which we wish to raise the current
        /// polynomial. Must be zero or positive.
        /// </param>
        /// <returns>The new polynomial</returns>
        
        public Polynomial Power(int p)
        {
            Polynomial result = Polynomial.One();
            while (p-- > 0)
                result = MultiplyBy(result);
            return result;
        }

        /// <summary>
        /// Create a polynomial that is the sum
        /// of the current polynomial plus the
        /// argument polynomial
        /// </summary>
        /// <param name="other">The argument
        /// polynomial to be added</param>
        /// <returns>The sum of the two
        /// polynomials</returns>
        
        public Polynomial Plus(Polynomial other)
        {
            Polynomial result = Zero();
            for(int i = 0; i <= Math.Max(Order, other.Order); i++)
                result[i] = this[i] + other[i];
            return result;
        }

        /// <summary>
        /// Create a polynomial that is the current
        /// polynomial minus the polynomial passed
        /// as an argument
        /// </summary>
        /// <param name="other">The subtrahend
        /// polynomial</param>
        /// <returns>The difference polynomial
        /// </returns>
        
        public Polynomial Minus(Polynomial other)
        {
            Polynomial result = Zero();
            for (int i = 0; i <= Math.Max(Order, other.Order); i++)
                result[i] = this[i] - other[i];
            return result;
        }

        /// <summary>
        /// Multiply the whole polynomial by
        /// a scalar value
        /// </summary>
        /// <param name="s">The scalar value</param>
        /// <returns>The polynomial multiplied
        /// by the scalar value</returns>
        
        public Polynomial Scale(double s)
        {
            Polynomial result = Zero();
            if (s != 0)
                for (int i = 0; i <= Order; i++)
                    result[i] = this[i] * s;
            return result;
        }

        /// <summary>
        /// The polynomial representing unity
        /// </summary>
        /// <returns> 1.0 as a polynomial</returns>
        
        public static Polynomial One()
            => new Polynomial(1.0);

        /// <summary>
        /// The zero-valued polynomial
        /// </summary>
        /// <returns>0.0 as a polynomial</returns>
        
        public static Polynomial Zero()
            => new Polynomial();

        public Polynomial() => Coefficients = new List<double>();

        /// <summary>
        /// Construct a polynomial from a variadic argument
        /// list of coefficient values. The lowest degree
        /// coefficient comes first.
        /// </summary>
        /// <param name="coeffs">The coefficients for the
        /// powers of the polynomial variable</param>
        
        public Polynomial(params double[] coeffs) : this()
        {
            for (int i = 0; i < coeffs.Length; i++)
                this[i] = coeffs[i];
        }

        /// <summary>
        /// Render polynomial in a human friendly fashion
        /// </summary>
        /// <returns>The polynomial in a descriptive form</returns>
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = Order; i > 1; i--)
            {
                sb.Append($"{this[i]}X^{i}");
                if (this[i - 1] >= 0)
                    sb.Append('+');
            }
            if (Order > 0)
            {
                sb.Append($"{this[1]}X");
                if (this[0] >= 0)
                    sb.Append('+');
            }
            sb.Append(this[0]);
            return sb.ToString();
        }
    }
}
