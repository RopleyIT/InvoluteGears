namespace TwoDimensionLib;

public static class Geometry
{
    /// <summary>
    /// Provide an implementation of the range adding method to an IList
    /// </summary>
    /// <typeparam name="T">The type of things in the list</typeparam>
    /// <param name="ilist">The list to add to</param>
    /// <param name="src">The source enumerable of items to be added</param>
    /// <exception cref="ArgumentException">Both arguments shold be non-null</exception>

    public static void AddRange<T>(this IList<T> ilist, IEnumerable<T> src)
    {
        if (ilist is null || src is null)
            throw new ArgumentException("AddRange passed a null list or null range");
        if (ilist is List<T> list)
            list.AddRange(src);
        else
            foreach (T t in src)
                ilist.Add(t);
    }

    /// <summary>
    /// Remove a range of elements from a list, given the indexes for the range
    /// </summary>
    /// <typeparam name="T">The type of each element in the list</typeparam>
    /// <param name="ilist">The list of elements from which elements will be removed
    /// </param>
    /// <param name="first">The index of the first removed item</param>
    /// <param name="count">The number of elements to be removed</param>
    /// <exception cref="ArgumentException">Thrown if the list is null, or if the
    /// forst index or count take the removal outside the valid indices for
    /// the list</exception>

    public static void RemoveRange<T>(this IList<T> ilist, int first, int count)
    {
        if (ilist is null)
            throw new ArgumentException("RemoveRange passed a null list or null range");
        if (count > 0 && first >= 0 && first + count <= ilist.Count)
        {
            if (ilist is List<T> list)
                list.RemoveRange(first, count);
            else
                for (int i = 0; i < count; i++)
                    ilist.RemoveAt(first);
        }
    }

    /// <summary>
    /// Rotate a sequence of points about the origin in the anticlockwise
    /// direction by the angle phi
    /// </summary>
    /// <param name="points">The points from which a rotated
    /// <param name="phi">The angle to rotate by in radians</param>
    /// point sequence will be generated</param>
    /// <returns>The sequence of rotated points, or Enumerable.Empty if the
    /// points argument is null</returns>

    public static IEnumerable<Coordinate> Rotated(this IEnumerable<Coordinate>? points, double phi)
        => points?.Select(p => p.Rotate(phi)) ?? Enumerable.Empty<Coordinate>();

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

    public static Coordinate InvolutePlusOffset
        (double radius, double offX, double offY, double phi, double phiOffset)
    {

        double cosPhiTotal = Math.Cos(phi + phiOffset);
        double sinPhiTotal = Math.Sin(phi + phiOffset);

        double x = radius * (cosPhiTotal + phi * sinPhiTotal)
            + offX * cosPhiTotal - offY * sinPhiTotal;
        double y = radius * (sinPhiTotal - phi * cosPhiTotal)
            + offX * sinPhiTotal + offY * cosPhiTotal;
        return new Coordinate(x, y);
    }

    /// <summary>
    /// Find the coordinate mid way between two other coordinates
    /// </summary>
    /// <param name="l">Left coordinate</param>
    /// <param name="r">Right coordinate</param>
    /// <returns>Midpoint</returns>
    
    public static Coordinate MidPoint(Coordinate l, Coordinate r)
        => new Coordinate((l.X + r.X) / 2, (l.Y + r.Y) / 2);

    /// <summary>
    /// Convert an angle from degrees to radians
    /// </summary>
    /// <param name="degrees">Angle value in degrees</param>
    /// <returns>The same angle in radians</returns>
    
    public static double DegToRad(double degrees)
        => degrees * Math.PI / 180;

    /// <summary>
    /// Convert an angle from radians to degrees
    /// </summary>
    /// <param name="radians">Anglue value in radians</param>
    /// <returns>The same angle in degrees</returns>
    
    public static double RadToDeg(double radians)
        => radians * 180 / Math.PI;

    /// <summary>
    /// Given an angle that is arbitrarily big or small
    /// map its value into the range 0 ... 2 * Math.PI
    /// </summary>
    /// <param name="angle">The un-normalised angle</param>
    /// <returns>The angle mapped into the first 2 Pi radians</returns>
    
