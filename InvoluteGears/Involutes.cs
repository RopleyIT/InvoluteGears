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

            double cosPhiTotal = Math.Cos(phi + phiOffset);
            double sinPhiTotal = Math.Sin(phi + phiOffset);

            double x = radius * (cosPhiTotal + phi * sinPhiTotal) + offX * cosPhiTotal - offY * sinPhiTotal;
            double y = radius * (sinPhiTotal - phi * cosPhiTotal) + offX * sinPhiTotal + offY * cosPhiTotal;
            return new PointF((float)x, (float)y);
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
        /// Calculate the contact ratio for two gears. The gears must be
        /// compatible from a meshing point of view (same module, same
        /// pressure angle). The contact ratio is the average number of teeth
        /// in contact with each other on the pressure-bearing faces as
        /// the gears are rotating. Ideally this should be > 1.1, but the
        /// absolute minimum is 1 for the gears to not 'click' as they turn.
        /// Note that this is the ideal figure, valid for no backlash, no
        /// profile offset, and no undercut. It is the theoretical maximum
        /// value that is never achieved!
        /// </summary>
        /// <param name="g1">The first gear being meshed</param>
        /// <param name="g2">The second gear being meshed</param>
        /// <returns>The contact ratio</returns>
        
        public static double IdealContactRatio(GearParameters g1, GearParameters g2)
        {
            if(g1 == null || !g1.CanMeshWith(g2))
                throw new ArgumentException("Gears have differing modules or pressure angles");

            double gear1 = 0.5 * RootDiffOfSquares
                (g1.PitchCircleDiameter + 2*g1.Module, g1.BaseCircleDiameter);
            double gear2 = 0.5 * RootDiffOfSquares
                (g2.PitchCircleDiameter + 2*g2.Module, g2.BaseCircleDiameter);
            return (gear1 + gear2 - Math.Sin(g1.PressureAngle) 
                * (g1.PitchCircleDiameter + g2.PitchCircleDiameter) / 2)
                / g1.BaseCirclePitch;
        }

        public static double RootDiffOfSquares(double a, double b) 
            => Math.Sqrt((a + b) * (a - b));

        /// <summary>
        /// Compute the intersection point of a straight line drawn from p11
        /// to p12 with a line drawn from p21 to p22. If they don't intersect
        /// return null. If they do, return the intersection point.
        /// </summary>
        /// <param name="p11">End of first line</param>
        /// <param name="p12">Other end of first line</param>
        /// <param name="p21">End of second line</param>
        /// <param name="p22">Other end of second line</param>
        /// <returns>Intersection point or null if lines do not
        /// intersect between their endpoints</returns>

        public static PointF? CrossAt(PointF p11, PointF p12, PointF p21, PointF p22)
        {
            // Find gradients of lines

            double m1 = Gradient(p12, p11);
            double m2 = Gradient(p22, p21);

            // Parallel lines do not intersect

            if (m1 == m2)
                return null;

            // Calculate the intersection point

            double x = (p22.Y - p12.Y + m1 * p12.X - m2 * p22.X) / (m1 - m2);
            double y = m1 * (x - p11.X) + p11.Y;

            // Check that lines cross

            if (Between(x, p11.X, p12.X) && Between(y, p11.Y, p12.Y))
                return new PointF((float)x, (float)y);
            else
                return null;
        }

        private static bool Between(double v, double r1, double r2)
            => r1 > v && v >= r2 || r2 > v && v >= r1;

        private static double Gradient(PointF p1, PointF p2)
            =>p2.X == p1.X ? double.MaxValue : (p2.Y - p1.Y) / (p2.X - p1.X);

        /// <summary>
        /// Given a list of PointF structures sorted in decreasing
        /// value of X property, find the index of the last item
        /// in the list whose X property is greater than xVal
        /// </summary>
        /// <param name="list">The list to search</param>
        /// <param name="xVal">The value we are comparing
        /// each element's X value against</param>
        /// <param name="start">The index into the list
        /// at which to begin the search. Defaults to
        /// the start of the list</param>
        /// <returns>The index of the latest item in the
        /// list whose X value is greater than the
        /// specified xVal argument. Returns -1 if none
        /// of the items in the list is greater than the
        /// specified value. Returns the index of the
        /// last item in the list if they all are.</returns>
        
        public static int IndexOfLastPointWithGreaterXVal
            (List<PointF> list, double xVal)
        {
            // Larger X values near beginning of list.
            // Sorted in decreasing order of X value.

            int i = 0;
            while (i < list.Count && list[i].X > xVal)
                i++;
            return i - 1;
        }

        /// <summary>
        /// Given two lists of points, find the point at which lines
        /// drawn from point to point in each list first intersect.
        /// </summary>
        /// <param name="ptList1">The first list of points, sorted in
        /// order of decreasing X value</param>
        /// <param name="ptList2">The second list of points, sorted
        /// in order of decreasing X value</param>
        /// <returns>The intersection point of lines. Returns null
        /// if none of the line segments intersects lines in the
        /// opposite list.</returns>

        public static PointF? Intersection(List<PointF> ptList1, List<PointF> ptList2)
        {
            // First clone each list so that we don't destroy the originals

            var list1 = new List<PointF>(ptList1);
            var list2 = new List<PointF>(ptList2);

            // Populate list1 with extra points having same X values as list 2,
            // then list2 with extra points having same X values as list 1

            foreach (PointF p in list2)
                InjectPointWithSameXVal(list1, p.X);
            foreach (PointF p in list1)
                InjectPointWithSameXVal(list2, p.X);

            // Search for a cross over between the lines in the two lists

            for (int i = 0; i < list1.Count - 1; i++)
            {
                var crossingPoint = CrossAt(list1[i], list1[i + 1], list2[i], list2[i + 1]);
                if (crossingPoint.HasValue)
                    return crossingPoint;
            }
            return null;
        }

        /// <summary>
        /// Given two lists of points, find the X value that corresponds to
        /// their closest Y values. Ideally they should be tangential at this
        /// point.
        /// </summary>
        /// <param name="ptList1">One list of points</param>
        /// <param name="ptList2">The second list of points</param>
        /// <returns>The X value at which the curves through the points are closest</returns>
        
        public static PointF ClosestPoint(List<PointF> ptList1, List<PointF> ptList2)
        {
            // First clone each list so that we don't destroy the originals

            var list1 = new List<PointF>(ptList1);
            var list2 = new List<PointF>(ptList2);

            // Populate list1 with extra points having same X values as list 2,
            // then list2 with extra points having same X values as list 1

            foreach (PointF p in list2)
                InjectPointWithSameXVal(list1, p.X);
            foreach (PointF p in list1)
                InjectPointWithSameXVal(list2, p.X);

            // Search for the closest points between the lines in the two lists

            float closestYValue = float.MaxValue;
            int closestIndex = 0;
            for (int i = 0; i < list1.Count - 1; i++)
            {
                float absYDiff = Math.Abs(list1[i].Y - list2[i].Y);
                if (absYDiff < closestYValue)
                {
                    closestIndex = i;
                    closestYValue = absYDiff;
                }
                else break;
            }
            return list1[closestIndex];
        }

        /// <summary>
        /// Given a list of points, and an X value, insert a point in the list
        /// that has the specified X value, with a Y value colinear with the
        /// two points either side of it. If a point with the X value already
        /// exists, do not insert one. If the X value is off either end of
        /// the list, a point is inserted at the corresponding end of the list,
        /// colinear with the first or last pair of points as appropriate.
        /// </summary>
        /// <param name="ptList">The list into which we shall insert
        /// a new point</param>
        /// <param name="x">The X value for the new point</param>

        public static void InjectPointWithSameXVal(List<PointF> ptList, double x)
        {
            // List is assumed sorted in order of decreasing X value

            if (ptList == null || ptList.Count <= 1)
                throw new ArgumentException
                    ("List must have at least two points in it");
            if (x > ptList.First().X)
                ptList.Insert(0, FindPoint(x, ptList[1], ptList[0]));
            else if (x < ptList.Last().X)
                ptList.Add(FindPoint(x, ptList[ptList.Count - 2], ptList[ptList.Count - 1]));
            else
            {
                int i = 0;
                while (i < ptList.Count && ptList[i].X > x)
                    i++;
                if(ptList[i].X < x)
                    ptList.Insert(i, FindPoint(x, ptList[i - 1], ptList[i]));
            }
        }

        /// <summary>
        /// Given the X value for a new point, and the two points pt1
        /// and pt2 on a straight line, find another point on the same
        /// straight line that has the specified X value.
        /// </summary>
        /// <param name="x">The X value for the new point</param>
        /// <param name="pt1">One of two points on the straight line</param>
        /// <param name="pt2">The other point on the straight line</param>
        /// <returns>The new point with the specified X value, and
        /// colinear with pt1 and pt2</returns>
        
        public static PointF FindPoint(double x, PointF pt1, PointF pt2)
        {
            double m = (pt2.Y - pt1.Y) / (pt2.X - pt1.X);
            double y = pt1.Y + (x - pt1.X) * m;
            return new PointF((float)x, (float)y);
        }

        /// <summary>
        /// Given a sequence of points, create a list of
        /// points from it that removes any that lie within
        /// an error margin of the original list.
        /// </summary>
        /// <param name="source">The original sequence</param>
        /// <param name="maxErr">The perpendicular error margin</param>
        /// <returns>A new reduced list of points</returns>
        
        public static List<PointF> LinearReduction(IList<PointF> source, float maxErr)
        {
            var result = new List<PointF>();
            int startIndex = 0;
            while(startIndex < source.Count - 1)
            {
                result.Add(source[startIndex]);
                startIndex = IndexOfFarthestPointWithinMargin(source, startIndex, maxErr);
            }
            result.Add(source.Last());
            return result;
        }

        private static int IndexOfFarthestPointWithinMargin(IList<PointF> source, int startIndex, float maxErr)
        {
            for(int i = source.Count - 1; i > startIndex; i--)
            {
                if(MaxDistance(source.Skip(startIndex).Take(i - 1 - startIndex), source[startIndex], source[i]) < maxErr)
                    return i;
            }
            return startIndex + 1; // Should never reach this
        }

        /// <summary>
        /// Given a sequence of points, find the maximum perpendicular distance
        /// any one of them has from the straight line through p1 and p2
        /// </summary>
        /// <param name="innerPoints">The sequence of points</param>
        /// <param name="p1">One of two points on the line</param>
        /// <param name="p2">The other of two points on the line</param>
        /// <returns>The longest distance from any of the points to the line</returns>
        
        private static float MaxDistance(IEnumerable<PointF> innerPoints, PointF p1, PointF p2)
        {
            var maxDist = 0f;
            foreach(PointF p in innerPoints)
            {
                var dist = PerpendicularDistance(p, p1, p2);
                if (dist > maxDist)
                    maxDist = dist;
            }
            return maxDist;
        }

        /// <summary>
        /// For a point x0,y0 its shortest distance to the straight line
        /// through points x1,y1 and x2,y2 is given by:
        /// Math.Abs((y2-y1)*x0 - (x2-x1)*y0 + x2*y1 - y2*x1) divided by:
        /// Math.Sqrt((y2-y1)**2 + (x2-x1)**2)
        /// </summary>
        /// <param name="p0">The point we wish to measure the distance of</param>
        /// <param name="p1">One of two points on the line</param>
        /// <param name="p2">The other of two points on the line</param>
        /// <returns>The shortest distance from the point to the line</returns>
        
        public static float PerpendicularDistance(PointF p0, PointF p1, PointF p2)
        {
            double numerator = (p2.Y - p1.Y) * p0.X;
            numerator -= (p2.X - p1.X) * p0.Y;
            numerator += p2.X * p1.Y - p2.Y * p1.X;
            numerator = Math.Abs(numerator);
            double denom = Square(p2.Y - p1.Y);
            denom += Square(p2.X - p1.X);
            denom = Math.Sqrt(denom);
            return (float)(numerator / denom);
        }

        private static double Square(double v) => v * v;

    }
}
