using System.Drawing;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.Interfaces
{
    public interface IScreenCaptureHandler
    {
        Bitmap? CurrentScreen { get; }
        Bitmap? CurrentScreenMouseArea { get; }
        bool IsActive { get; set; }
        string MouseCoordinates { get; }
        string MouseCoordinatesScaled { get; }
        int OffsetX { get; }
        int OffsetY { get; }

        BitmapSource? ImageSourceFromScreenCapture();
        BitmapSource? ImageSourceFromScreenCaptureMouseArea();
    }
}
