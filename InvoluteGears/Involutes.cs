using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;

namespace InvoluteGears
{
    /// <summary>
    /// Algorithms and calculators used for
    /// computing points on an involute curve
    /// </summary>
    
    public static class Involutes
    {
        /// <summary>
        /// Obtain a point on an involute or a trochoidal curve
        /// </summary>
        /// <param name="radius">The radius of the base circle on which the involute is formed</param>
        /// <param name="offX">Assuming the involute touches the base circle along the +ve
        /// X axis, with the circle centred on the origin, offX is the X component of the
        /// offset to the point being traced as a locus relative to the involute. This value and
        /// the offY value must both be zero to draw the involute itself. Non-zero values
        /// trace out the trochoid path, as offX and offY also rotate about the point on the involute
        /// at the same angular rate as the involute tangential point rotates.</param>
        /// <param name="offY">The Y component of the offset from the point on the involute</param>
        /// <param name="phi">The angle away from the involute contact point at the right of the circle.
        /// Note that anticlockwise angles are positive.</param>
        /// <param name="phiOffset">The angle by which the involute's touch point differs from the
        /// right of the circle. The whole involute is rotated around the outside of the circle
        /// by this amount. Note that the trochoid is rotated by the same amount.</param>
        /// <returns>The computed X, Y coordinate for the parameters supplied.</returns>
        
        public static PointF InvolutePlusOffset(double radius, double offX, double offY, double phi, double phiOffset)
        {
            double cosPhi = Math.Cos(phi);
            double sinPhi = Math.Sin(phi);
            double cosOff = Math.Cos(phiOffset);
            double sinOff = Math.Sin(phiOffset);

            // Basic involute

            double x = radius * (cosPhi + phi * sinPhi);
            double y = radius * (sinPhi - phi * cosPhi); 

            // Transform the offset to produce the trochoidal curve

            x += offX * cosPhi - offY * sinPhi;
            y += offX * sinPhi + offY * cosPhi;

            // Rotate the whole involute or trochoid around the larger circle - anticlockwise positive

            return new PointF((float)(x * cosOff - y * sinOff), (float)(x * sinOff + y * cosOff));
        }

        /// <summary>
        /// Calculate a point on the circumference of a circle
        /// </summary>
        /// <param name="radius">The radius of the circle, centred on the origin</param>
        /// <param name="phi">The angle around the circle for which we want a point. Note
        /// that the zero angle is aligned horizontally along the positive X axis.</param>
        /// <returns>The point on the circle.</returns>

        public static PointF Circle(double radius, double phi) 
            => new PointF((float)(radius * Math.Cos(phi)), (float)(radius * Math.Sin(phi)));

        /// <summary>
        /// Generate a sequence of points on a circle, centred on the origin
        /// </summary>
        /// <param name="startAngle">The starting angle on the curve. 0 represents the
        /// point on the circle where it crosses the positive X axis, with anticlockwise
        /// angle values being positive. Given the centre of the circle is set at the
        /// origin, the touch point is set to the right of the circle (Y = 0
        /// and X positive)</param>
        /// <param name="endAngle">The angle beyond which no points are added to
        /// the list of output points</param>
        /// <param name="dAngle">The delta value for the angle between each point</param>
        /// <param name="radius">The radius of the circle</param>
        /// <returns>The set of points on the circumference of the circle</returns>
        
        public static IEnumerable<PointF> CirclePoints
            (double startAngle, double endAngle, double dAngle, double radius)
        {
            int pointCount = (int)((endAngle - startAngle) / dAngle);
            return Enumerable
                .Range(0, pointCount)
                .Select(i => Circle(radius, startAngle + i * dAngle));
        }

        /// <summary>
        /// Generate a sequence of points on an involute or trochoidal curve
        /// </summary>
        /// <param name="startAngle">The starting angle on the curve. 0 represents the
        /// point at which the involute touches the base circle, with anticlockwise
        /// angle values being positive. Given the centre of the base circle is set
        /// at the origin, the touch point is set to the right of the circle (Y = 0
        /// and X positive)</param>
        /// <param name="endAngle">The angle beyond which no points are added to
        /// the list of output points</param>
        /// <param name="dAngle">The delta value for the angle between each point</param>
        /// <param name="radius">The radius of the base circle on which the involute is formed</param>
        /// <param name="offX">Assuming the involute touches the base circle at the right of the
        /// circle, where the circle is centred on the origin, offX is the X component of the
        /// offset to the point being traced as a locus relative to the involute. This value and
        /// the offY value must both be zero to draw the involute itself. Non-zero values
        /// trace out the trochoid path, as offX and offY also rotate about the point on the involute
        /// at the same angular rate as the involute tangential point rotates.</param>
        /// <param name="offY">The Y component of the offset from the point on the involute</param>
        /// <param name="phi">The angle away from the involute contact point at the right of the circle.
        /// Note that anticlockwise angles are positive.</param>
        /// <param name="phiOffset">The angle by which the involute's touch point differs from the
        /// right of the circle. The whole involute is rotated around the outside of the circle
        /// by this amount. Note that the trochoid is rotated by the same amount.</param>
        /// <returns>The list of points on the involute or trochoid between the starting
        /// and the ending angle</returns>

        public static IEnumerable<PointF> InvolutePlusOffsetPoints
        (
            double startAngle, double endAngle, double dAngle, double radius, 
            double offX, double offY, double phiOffset
        )
        {
            int pointCount = (int)((endAngle - startAngle) / dAngle);
            return Enumerable
                .Range(0, pointCount)
                .Select(i => InvolutePlusOffset(radius, offX, offY, startAngle + i * dAngle, phiOffset));
        }

        /// <summary>
        /// Calculate the contact ratio for two gears. The gears must be
        /// compatible from a meshing point of view (same module, same
        /// pressure angle). The contact ratio is the average number of teeth
        /// in contact with each other on the pressure-bearing faces as
        /// the gears are rotating. Ideally this should be > 1.1, but the
        /// absolute minimum is 1 for the gears to not 'click' as they turn.
        /// </summary>
        /// <param name="g1">The first gear being meshed</param>
        /// <param name="g2">The second gear being meshed</param>
        /// <returns>The contact ratio</returns>
        
        public static double ContactRatio(GearParameters g1, GearParameters g2)
        {
            if (g1.PressureAngle != g2.PressureAngle)
                throw new ArgumentException("Gears have differing pressure angles");
            if (g1.Module != g2.Module)
                throw new ArgumentException("Gears have differing teeth separation");

            double gear1 = 0.5 * RootDiffOfSquares(g1.PitchCircleDiameter + 2*g1.Module, g1.BaseCircleDiameter);
            double gear2 = 0.5 * RootDiffOfSquares(g2.PitchCircleDiameter + 2*g2.Module, g2.BaseCircleDiameter);
            return (gear1 + gear2 - Math.Sin(g1.PressureAngle) * (g1.PitchCircleDiameter + g2.PitchCircleDiameter) / 2)
                / g1.BaseCirclePitch;
        }

        private static double RootDiffOfSquares(double a, double b)
        {
            return Math.Sqrt(a * a - b * b);
        }
    }
}
