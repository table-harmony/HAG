using System.Xml.Linq;

namespace ImageProcessing.Core;

public class Pixel {
    public int Red { get; set; } = 0;
    public int Green { get; set; } = 0;
    public int Blue { get; set; } = 0;
    public int Alpha { get; set; } = 0;

    public override bool Equals(object? obj) {
        if (obj is not Pixel other)
            return false;

        return Red == other.Red &&
               Green == other.Green &&
               Blue == other.Blue &&
               Alpha == other.Alpha;
    }

    public static bool operator ==(Pixel? a, Pixel? b) {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        if (ReferenceEquals(a, b)) return true;
        return a.Equals(b);
    }

    public static bool operator !=(Pixel? a, Pixel? b) {
        return !(a == b);
    }

    public override int GetHashCode() {
        return Red * 3 + Green * 5 + Blue * 7 + Alpha * 11;
    }

    public int GetCode() {
        return GetHashCode();
    }

    public static Pixel operator -(Pixel? a, Pixel? b) {
        if (a is null && b is null)
            return new Pixel();

        if (a is null && b is not null)
            return b;

        if (a is not null && b is null)
            return a;

        return new Pixel {
            Red = a.Red - b.Red,
            Green = a.Green - b.Green,
            Blue = a.Blue - b.Blue,
            Alpha = a.Alpha - b.Alpha
        };
    }

    public override string ToString() {
        return $"Red: {Red}, Green: {Green}, Blue: {Blue}, Alpha: {Alpha}";
    }
}
