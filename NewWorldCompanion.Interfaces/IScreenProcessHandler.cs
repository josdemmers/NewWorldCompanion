using System.Drawing;

namespace NewWorldCompanion.Interfaces
{
    public interface IScreenProcessHandler
    {
        int AreaLower { get; set; }
        int AreaUpper { get; set; }
        int HysteresisLower { get; set; }
        int HysteresisUpper { get; set; }
        int ThresholdMin { get; set; }
        int ThresholdMax { get; set; }

        Bitmap? ProcessedImage { get; }
        Bitmap? RoiImage { get; }
        Bitmap? OcrImage { get; }
    }
}
