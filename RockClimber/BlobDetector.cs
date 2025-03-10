using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.Generic;
using System.Drawing;

public static class BlobDetector
{
    public static List<System.Drawing.Rectangle> DetectHoldsByColor(Mat inputImage, MCvScalar lowerBound, MCvScalar upperBound)
    {
        // Convert to HSV color space
        Mat hsvImage = new Mat();
        CvInvoke.CvtColor(inputImage, hsvImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);

        // Create a binary mask for the given color range
        Mat mask = new Mat();
        CvInvoke.InRange(hsvImage, new ScalarArray(lowerBound), new ScalarArray(upperBound), mask);

        // Clean up noise
        Mat kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Ellipse, new System.Drawing.Size(5, 5), new System.Drawing.Point(-1, -1));
        CvInvoke.MorphologyEx(mask, mask, Emgu.CV.CvEnum.MorphOp.Close, kernel, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());

        // Find contours
        using (var contours = new VectorOfVectorOfPoint())
        {
            CvInvoke.FindContours(mask, contours, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            Console.WriteLine($"Contours detected: {contours.Size}");

            // Extract bounding rectangles for each contour
            var boundingBoxes = new List<System.Drawing.Rectangle>();
            for (int i = 0; i < contours.Size; i++)
            {
                using (var contour = contours[i])
                {
                    // Calculate the bounding rectangle of the contour
                    System.Drawing.Rectangle boundingBox = CvInvoke.BoundingRectangle(contour);

                    // Filter by size
                    if (boundingBox.Width > 3 && boundingBox.Height > 3) // Adjust size thresholds
                    {
                        boundingBoxes.Add(boundingBox);
                    }
                }
            }
            return boundingBoxes;
        }
    }
}