    public static double NormaliseAngle(double angle)
        => angle - 2 * Math.PI * Math.Floor(angle / (2 * Math.PI));

    /// <summary>
    /// Find point on epicycloid. A pitch circle of radius 'radius' has a locus
    /// wheel of radius 'locusRadius' rolloing anticlockwise around it. At the
    /// point (radius, 0) the locus lies on the pitch circle, hence the -PI in
    /// the function below.
    /// </summary>
    /// <param name="radius">Radius of the pitch circle</param>
    /// <param name="locusRadius">Radius of the rolling wheel a point on the
    /// circumference of which traces out the epicycloid</param>
    /// <param name="phi">The angle around the pitch circle of the contact
    /// point between the two circles, also the angle relative to the X
    /// axis of the line of centres between the two circles</param>
    /// <returns>The point on the locus corresponding to the selected
    /// angle around the pitch circle</returns>

    public static Coordinate Epicycloid(double radius, double locusRadius, double phi)
    {
        Coordinate locusCentre = Coordinate.FromPolar(radius + locusRadius, phi);
        double locusAngle = -Math.PI + phi * (1 + radius / locusRadius);
        Coordinate locus = Coordinate.FromPolar(locusRadius, locusAngle);
        return locusCentre + locus;
    }

    /// <summary>
    /// Find point on hypocycloid. A pitch circle of radius 'radius' has a locus
    /// wheel of radius 'locusRadius' rolloing anticlockwise inside it. At the
    /// point (radius, 0) the locus lies on the pitch circle.
    /// </summary>
    /// <param name="radius">Radius of the pitch circle</param>
    /// <param name="locusRadius">Radius of the rolling wheel a point on the
    /// circumference of which traces out the hypocycloid</param>
    /// <param name="phi">The angle around the pitch circle of the contact
    /// point between the two circles, also the angle relative to the X
    /// axis of the line of centres between the two circles</param>
    /// <returns>The point on the locus corresponding to the selected
    /// angle around the pitch circle</returns>

    public static Coordinate Hypocycloid(double radius, double locusRadius, double phi)
    {
        Coordinate locusCentre = Coordinate.FromPolar(radius - locusRadius, phi);
        double locusAngle = phi * (1 - radius / locusRadius);
        Coordinate locus = Coordinate.FromPolar(locusRadius, locusAngle);
        return locusCentre + locus;
    }

    /// <summary>
    /// Generate a sequence of points on a circle that is centred on a nominated point
    /// </summary>
    /// <param name="startAngle">The starting angle on the curve. 0 represents the
    /// point on the circle where it crosses a line parallel to the positive X axis, 
    /// with anticlockwise angles being positive.</param>
    /// <param name="endAngle">The angle beyond which no points are added to
    /// the list of output points</param>
    /// <param name="dAngle">The delta value for the angle between each point</param>
    /// <param name="radius">The radius of the circle</param>
    /// <param name="centre">The centre for the circle from which the points
    /// are computed</param>
    /// <returns>The set of points on the circumference of the circle</returns>

    public static IEnumerable<Coordinate> CirclePoints
        (double startAngle, double endAngle, double dAngle, double radius, Coordinate centre)
        => CirclePoints(startAngle, endAngle, dAngle, radius)
            .Select(p => centre.Offset(p));

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
    /// are computed</param>
    /// <returns>The set of points on the circumference of the circle</returns>

    public static IEnumerable<Coordinate> CirclePoints
        (double startAngle, double endAngle, double dAngle, double radius)
    {
        double angle = startAngle;
        for (int i = 0; angle < endAngle; angle = startAngle + dAngle * ++i)
            yield return Coordinate.FromPolar(radius, angle);
        yield return Coordinate.FromPolar(radius, endAngle);
    }

    /// <summary>
    /// Compute the suware of a number
    /// </summary>
    /// <param name="v">The expression to be squared</param>
    /// <returns>The square of the value</returns>

    public static double Square(double v) => v * v;

    /// <summary>
    /// Divide two numbers, saturating at double.MaxValue or
    /// double.MinValue if the denominator is zero. Note that
    /// if the numerator and denominator are both 0, the value
    /// returned is double.MaxValue, not undefined
    /// </summary>
    /// <param name="y">The numerator</param>
    /// <param name="x">The denominator</param>
    /// <returns>The safe division result for the two numbers</returns>

