namespace Server.x.Entities;
public class Header
{
    public int Width { get; set; }
    public int Height { get; set; }

    public override string ToString()
    {
        return $"Width = {Width}, Height = {Height}";
    }
}
