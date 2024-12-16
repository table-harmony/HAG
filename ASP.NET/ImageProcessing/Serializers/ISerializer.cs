using ImageProcessing.Core;

namespace ImageProcessing.Serializers;

public interface ISerializer<T> {
    /// <summary>
    /// Converts a specific image format to a standardized SerializedImage
    /// </summary>
    Sif Deserialize(T source);

    /// <summary>
    /// Converts a SerializedImage back to a specific image format
    /// </summary>
    T Serialize(Sif source);
}