    public static double SafeDiv(double y, double x)
        => x == 0 ? y >= 0 ? double.MaxValue : double.MinValue : y / x;

    /// <summary>
    /// Given two values, compute the sum of the squares of each
    /// value. Used in geometry for determining magnitudes and
    /// gradients
    /// </summary>
    /// <param name="x">First value to be squared</param>
    /// <param name="y">Second value to be squared</param>
    /// <returns>The sum of squares of the two arguments</returns>

    public static double SumOfSquares(double x, double y) => x * x + y * y;

    /// <summary>
    /// Calculate the root sum of squares of two values, as per
    /// Pythagoras for finding the length of a hypotenuse
    /// </summary>
    /// <param name="x">First value to be squared</param>
    /// <param name="y">Second value to be squared</param>
    /// <returns>The root sum of squares</returns>

    public static double RootSumOfSquares(double x, double y)
        => Math.Sqrt(SumOfSquares(x, y));

    /// <summary>
    /// Given two values, calculate the difference between their squares.
    /// A positive result is returned if the first argument is greater in
    /// magnitude than the second
    /// </summary>
    /// <param name="x">The first value to be squared</param>
    /// <param name="y">The value to be squared and subtracted</param>
    /// <returns>The difference in the squares of the two numbers</returns>

    public static double DiffOfSquares(double x, double y) => (x + y) * (x - y);

    /// <summary>
    /// Compute the value (a^2 - b^2)^0.5 if ^ is the power operator
    /// </summary>
    /// <param name="a">The first value to be squared</param>
    /// <param name="b">The value to be squared and subtracted</param>
    /// <returns>The value (a^2 - b^2)^0.5 if ^ is the power operator</returns>

    public static double RootDiffOfSquares(double a, double b)
        => Math.Sqrt(DiffOfSquares(a, b));

    /// <summary>
    /// Determine if a point lies inside a rectangle formed by two other points
    /// </summary>
    /// <param name="c">The point we are inspecting to see if it is within
    /// the rectangle</param>
    /// <param name="corner1">First corner of containing rectangle</param>
    /// <param name="corner2">Second corner of containing rectangle</param>
    /// <returns>True if coordinate lies within the rectangle</returns>

    public static bool InRectangle(Coordinate c, Coordinate corner1, Coordinate corner2)
        => Between(c.X, corner1.X, corner2.X) && Between(c.Y, corner1.Y, corner2.Y);

    /// <summary>
    /// Compute the intersection point of a straight line drawn from p11
    /// to p12 with a line drawn from p21 to p22. If they don't intersect
    /// return null. If they do, return the intersection point.
    /// </summary>
    /// <param name="p11">End of first line</param>
    /// <param name="p12">Other end of first line</param>
    /// <param name="p21">End of second line</param>
    /// <param name="p22">Other end of second line</param>
    /// <returns>Intersection point or false if lines do not
    /// intersect between their endpoints</returns>

    public static (bool Found, Coordinate Value)
        CrossAt(Coordinate p11, Coordinate p12, Coordinate p21, Coordinate p22)
    {
        // Find gradients of lines

        double m1 = (p12 - p11).Gradient;
        double m2 = (p22 - p21).Gradient;

        // Parallel lines do not intersect

        if (m1 != m2)
        {
            // Calculate the intersection point

            double x = (p22.Y - p12.Y + m1 * p12.X - m2 * p22.X) / (m1 - m2);
            double y = m1 * (x - p11.X) + p11.Y;
            Coordinate result = new(x, y);

            // Make sure the intersection point
            // doesn't lie beyond either line end

            if (InRectangle(result, p11, p12))
                return (true, result);
        }
        return (false, Coordinate.Empty);
    }

    /// <summary>
    /// Given two straight lines, find their point of intersection
    /// </summary>
    /// <param name="m0">Gradient of first line</param>
    /// <param name="c0">Y axis intersection value of first line</param>
    /// <param name="m1">Gradient of second line</param>
    /// <param name="c1">Y axis intersection value of second line</param>
    /// <returns>The point of intersection</returns>

