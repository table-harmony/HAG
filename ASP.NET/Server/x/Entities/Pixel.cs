using System.Xml.Linq;

namespace Server.x.Entities;

public class Pixel
{
    public int Red { get; set; } = 0;
    public int Green { get; set; } = 0;
    public int Blue { get; set; } = 0;
    public int Alpha { get; set; } = 0;

    public override bool Equals(object? obj)
    {
        if (obj is not Pixel other)
            return false;

        return Red == other.Red &&
               Green == other.Green &&
               Blue == other.Blue &&
               Alpha == other.Alpha;
    }

    public static bool operator ==(Pixel a, Pixel b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (ReferenceEquals(a, b)) return true;
        return a.Equals(b);
    }

    public static bool operator !=(Pixel a, Pixel b)
    {
        return !(a == b);
    }

    public int GetCode()
    {
        return Red * 3 + Green * 5 + Blue * 7;
    }

    public static Pixel operator -(Pixel a, Pixel b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return new Pixel
        {
            Red = a.Red - b.Red,
            Green = a.Green - b.Green,
            Blue = a.Blue - b.Blue,
            Alpha = a.Alpha - b.Alpha
        };
    }

    public override string ToString()
    {
        return $"Red: {Red}, Green: {Green}, Blue: {Blue}, Alpha: {Alpha}";
    }
}
