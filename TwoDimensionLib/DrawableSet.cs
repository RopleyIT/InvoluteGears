namespace TwoDimensionLib
{
    /// <summary>
    /// Manage a set of drawable paths as an IDrawable
    /// so that rotation, translation etc. can be
    /// applied to the whole set
    /// </summary>

    public class DrawableSet : IDrawable
    {
        public DrawableSet()
        {
            Paths = new List<DrawablePath>();
        }

        public DrawableSet Merge(DrawableSet other)
            => Merge(other.Paths);

        public DrawableSet Merge(IList<DrawablePath> paths)
        {
            DrawableSet ds = new DrawableSet
            {
                Paths = this.Paths
            };
            Paths.AddRange(paths);
            return ds;
        }

        public IList<DrawablePath> Paths { get; set; }

        public Coordinate Start => Paths.Count == 0 ? Coordinate.Empty : Paths[0].Start;

        public Coordinate End => Paths.Count == 0 ? Coordinate.Empty : Paths[^1].End;

        public Rectangle Bounds
        {
            get
            {
                BoundsTracker tracker = new BoundsTracker();
                foreach (var path in Paths)
                {
                    tracker.Track(path.Bounds);
                }
                return tracker.Bounds;
            }
        }

        public IDrawable ReflectY()
        {
            var newSet = new DrawableSet();
            newSet.Paths.AddRange
                (Paths.Select(p => (p.ReflectY() as DrawablePath)));
            return newSet;
        }

        public IDrawable Reversed()
        {
            var newSet = new DrawableSet();
            newSet.Paths.AddRange
                (Paths.Select(p => (p.Reversed() as DrawablePath)));
            return newSet;
        }

        public IDrawable RotatedBy(double phi, Coordinate pivot)
        {
            var newSet = new DrawableSet();
            newSet.Paths.AddRange
                (Paths.Select(p => (p.RotatedBy(phi, pivot) as DrawablePath)));
            return newSet;
        }

        public IDrawable Translated(Coordinate offset)
        {
            var newSet = new DrawableSet();
            newSet.Paths.AddRange
                (Paths.Select(p => (p.Translated(offset) as DrawablePath)));
            return newSet;
        }
    }
}
