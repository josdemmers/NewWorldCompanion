using System.Drawing;

namespace NewWorldCompanion.Interfaces
{
    public interface IScreenProcessHandler
    {
        int AreaLower { get; }
        int AreaUpper { get; }
        int HysteresisLower { get; }
        int HysteresisUpper { get; }
        int ThresholdMin { get; }
        int ThresholdMax { get; }

        Bitmap? ProcessedImage { get; }
        Bitmap? RoiImage { get; }
        Bitmap? OcrImage { get; }
    }
}
