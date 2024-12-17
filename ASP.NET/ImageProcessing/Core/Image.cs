namespace ImageProcessing.Core;

public class ImageHeader {
    public int Width { get; set; }
    public int Height { get; set; }
    public ColorFormat Format { get; set; }

    public override string ToString() =>
        $"Width: {Width}, Height: {Height}, Format: {Format}";
}

public enum ColorFormat {
    RGB,
    RGBA,
}

public class ImageBody {
    public IEnumerable<Pixel> Pixels { get; set; } = [];
}