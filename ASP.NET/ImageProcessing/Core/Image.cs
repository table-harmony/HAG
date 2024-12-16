namespace ImageProcessing.Core;

public class ImageHeader {
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public ColorFormat Format { get; protected set; }

    public override string ToString() =>
        $"Width: {Width}, Height: {Height}, Format: {Format}";
}

public enum ColorFormat {
    RGB,
    RGBA,
}

public class ImageBody {
    public IEnumerable<Pixel> Pixels { get; protected set; } = [];
}