    public static Coordinate LineIntersection(double m0, double c0, double m1, double c1)
    {
        if (m0 == m1)
            throw new ArgumentException("Lines are parallel");
        double x = (c0 - c1) / (m1 - m0);
        return new Coordinate(x, m0 * x + c0);
    }

    /// <summary>
    /// Find the 0, 1 or 2 points of intersection
    /// between a straight line and a circle
    /// </summary>
    /// <param name="centre">Coordinate of the circle's centre</param>
    /// <param name="radius">The radius of the circle</param>
    /// <param name="lineGradient">The gradient of the straight line</param>
    /// <param name="yOffset">The value of the straight line equation
    /// when the x value is zero</param>
    /// <returns>An enumeration of the coordinates of intersection. The
    /// coordinates are delivered in order of increasing x value.</returns>

    public static IEnumerable<Coordinate> CircleLineIntersection
        (Coordinate centre, double radius, double lineGradient, double yOffset)
    {
        double a = 1 + Square(lineGradient);
        double b = 2 * lineGradient * (yOffset - centre.Y) - 2 * centre.X;
        double c = Square(centre.X) + Square(yOffset - centre.Y) - Square(radius);
        return SolveQuadratic(a, b, c)
            .Select(x => new Coordinate(x, lineGradient * x + yOffset));
    }

    /// <summary>
    /// Compute the roots of a quadratic function, using the well-known
    /// formula: "Minus B plus or minus the square root of B squared minus 4AC
    /// all over 2A"
    /// </summary>
    /// <param name="a">Coefficient of x squared</param>
    /// <param name="b">Coefficient of x</param>
    /// <param name="c">Constant coefficient</param>
    /// <returns>An enumeration of the roots found. Zero length
    /// if no roots, length one or two to match the number
    /// of roots found</returns>

    public static IEnumerable<double> SolveQuadratic(double a, double b, double c)
    {
        double rootTerm = Square(b) - 4 * a * c;
        if (rootTerm >= 0)
            yield return 0.5 * (-b - Math.Sqrt(rootTerm)) / a;
        if (rootTerm > 0)
            yield return 0.5 * (-b + Math.Sqrt(rootTerm)) / a;
    }

    /// <summary>
    /// Find if a value lies within a determined range
    /// </summary>
    /// <param name="v">The value under test</param>
    /// <param name="r1">One end of the range</param>
    /// <param name="r2">The other end of the range</param>
    /// <returns>True if the number is within range</returns>

    public static bool Between(double v, double r1, double r2)
        => v < r1 && v >= r2 || v < r2 && v >= r1;

    /// <summary>
    /// Given a list of Coordinates sorted in decreasing
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
        (IList<Coordinate> list, double xVal)
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

