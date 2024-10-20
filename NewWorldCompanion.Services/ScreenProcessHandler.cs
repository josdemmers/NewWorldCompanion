﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Extensions.Logging;
using NewWorldCompanion.Constants;
using NewWorldCompanion.Events;
using NewWorldCompanion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Services
{
    public class ScreenProcessHandler : IScreenProcessHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        private readonly ISettingsManager _settingsManager;
        private readonly IScreenCaptureHandler _screenCaptureHandler;

        private readonly object ocrDebugLock = new object();

        private bool _isBusy = false;
        private Bitmap? _capturedImage = null;
        private Bitmap? _capturedImageCount = null;
        private Bitmap? _processedImage = null;
        private Bitmap? _roiImage = null;
        private Bitmap? _ocrImage = null;
        private Bitmap? _ocrImageCount = null;
        private Bitmap? _ocrImageCountRaw = null;

        private int _overlayX = 0;
        private int _overlayY = 0;
        private int _overlayWidth = 0;
        private int _overlayHeigth = 0;

        // Start of Constructor region

        #region Constructor

        public ScreenProcessHandler(IEventAggregator eventAggregator, ILogger<ScreenProcessHandler> logger, ISettingsManager settingsManager, IScreenCaptureHandler screenCaptureHandler)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ScreenCaptureReadyEvent>().Subscribe(HandleScreenCaptureReadyEvent);

            // Init logger
            _logger = logger;

            // Init services
            _settingsManager = settingsManager;
            _screenCaptureHandler = screenCaptureHandler;
        }

        #endregion

        // Start of Properties region

        #region Properties

        public int AreaLower { get => _settingsManager.Settings.EmguAreaLower; }
        public int AreaUpper { get => _settingsManager.Settings.EmguAreaUpper; }
        public int HysteresisLower { get => _settingsManager.Settings.EmguHysteresisLower; }
        public int HysteresisUpper { get => _settingsManager.Settings.EmguHysteresisUpper; }
        public int ThresholdMin { get => _settingsManager.Settings.EmguThresholdMin; }
        public int ThresholdMax { get => _settingsManager.Settings.EmguThresholdMax; }
        public int ThresholdOcrMaxR { get => _settingsManager.Settings.EmguThresholdMaxR; }
        public int ThresholdOcrMaxG { get => _settingsManager.Settings.EmguThresholdMaxG; }
        public int ThresholdOcrMaxB { get => _settingsManager.Settings.EmguThresholdMaxB; }
        public Bitmap? ProcessedImage { get => _processedImage; set => _processedImage = value; }
        public Bitmap? RoiImage { get => _roiImage; set => _roiImage = value; }
        //public Bitmap? OcrImage { get => _roiImage; set => _roiImage = value; }
        public Bitmap? OcrImage { get => _ocrImage; set => _ocrImage = value; }
        public Bitmap? OcrImageCount { get => _ocrImageCount; set => _ocrImageCount = value; }
        public Bitmap? OcrImageCountRaw { get => _ocrImageCountRaw; set => _ocrImageCountRaw = value; }
        public int OverlayX { get => _overlayX; set => _overlayX = value; }
        public int OverlayY { get => _overlayY; set => _overlayY = value; }
        public int OverlayWidth { get => _overlayWidth; set => _overlayWidth = value; }
        public int OverlayHeigth { get => _overlayHeigth; set => _overlayHeigth = value; }

        #endregion

        // Start of Events region

        #region Events

        private void HandleScreenCaptureReadyEvent()
        {
            if (!_isBusy)
            {
                _isBusy = true;
                _capturedImage = _screenCaptureHandler.CurrentScreen;
                _capturedImageCount = _screenCaptureHandler.CurrentScreenMouseArea;
                Image<Bgr, byte> imageCV = _capturedImage.ToImage<Bgr, byte>();
                Image<Bgr, byte> imageCountCV = _capturedImageCount.ToImage<Bgr, byte>();

                ProcessImageCount(imageCountCV.Mat);
                ProcessImage(imageCV.Mat);

                _isBusy = false;
            }
        }

        #endregion

        // Start of Methods region
        #region Methods

        private void ProcessImage(Mat img)
        {
            bool result = ProcessImageNormal(img);
            if (!result) 
            {
                result = ProcessImageSwirling(img);
            }
            if (!result)
            {
                _eventAggregator.GetEvent<OverlayHideEvent>().Publish();
            }
        }

        private bool ProcessImageNormal(Mat img)
        {
            bool result = true;

            UMat filter = new UMat();
            UMat cannyEdges = new UMat();
            Mat crop = new Mat(img.Size, DepthType.Cv8U, 3);

            //Convert the image to grayscale and filter out the noise
            CvInvoke.CvtColor(img, filter, ColorConversion.Bgr2Gray);
            //CvInvoke.GaussianBlur(filter, filter, new System.Drawing.Size(3, 3), 1);

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
                    // Only consider contours with area between upper and lower values
                    // e.g. when using 2560x1440
                    // Equipment icon size: ca. 5000-6000
                    // Schematic icon size: ca. 10000-10800
                    if (CvInvoke.ContourArea(approxContour, false) > AreaLower &&
                        CvInvoke.ContourArea(approxContour, false) < AreaUpper)
                    {
                        // The contour has 4 vertices.
                        if (approxContour.Size >= 2)
                        {
                            // Determine if all the angles in the contour are within [80, 100] degree
                            bool isRectangle = true;
                            System.Drawing.Point[] pts = approxContour.ToArray();
                            LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);

                            int angleCounter = 0;
                            for (int j = 0; j < edges.Length; j++)
                            {
                                double angle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                
                                if (angle > 80 && angle < 100)
                                {
                                    angleCounter++;
                                }
                            }
                            isRectangle = angleCounter > 1;

                            if (isRectangle)
                            {
                                var rotatedRec = CvInvoke.MinAreaRect(approxContour);
                                if (!rectangleList.Exists(rec =>
                                ((rec.Center.X - rotatedRec.Center.X > -2 && rec.Center.X - rotatedRec.Center.X < 2) || (rotatedRec.Center.X - rec.Center.X > -2 && rotatedRec.Center.X - rec.Center.X < 2)) &&
                                ((rec.Center.Y - rotatedRec.Center.Y > -2 && rec.Center.Y - rotatedRec.Center.Y < 2) || (rotatedRec.Center.Y - rec.Center.Y > -2 && rotatedRec.Center.Y - rec.Center.Y < 2))))
                                {
                                    // We only want squares
                                    float ratio = rotatedRec.Size.Width / rotatedRec.Size.Height;
                                    if (ratio > 0.8 && ratio < 1.2)
                                    {
                                        rectangleList.Add(rotatedRec);
                                    }
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
                  (int)(rectangleList[0].MinAreaRect().Width * 2.8),
                  (int)(rectangleList[0].MinAreaRect().Height * 0.5)
                );

                // Update overlay position
                // Note: Using a constant height of 100. So that all text rows fit the overlay for every resolution.
                OverlayX = _screenCaptureHandler.OffsetX + rectangleList[0].MinAreaRect().X;
                //OverlayY = _screenCaptureHandler.OffsetY + rectangleList[0].MinAreaRect().Y - (rectangleList[0].MinAreaRect().Height + 12);
                OverlayY = _screenCaptureHandler.OffsetY + rectangleList[0].MinAreaRect().Y - (100 + 12);
                OverlayWidth = rectangleList[0].MinAreaRect().Width + (int)(rectangleList[0].MinAreaRect().Width * 2.8);
                //OverlayHeigth = rectangleList[0].MinAreaRect().Height;
                OverlayHeigth = 100;

                try
                {
                    // Validate width
                    roiRectangle.Width = (roiRectangle.X + roiRectangle.Width) <= img.Width ? roiRectangle.Width : roiRectangle.Width - (roiRectangle.X + roiRectangle.Width - img.Width);

                    if (roiRectangle.Width > 0 && roiRectangle.Y > 0)
                    {
                        crop = new Mat(img, roiRectangle);
                        RoiImage = crop.ToBitmap();
                        _eventAggregator.GetEvent<RoiImageReadyEvent>().Publish();
                        ProcessImageOCR(crop);
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<OverlayHideEvent>().Publish();
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);

                    //_eventAggregator.GetEvent<OverlayHideEvent>().Publish();
                    result = false;
                }
            }
            else
            {
                //_eventAggregator.GetEvent<OverlayHideEvent>().Publish();
                result = false;
            }
            return result;
        }

        private bool ProcessImageSwirling(Mat img)
        {
            bool result = true;

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
                    // Only consider contours with area between upper and lower values
                    // e.g. when using 2560x1440
                    // Equipment icon size: ca. 5000-6000
                    // Schematic icon size: ca. 10000-10800
                    if (CvInvoke.ContourArea(approxContour, false) > AreaLower &&
                        CvInvoke.ContourArea(approxContour, false) < AreaUpper)
                    {
                        // The contour has 4 vertices.
                        if (approxContour.Size >= 2)
                        {
                            // Determine if all the angles in the contour are within [80, 100] degree
                            bool isRectangle = true;
                            System.Drawing.Point[] pts = approxContour.ToArray();
                            LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);

                            int angleCounter = 0;
                            for (int j = 0; j < edges.Length; j++)
                            {
                                double angle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));

                                if (angle > 80 && angle < 100)
                                {
                                    angleCounter++;
                                }
                            }
                            isRectangle = angleCounter > 1;

                            if (isRectangle)
                            {
                                var rotatedRec = CvInvoke.MinAreaRect(approxContour);
                                if (!rectangleList.Exists(rec =>
                                ((rec.Center.X - rotatedRec.Center.X > -2 && rec.Center.X - rotatedRec.Center.X < 2) || (rotatedRec.Center.X - rec.Center.X > -2 && rotatedRec.Center.X - rec.Center.X < 2)) &&
                                ((rec.Center.Y - rotatedRec.Center.Y > -2 && rec.Center.Y - rotatedRec.Center.Y < 2) || (rotatedRec.Center.Y - rec.Center.Y > -2 && rotatedRec.Center.Y - rec.Center.Y < 2))))
                                {
                                    // We only want squares
                                    float ratio = rotatedRec.Size.Width / rotatedRec.Size.Height;
                                    if (ratio > 0.8 && ratio < 1.2)
                                    {
                                        rectangleList.Add(rotatedRec);
                                    }
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
                  (int)(rectangleList[0].MinAreaRect().Width * 2.8),
                  (int)(rectangleList[0].MinAreaRect().Height * 0.5)
                );

                // Update overlay position
                // Note: Using a constant height of 100. So that all text rows fit the overlay for every resolution.
                OverlayX = _screenCaptureHandler.OffsetX + rectangleList[0].MinAreaRect().X;
                //OverlayY = _screenCaptureHandler.OffsetY + rectangleList[0].MinAreaRect().Y - (rectangleList[0].MinAreaRect().Height + 12);
                OverlayY = _screenCaptureHandler.OffsetY + rectangleList[0].MinAreaRect().Y - (100 + 12);
                OverlayWidth = rectangleList[0].MinAreaRect().Width + (int)(rectangleList[0].MinAreaRect().Width * 2.8);
                //OverlayHeigth = rectangleList[0].MinAreaRect().Height;
                OverlayHeigth = 100;

                try
                {
                    // Validate width
                    roiRectangle.Width = (roiRectangle.X + roiRectangle.Width) <= img.Width ? roiRectangle.Width : roiRectangle.Width - (roiRectangle.X + roiRectangle.Width - img.Width);

                    if (roiRectangle.Width > 0 && roiRectangle.Y > 0)
                    {
                        crop = new Mat(img, roiRectangle);
                        RoiImage = crop.ToBitmap();
                        _eventAggregator.GetEvent<RoiImageReadyEvent>().Publish();
                        ProcessImageOCR(crop);
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<OverlayHideEvent>().Publish();
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);

                    //_eventAggregator.GetEvent<OverlayHideEvent>().Publish();
                    result = false;
                }
            }
            else
            {
                //_eventAggregator.GetEvent<OverlayHideEvent>().Publish();
                result = false;
            }
            return result;
        }

        private void ProcessImageCount(Mat img)
        {
            ProcessImageCountOCR(img);
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
                imgFilter.Save(@"ocrimages\itemname.png");
                _eventAggregator.GetEvent<OcrImageReadyEvent>().Publish();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
            }
        }

        private void ProcessImageCountOCR(Mat img)
        {
            try
            {
                Mat imgFilter = new Mat(img.Size, DepthType.Cv8U, 3);

                CvInvoke.InRange(img, new ScalarArray(new MCvScalar(0, 0, 0)), new ScalarArray(new MCvScalar(ThresholdOcrMaxR, ThresholdOcrMaxG, ThresholdOcrMaxB)), imgFilter);

                if (!Directory.Exists(@"ocrimages\"))
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(@"ocrimages\");
                }

                OcrImageCountRaw = img.ToBitmap();
                OcrImageCount = imgFilter.ToBitmap();
                img.Save(@"ocrimages\itemcountraw.png");
                imgFilter.Save(@"ocrimages\itemcount.png");
                _eventAggregator.GetEvent<OcrImageCountReadyEvent>().Publish();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
            }
        }

        public void ProcessImageCountOCRDebug(int minR, int minG, int minB, int maxR, int maxG, int maxB)
        {
            //https://stackoverflow.com/questions/26218280/thresholding-rgb-image-in-opencv
            //https://stackoverflow.com/questions/16407267/how-to-use-cvinrange-in-emgu-cv


            lock (ocrDebugLock)
            {
                try
                {
                    var img = new Mat(@"ocrimages\itemcountdebug.png");
                    Mat imgFilter = new Mat(img.Size, DepthType.Cv8U, 3);

                    CvInvoke.InRange(img, new ScalarArray(new MCvScalar(minR, minG, minB)), new ScalarArray(new MCvScalar(maxR, maxG, maxB)), imgFilter);

                    if (!Directory.Exists(@"ocrimages\"))
                    {
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(@"ocrimages\");
                    }

                    OcrImageCountRaw = img.ToBitmap();
                    OcrImageCount = imgFilter.ToBitmap();
                    imgFilter.Save(@"ocrimages\itemcount.png");
                    _eventAggregator.GetEvent<OcrImageCountReadyEvent>().Publish();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, MethodBase.GetCurrentMethod()?.Name);
                }
            }
        }

        #endregion
    }
}
