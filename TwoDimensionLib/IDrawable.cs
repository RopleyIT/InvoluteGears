namespace TwoDimensionLib
{
    public interface IDrawable
    {
        IDrawable Translated(Coordinate offset);
        IDrawable RotatedBy(double phi, Coordinate pivot);
        IDrawable Reversed();
        IDrawable ReflectY();
        Coordinate Start { get; }
        Coordinate End { get; }
        Rectangle Bounds { get; }
    }
}
