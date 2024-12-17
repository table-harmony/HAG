using ImageProcessing.Services;
using ImageProcessing.Serializers;

ImageConverter converter = new();

converter.Convert(
    "C:\\Users\\liron\\Downloads\\1531344.png",
    "C:\\Users\\liron\\Downloads\\output.hag",
    SupportedImageFormats.Png,
    SupportedImageFormats.Hag
);

converter.Convert(
    "C:\\Users\\liron\\Downloads\\output.hag",
    "C:\\Users\\liron\\Downloads\\output2.png",
    SupportedImageFormats.Hag,
    SupportedImageFormats.Png
);