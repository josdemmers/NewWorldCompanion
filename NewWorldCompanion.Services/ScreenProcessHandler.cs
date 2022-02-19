using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using NewWorldCompanion.Constants;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class ScreenProcessHandler : IScreenProcessHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScreenCaptureHandler _screenCaptureHandler;

        private int _areaLower;
        private int _areaUpper;
        private int _hysteresisLower;
        private int _hysteresisUpper;
        private int _thresholdMin;
        private int _thresholdMax;
        private bool _isBusy = false;
        private Bitmap? _capturedImage = null;
        private Bitmap? _processedImage = null;
        private Bitmap? _roiImage = null;

        // Start of Constructor region

        #region Constructor

        public ScreenProcessHandler(IEventAggregator eventAggregator, IScreenCaptureHandler screenCaptureHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ScreenCaptureReadyEvent>().Subscribe(HandleScreenCaptureReadyEvent);

            // Init services
            _screenCaptureHandler = screenCaptureHandler;

            // Restore defaults
            AreaLower = EmguConstants.AreaLower;
            AreaUpper = EmguConstants.AreaUpper;
            HysteresisLower = EmguConstants.HysteresisLower;
            HysteresisUpper = EmguConstants.HysteresisUpper;
            ThresholdMin = EmguConstants.ThresholdMin;
            ThresholdMax = EmguConstants.ThresholdMax;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public int AreaLower { get => _areaLower; set => _areaLower = value; }
        public int AreaUpper { get => _areaUpper; set => _areaUpper = value; }
        public int HysteresisLower { get => _hysteresisLower; set => _hysteresisLower = value; }
        public int HysteresisUpper { get => _hysteresisUpper; set => _hysteresisUpper = value; }
        public int ThresholdMin { get => _thresholdMin; set => _thresholdMin = value; }
        public int ThresholdMax { get => _thresholdMax; set => _thresholdMax = value; }
        public Bitmap? ProcessedImage { get => _processedImage; set => _processedImage = value; }
        public Bitmap? RoiImage { get => _roiImage; set => _roiImage = value; }
        public Bitmap? OcrImage { get => _roiImage; set => _roiImage = value; }

        #endregion

        // Start of Events region

        #region Events

        private void HandleScreenCaptureReadyEvent()
        {
            if (!_isBusy)
            {
                _isBusy = true;
                _capturedImage = _screenCaptureHandler.CurrentScreen;
                Image<Bgr, byte> imageCV = _capturedImage.ToImage<Bgr, byte>();

                ProcessImage(imageCV.Mat);

                _isBusy = false;
            }
        }

        #endregion

        // Start of Methods region
        #region Methods

        private void ProcessImage(Mat img)
        {
            UMat filter = new UMat();
            UMat cannyEdges = new UMat();
            Mat crop = new Mat(img.Size, DepthType.Cv8U, 3);

            //Convert the image to grayscale and filter out the noise
            CvInvoke.CvtColor(img, filter, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(filter, filter, new System.Drawing.Size(3, 3), 1);

            // Canny and edge detection
            double cannyThreshold = HysteresisLower;
            double cannyThresholdLinking = HysteresisUpper;
            CvInvoke.Canny(filter, cannyEdges, cannyThreshold, cannyThresholdLinking);

            // Find rectangles
            List<RotatedRect> rectangleList = new List<RotatedRect>();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            int count = contours.Size;
            for (int i = 0; i < count; i++)
            {
                using (VectorOfPoint contour = contours[i])
                using (VectorOfPoint approxContour = new VectorOfPoint())
                {
                    CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                    // Only consider contours with area greater than 250
                    // Equipment icon size: ca. 5000-6000
                    // Schematic icon size: ca. 10000-10800
                    if (CvInvoke.ContourArea(approxContour, false) > AreaLower &&
                        CvInvoke.ContourArea(approxContour, false) < AreaUpper)
                    {
                        // The contour has 4 vertices.
                        if (approxContour.Size == 4)
                        {
                            // Determine if all the angles in the contour are within [80, 100] degree
                            bool isRectangle = true;
                            System.Drawing.Point[] pts = approxContour.ToArray();
                            LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);

                            for (int j = 0; j < edges.Length; j++)
                            {
                                double angle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                if (angle < 80 || angle > 100)
                                {
                                    isRectangle = false;
                                    break;
                                }
                            }

                            if (isRectangle)
                            {
                                var rotatedRec = CvInvoke.MinAreaRect(approxContour);
                                if (!rectangleList.Exists(rec =>
                                ((rec.Center.X - rotatedRec.Center.X > -2 && rec.Center.X - rotatedRec.Center.X < 2) || (rotatedRec.Center.X - rec.Center.X > -2 && rotatedRec.Center.X - rec.Center.X < 2)) &&
                                ((rec.Center.Y - rotatedRec.Center.Y > -2 && rec.Center.Y - rotatedRec.Center.Y < 2) || (rotatedRec.Center.Y - rec.Center.Y > -2 && rotatedRec.Center.Y - rec.Center.Y < 2))))
                                {
                                    rectangleList.Add(rotatedRec);
                                }
                            }
                        }
                    }
                }
            }

            // Draw rectangles
            foreach (RotatedRect rectangle in rectangleList)
            {
                CvInvoke.Polylines(img, Array.ConvertAll(rectangle.GetVertices(), System.Drawing.Point.Round), true,
                    new Bgr(Color.DarkOrange).MCvScalar, 2);
            }
            ProcessedImage = img.ToBitmap();
            _eventAggregator.GetEvent<ProcessedImageReadyEvent>().Publish();

            // Create roi
            if (rectangleList.Count == 1)
            {
                var roiRectangle = new Rectangle
                (
                  rectangleList[0].MinAreaRect().X + rectangleList[0].MinAreaRect().Width + 5,
                  rectangleList[0].MinAreaRect().Y,
                  (int)(rectangleList[0].MinAreaRect().Width * 3.25),
                  rectangleList[0].MinAreaRect().Height - 25
                );

                try
                {
                    crop = new Mat(img, roiRectangle);
                    RoiImage = crop.ToBitmap();
                    _eventAggregator.GetEvent<RoiImageReadyEvent>().Publish();
                    ProcessImageOCR(crop);
                }
                catch (Exception) { }
            }
        }

        private void ProcessImageOCR(Mat img)
        {
            try
            {
                Mat imgFilter = new Mat(img.Size, DepthType.Cv8U, 3);

                // Convert the image to grayscale
                CvInvoke.CvtColor(img, imgFilter, ColorConversion.Bgr2Gray);

                // Apply threshold
                //CvInvoke.Threshold(imgFilter, imgFilter, 0, 255, ThresholdType.Otsu);
                //CvInvoke.Threshold(imgFilter, imgFilter, ThresholdMin, ThresholdMax, ThresholdType.Binary);
                CvInvoke.Threshold(imgFilter, imgFilter, ThresholdMin, ThresholdMax, ThresholdType.BinaryInv);

                // Thinning and Skeletonization
                //Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(2, 2), new System.Drawing.Point(-1, -1));
                //CvInvoke.Erode(imgFilter, imgFilter, element, new System.Drawing.Point(-1, -1), 1, BorderType.Constant, new MCvScalar(255, 255, 255));

                // Filter out the noise
                //CvInvoke.GaussianBlur(imgFilter, imgFilter, new System.Drawing.Size(0, 0), 1, 0, BorderType.Default);

                if (!Directory.Exists(@"ocrimages\"))
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(@"ocrimages\");
                }

                OcrImage = imgFilter.ToBitmap();
                imgFilter.Save(@"ocrimages\latest.png");
                _eventAggregator.GetEvent<OcrImageReadyEvent>().Publish();
            }
            catch (Exception) { }
        }

        #endregion
    }
}
