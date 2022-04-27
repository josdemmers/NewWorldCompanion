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
        Bitmap? OcrImageCount { get; }
        Bitmap? OcrImageCountRaw { get; }

        int OverlayX { get; }
        int OverlayY { get; }
        int OverlayWidth { get; }
        int OverlayHeigth { get; }

        void ProcessImageCountOCRDebug(int minR, int minG, int minB, int maxR, int maxG, int maxB);
    }
}
