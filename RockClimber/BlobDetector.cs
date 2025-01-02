using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.Generic;
using System.Drawing;

public static class BlobDetector
{
    public static List<CircleF> DetectBlobs(Mat inputImage)
    {
        // Convert to grayscale
        Mat grayImage = new Mat();
        CvInvoke.CvtColor(inputImage, grayImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

        // Apply Gaussian blur
        CvInvoke.GaussianBlur(grayImage, grayImage, new System.Drawing.Size(5, 5), 0);

        // Apply adaptive thresholding
        Mat thresholded = new Mat();
        CvInvoke.AdaptiveThreshold(grayImage, thresholded, 255,
            Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC,
            Emgu.CV.CvEnum.ThresholdType.Binary, 15, 5);

        // Detect contours
        using (var contours = new VectorOfVectorOfPoint())
        {
            CvInvoke.FindContours(thresholded, contours, null, Emgu.CV.CvEnum.RetrType.External,
                Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            Console.WriteLine($"Contours detected: {contours.Size}");

            // Draw contours for debugging
            CvInvoke.DrawContours(inputImage, contours, -1, new MCvScalar(255, 0, 0), 2); // Draw in blue

            // Extract circular blobs
            var blobs = new List<CircleF>();
            for (int i = 0; i < contours.Size; i++)
            {
                using (var contour = contours[i])
                {
                    var circle = CvInvoke.MinEnclosingCircle(contour);
                    double contourArea = CvInvoke.ContourArea(contour);
                    double perimeter = CvInvoke.ArcLength(contour, true);
                    double circularity = 4 * Math.PI * contourArea / (perimeter * perimeter);

                    Console.WriteLine($"Blob {i}: Radius={circle.Radius}, Area={contourArea}, Circularity={circularity}");

                    // Relaxed filtering for testing
                    if (circle.Radius > 5 && circularity > 0.5)
                    {
                        blobs.Add(circle);
                    }
                }
            }
            return blobs;
        }
    }
}


