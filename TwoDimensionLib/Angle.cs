namespace TwoDimensionLib;

/// <summary>
/// Representation of an angle,
/// implemented as a unit vector
/// to make the retrieval of sin,
/// cos and tan fast to implement.
/// </summary>

public class Angle
{
    public const double RadiansPerDegree = Math.PI / 180;
    public const double DegresPerRadian = 1 / RadiansPerDegree;

    double real;
    double imaginary;

    public Angle()
    {
        real = 1.0;
        imaginary = 0.0;
    }

    public double Radians
    {
        get
        {
            return Math.Atan2(imaginary, real);
        }
        set
        {
            real = Math.Cos(value);
            imaginary = Math.Sin(value);
        }
    }

    public double Degrees
    {
        get
        {
            return Radians * DegresPerRadian;
        }
        set
        {
            Radians = value * RadiansPerDegree;
        }
    }

    public override string ToString()
    {
        var radians = Math.Atan2(imaginary, real);
        return $"{radians}rad ({radians * DegresPerRadian}°)";
    }

    public double Cos => real;
    public double Sin => imaginary;

    public double Tan => imaginary / real;

    public double Cot => real / imaginary;

    public double Sec => 1 / real;

    public double Cosec => 1 / imaginary;

    public static Angle operator -(Angle a) => new()
    {
        real = a.real,
        imaginary = -a.imaginary
    };

    public static Angle operator +(Angle a, Angle b) => new()
    {
        real = a.real * b.real - a.imaginary * b.imaginary,
        imaginary = a.real * b.imaginary + b.real * a.imaginary
    };

    public static Angle operator -(Angle a, Angle b) => a + (-b);

    /// <summary>
    /// Generate an Angle object from a cosine value. Note that
    /// the result could be a positive or negative angle since
    /// there are two roots to the RootDiffOfSquares operation.
    /// This function always returns the positive root, i.e. an
    /// angle in the range 0 .. Math.PI.
    /// </summary>
    /// <param name="val">The cosine value</param>
    /// <returns>The cosine's positive corresponding angle</returns>
    /// <exception cref="ArgumentException">A cosine value must
    /// lie betwen +1 and -1</exception>

    public static Angle ACos(double val)
    {
        if (val < -1 || val > 1)
            throw new ArgumentException("Trig argument out of range");
        return new Angle
        {
            real = val,
            imaginary = Geometry.RootDiffOfSquares(1, val)
        };
    }

    /// <summary>
    /// Generate an Angle object from a sine value. Note that
    /// the result could be in the range -Math.PI/2 to Math.PI/2
    /// or in the range Math.PI/2 to 3*Math.PI/2 as there
    /// are two roots to the RootDiffOfSquares operation.
    /// This function always returns the root for the smaller 
    /// angle, i.e. an angle in the range -Math.PI/2 .. Math.PI/2.
    /// </summary>
    /// <param name="val"></param>
    /// <param name="val">The sine value</param>
    /// <returns>The sine's corresponding angle</returns>
    /// <exception cref="ArgumentException">A sine value must
    /// lie betwen +1 and -1</exception>

    public static Angle ASin(double val)
    {
        if (val < -1 || val > 1)
            throw new ArgumentException("Trig argument out of range");
        return new Angle
        {
            imaginary = val,
            real = Geometry.RootDiffOfSquares(1, val)
        };
    }

    /// <summary>
    /// Given the Y and X values, calculate the value
    /// Arctan(Y/X), using the signs of Y and X to
    /// ensure the angle lies in the correct quadrant.
    /// </summary>
    /// <param name="im">The Y value</param>
    /// <param name="re">The X value</param>
    /// <returns>An angle object set to the correct
    /// angle</returns>

    public static Angle ATan(double im, double re)
    {
        double magnitude = Geometry.RootSumOfSquares(im, re);
        return new Angle
        {
            real = re / magnitude,
            imaginary = im / magnitude
        };
    }

    /// <summary>
    /// Calculate the angle corresponding to a single
    /// tan value Y/X. As the sign of Y/X loses the
    /// separate sign information of Y and X individually,
    /// this only returns the angle in the range
    /// -Math.PI/2 to +Math.PI/2.
    /// </summary>
    /// <param name="v">The tangent ratio</param>
    /// <returns>The angle that has this tangent
    /// value</returns>

    public static Angle ATan(double v) => ATan(v, 1);

    public static Angle ACot(double v) => ACot(v, 1);

    public static Angle ACot(double im, double re) => ATan(re, im);

    public static Angle ASec(double v) => ACos(1 / v);

    public static Angle ACosec(double v) => ASin(1 / v);
}
