using System.Drawing;
using System.Windows.Media.Imaging;

namespace NewWorldCompanion.Interfaces
{
    public interface IScreenCaptureHandler
    {
        Bitmap? CurrentScreen
        {
            get;
        }

        bool IsActive { get; set; }

        public string MouseCoordinates
        {
            get;
        }

        public string MouseCoordinatesScaled
        {
            get;
        }

        BitmapSource? ImageSourceFromScreenCapture();
    }
}
