using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter;

/// <summary>
/// A filter class that spies on a sequence of PointF points,
/// and computes the bounding rectangle around them.
/// </summary>

public class BoundsF
{
    private PointF topLeft = new(float.MaxValue, float.MaxValue);
    private PointF bottomRight = new(float.MinValue, float.MinValue);

    /// <summary>
    /// As points pass through this IEnumerable filter, keep track
    /// of the coordinates of the top left corner and the bottom
    /// right corner of the bounding box around them. Note that
    /// the enumerable is restartable, inasmuch as the same
    /// BoundF object can be used for several sequences of points
    /// so that we find the bounding box around the whole set.
    /// </summary>
    /// <param name="points">The stream of points for which
    /// we want to find the bounding box</param>
    /// <returns>The same enumerable of points as was passed
    /// in as an argument</returns>

    public IEnumerable<PointF> Track(IEnumerable<PointF> points)
    {
        foreach (PointF p in points)
        {
            topLeft.X = Math.Min(p.X, topLeft.X);
            topLeft.Y = Math.Min(p.Y, topLeft.Y);
            bottomRight.X = Math.Max(p.X, bottomRight.X);
            bottomRight.Y = Math.Max(p.Y, bottomRight.Y);
            yield return p;
        }
    }

    /// <summary>
    /// Generate the calculated bounding box around the points
    /// </summary>

    public RectangleF Bounds => new(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
}