    public static (bool Found, Coordinate Value)
        Intersection(IList<Coordinate> ptList1, IList<Coordinate> ptList2)
    {
        // First clone each list so that we don't destroy the originals

        List<Coordinate> list1 = new(ptList1);
        List<Coordinate> list2 = new(ptList2);

        // Populate list1 with extra points having same X values as list 2,
        // then list2 with extra points having same X values as list 1

        foreach (Coordinate p in list2)
            InjectPointWithSameXVal(list1, p.X);
        foreach (Coordinate p in list1)
            InjectPointWithSameXVal(list2, p.X);

        // Search for a cross over between the lines in the two lists

        for (int i = 0; i < list1.Count - 1; i++)
        {
            var crossPt = CrossAt(list1[i], list1[i + 1], list2[i], list2[i + 1]);
            if (crossPt.Found)
                return crossPt;
        }
        return (false, Coordinate.Empty);
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

    public static void InjectPointWithSameXVal(IList<Coordinate> ptList, double x)
    {
        // List is assumed sorted in order of decreasing X value

        if (ptList == null || ptList.Count <= 1)
            throw new ArgumentException
                ("List must have at least two points in it");
        if (x > ptList.First().X)
            ptList.Insert(0, FindPoint(x, ptList[1], ptList[0]));
        else if (x < ptList.Last().X)
            ptList.Add(FindPoint(x, ptList[^2], ptList[^1]));
        else
        {
            int i = 0;
            while (i < ptList.Count && ptList[i].X > x)
                i++;
            if (ptList[i].X < x)
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

    public static Coordinate FindPoint(double x, Coordinate pt1, Coordinate pt2)
    {
        double m = (pt2.Y - pt1.Y) / (pt2.X - pt1.X);
        double y = pt1.Y + (x - pt1.X) * m;
        return new Coordinate(x, y);
    }

    /// <summary>
    /// Given two lists of points, find the X value that corresponds to
    /// their closest Y values. Ideally they should be tangential at this
    /// point.
    /// </summary>
    /// <param name="ptList1">One list of points</param>
    /// <param name="ptList2">The second list of points</param>
    /// <returns>The X value at which the curves through the points are closest</returns>

    public static Coordinate ClosestPoint(IList<Coordinate> ptList1, IList<Coordinate> ptList2)
    {
        // First clone each list so that we don't destroy the originals

        List<Coordinate> list1 = new(ptList1);
        List<Coordinate> list2 = new(ptList2);

        // Populate list1 with extra points having same X values as list 2,
        // then list2 with extra points having same X values as list 1

        foreach (Coordinate p in ptList2)
            InjectPointWithSameXVal(list1, p.X);
        foreach (Coordinate p in ptList1)
            InjectPointWithSameXVal(list2, p.X);

        // Search for the closest points between the lines in the two lists

        double closestYValue = double.MaxValue;
        int closestIndex = 0;
        for (int i = 0; i < list1.Count - 1; i++)
        {
            double absYDiff = Math.Abs(list1[i].Y - list2[i].Y);
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
    /// Given a sequence of points, create a list of
    /// points from it that removes any that lie within
    /// an error margin of the original list.
    /// </summary>
    /// <param name="source">The original sequence</param>
    /// <param name="maxErr">The perpendicular error margin</param>
    /// <returns>A new reduced list of points</returns>

    public static IList<Coordinate> LinearReduction(IList<Coordinate> source, double maxErr)
    {
        List<Coordinate> result = new();
        int startIndex = 0;
        while (startIndex < source.Count - 1)
        {
            result.Add(source[startIndex]);
            startIndex = IndexOfFarthestPointWithinMargin(source, startIndex, maxErr);
        }
        result.Add(source.Last());
        return result;
    }

    /// <summary>
    /// Walk along a list of points, and find the index of the farthest point from
    /// the starting point for which all points between lie within maxErr
    /// perpendicular distance from a straight line from the starting point to
    /// the farthest point.
    /// </summary>
    /// <param name="source">The list of points to be searched</param>
    /// <param name="startIndex">Where in the list to start the search</param>
    /// <param name="maxErr">The tolerance to fit to</param>
    /// <returns>The index of the farthest point that meets the
    /// tolerance criteria for points in between</returns>

    private static int IndexOfFarthestPointWithinMargin
        (IList<Coordinate> source, int startIndex, double maxErr)
    {
        Coordinate earlier = source[startIndex];
        for (int i = startIndex + 2; i < source.Count; i++)
        {
            Coordinate later = source[i];
            for (int j = startIndex + 1; j < i; j++)
                if (PerpendicularDistance(source[j], earlier, later) > maxErr)
                    return i - 1;
        }
        return source.Count - 1;
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

    public static double PerpendicularDistance(Coordinate p0, Coordinate p1, Coordinate p2)
    {
        double numerator = (p2.Y - p1.Y) * p0.X;
        numerator -= (p2.X - p1.X) * p0.Y;
        numerator += p2.X * p1.Y - p2.Y * p1.X;
        numerator = Math.Abs(numerator);
        double denom = (p2 - p1).Magnitude;
        return numerator / denom;
    }

    /// <summary>
    /// Given two points, find the centres of the two
    /// circles of radius 'radius' that intersect at
    /// those two points. Used for working out the
    /// cutting path for a circular end mill of
    /// radius 'radius' that will be used to cut the
    /// gear profile.
    /// </summary>
    /// <param name="p1">The first intersecting point</param>
    /// <param name="p2">The second intersecting point</param>
    /// <param name="radius">The radius of the circles</param>
    /// <returns>The two possible centres for the circles</returns>

    public static Coordinate[] CircleCentres(Coordinate p1, Coordinate p2, double radius)
    {
        // Find the centre of the line from p1 to p2

        Coordinate midPoint = (p1 + p2).Scale(0.5);

        // Find the square of the linear distance
        // from one point to the midpoint

        double sqrDistToMidPoint =
            SumOfSquares(p2.X - p1.X, p2.Y - p1.Y) / 4;

        // Find the gradient of a line normal to 
        // the line between the two points

        double m = (p1.X - p2.X) / (p2.Y - p1.Y);

        // Calculate the distance along the line from the midpoint
        // to the centre of each circle, using Pythagoras on the
        // intersection - midpoint - circle-centre triangle

        double sqrDistanceToCentre = radius * radius - sqrDistToMidPoint;

        // Convert this distance to x and y components

        double dx = Math.Sqrt(sqrDistanceToCentre / (1 + m * m));
        double dy = m * dx;

        // Create and return the two circle centres

        return new Coordinate[]
        {
                new Coordinate(midPoint.X - dx, midPoint.Y - dy),
                new Coordinate(midPoint.X + dx, midPoint.Y + dy),
        };
    }

    /// <summary>
    /// Determine whether a point lies within a circle
    /// </summary>
    /// <param name="pt">The point being tested</param>
    /// <param name="centre">The coordinates of the circle centre</param>
    /// <param name="radius">The radius of the circle</param>
    /// <returns>True if the point is within the circle</returns>

    public static bool PointInCircle(Coordinate pt, Coordinate centre, double radius)
        => SumOfSquares(pt.X - centre.X, pt.Y - centre.Y) < Square(radius);

    /// <summary>
    /// The resolution of points on the various curves
    /// that are plotted as part of the gear profile
    /// generation. This figure is the number of points
    /// in the full rotation of the gear along a rack.
    /// The default value of 7200 allows for 20
    /// points per degree or one point for every
    /// three minutes of arc.
    /// </summary>

    public const int PointsPerRotation = 7200;

    /// <summary>
    /// The step size used when plotting points on a curve
    /// over a sequence of angles
    /// </summary>

    public static double AngleStep => 2 * Math.PI / PointsPerRotation;

    /// <summary>
    /// Implement the Newton Raphson iterative root finding algorithm
    /// </summary>
    /// <param name="func">The function whose value and gradient are computed
    /// as part of the process of finding the function's roots</param>
    /// <param name="xCurr">The starting value for the iterative
    /// root finder</param>
    /// <param name="xResolution">The accuracy at which we shall terminate
    /// the iteration</param>
    /// <returns>The value of the nearest root to the starting value</returns>

    public static double NewtonRaphson
        (Func<double, (double, double)> func, double xCurr, double xResolution)
    {
        double xPrev;
        do
        {
            xPrev = xCurr;
            (double fOfX, double dfByDx) = func(xCurr);
            xCurr -= fOfX / dfByDx;
        } while (Math.Abs(xCurr - xPrev) > xResolution);
        return xCurr;
    }

    /// <summary>
    /// Search between two values of X for the root of the
    /// equation func(X) = 0 using a binary search algorithm.
    /// The equation is assumed to have no inflexions between
    /// the lower and upper value of X
    /// </summary>
    /// <param name="func">The function we are finding
    /// roots for</param>
    /// <param name="lower">The lower bound on X</param>
    /// <param name="upper">The upper bound on X</param>
    /// <param name="delta">The resolution of the
    /// root finder</param>
    /// <returns>An approximation to the root with an
    /// allowed error margin of delta</returns>

    public static double RootBinarySearch
        (Func<double, double> func, double lower, double upper, double delta)
    {
        // Find the value of the function at the upper and lower bounds

        double fLower = func(lower);
        double fUpper = func(upper);
        if (Math.Sign(fLower) == Math.Sign(fUpper))
            throw new ArgumentException
                ("Root gradient search must have values either side of zero");
        int direction = fUpper >= 0 ? 1 : -1;
        double x = lower;
        for (double dx = (upper - lower) / 2; dx > delta / 2; dx /= 2)
        {
            double fx = direction * func(x);
            if (fx < 0)
                x += dx;
            else
                x -= dx;
        }
        return x;
    }
}
