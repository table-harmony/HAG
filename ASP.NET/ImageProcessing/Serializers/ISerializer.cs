using ImageProcessing.Core;

namespace ImageProcessing.Serializers;

public interface ISerializer {
    /// <summary>
    /// Converts a specific image format to a standardized SerializedImage
    /// </summary>
    Sif Serialize(Stream source);

    /// <summary>
    /// Converts a SerializedImage back to a specific image format
    /// </summary>
    Stream Deserialize(Sif source);
}
