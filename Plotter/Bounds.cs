using System;
using System.Collections.Generic;
using TwoDimensionLib;

namespace Plotter
{
    public class BoundsTracker
    {
        private double left = double.MaxValue;
        private double top = double.MaxValue;
        private double right = double.MinValue;
        private double bottom = double.MinValue;

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

        public IEnumerable<Coordinate> Track(IEnumerable<Coordinate> points)
        {
            foreach (Coordinate p in points)
            {
                left = Math.Min(p.X, left);
                top = Math.Min(p.Y, top);
                right = Math.Max(p.X, right);
                bottom = Math.Max(p.Y, bottom);
                yield return p;
            }
        }

        /// <summary>
        /// Generate the calculated bounding box around the points
        /// </summary>

        public Rectangle Bounds => new(new(top, left), right - left, bottom - top);
    }
}
