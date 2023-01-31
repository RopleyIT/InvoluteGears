namespace TwoDimensionLib
{
    /// <summary>
    /// Manage a set of drawable paths as an IDrawable
    /// so that rotation, translation etc. can be
    /// applied to the whole set
    /// </summary>

    public class DrawableSet : IDrawable
    {
        public DrawableSet Merge(DrawableSet other)
            => Merge(other.Paths);

        public DrawableSet Merge(IList<DrawablePath> paths)
        {
            DrawableSet ds = new()
            {
                Paths = this.Paths
            };
            Paths.AddRange(paths);
            return ds;
        }

        public IList<DrawablePath> Paths { get; set; } = new List<DrawablePath>();

        public Coordinate Start => Paths.Any() ? Paths[0].Start : Coordinate.Empty;

        public Coordinate End => Paths.Any() ? Paths[^1].End : Coordinate.Empty;

        public Rectangle Bounds
        {
            get
            {
                BoundsTracker tracker = new();
                foreach (var path in Paths)
                    tracker.Track(path.Bounds);
                return tracker.Bounds;
            }
        }

        public IDrawable ReflectY()
        {
            var newSet = new DrawableSet();
            newSet.Paths.AddRange
                (Paths.Select(p => (DrawablePath)p.ReflectY()));
            return newSet;
        }

        public IDrawable Reversed()
        {
            var newSet = new DrawableSet();
            newSet.Paths.AddRange
                (Paths.Select(p => (DrawablePath)p.Reversed()));
            return newSet;
        }

        public IDrawable RotatedBy(double phi, Coordinate pivot)
        {
            var newSet = new DrawableSet();
            newSet.Paths.AddRange
                (Paths.Select(p => (DrawablePath)p.RotatedBy(phi, pivot)));
            return newSet;
        }

        public IDrawable Translated(Coordinate offset)
        {
            var newSet = new DrawableSet();
            newSet.Paths.AddRange
                (Paths.Select(p => (DrawablePath)p.Translated(offset)));
            return newSet;
        }
    }
}
