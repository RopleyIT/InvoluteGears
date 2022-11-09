using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib
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
            foreach (Coordinate c in points)
                Track(c);
            return points;
        }

        public Coordinate Track(Coordinate p)
        {
            if (double.IsNaN(p.X) || double.IsNaN(p.Y))
                throw new NotFiniteNumberException("NaN in bounds tracker");
            left = Math.Min(p.X, left);
            top = Math.Min(p.Y, top);
            right = Math.Max(p.X, right);
            bottom = Math.Max(p.Y, bottom);
            return p;
        }

        /// <summary>
        /// Grow the bounds to include the area from another
        /// rectangle. Typically this would be used to find
        /// the bounds of a whole set of graphical items.
        /// </summary>
        /// <param name="r">The rectangle to incorporate
        /// into the overall bounds</param>
        
        public void Track(Rectangle r)
        {
            Track(r.Location);
            Track(new Coordinate(r.Right, r.Bottom));
        }

        /// <summary>
        /// Generate the calculated bounding box around the points
        /// </summary>

        public Rectangle Bounds => new(new(top, left), right - left, bottom - top);
    }
}
