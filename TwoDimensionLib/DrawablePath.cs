namespace TwoDimensionLib
{
    public class DrawablePath : IDrawable
    {
        public IList<IDrawable> Curves { get; set; } = new List<IDrawable>();
        public bool Closed { get; set; }

        public Rectangle Bounds
        {
            get
            {
                BoundsTracker b = new();
                foreach (IDrawable d in Curves)
                    b.Track(d.Bounds);
                return b.Bounds;
            }
        }

        public Coordinate Start => Curves.Any() ? Curves[0].Start : Coordinate.Empty;

        public Coordinate End => Curves.Any() ? Curves[^1].End : Coordinate.Empty;

        public IDrawable ReflectY()
            => new DrawablePath
            {
                Curves = new List<IDrawable>
                    (this.Curves.Select(c => c.ReflectY())),
                Closed = this.Closed
            };

        public IDrawable Reversed()
            => new DrawablePath
            {
                Curves = new List<IDrawable>
                    (this.Curves.Reverse().Select(c => c.Reversed())),
                Closed = this.Closed
            };

        public IDrawable RotatedBy(double phi, Coordinate pivot)
            => new DrawablePath
            {
                Curves = new List<IDrawable>
                    (this.Curves.Select(c => c.RotatedBy(phi, pivot))),
                Closed = this.Closed
            };

        public IDrawable Translated(Coordinate offset)
            => new DrawablePath
            {
                Curves = new List<IDrawable>
                    (this.Curves.Select(c => c.Translated(offset))),
                Closed = this.Closed
            };
    }
}
