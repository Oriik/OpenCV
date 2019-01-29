using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.XImgproc;
using System.Drawing;
using Emgu.CV.UI;

public class CamEmgu : MonoBehaviour
{

    public VideoCapture _webcam;
    public int webcamId = 0;
    public int sizeBlur = 21;
    public int nbOfIter = 10;
    public int sizeStruct = 1;

    private VectorOfVectorOfPoint contours;
    private VectorOfPoint biggestContour;
    int biggestContourIndex;
    double biggestContourArea=0;

    [Range(0, 60)] public int intensity;
    [Range(0, 255)] public int sMin;
    [Range(0, 255)] public int sMax;
    [Range(0, 255)] public int vMin;
    [Range(0, 255)] public int vMax;

    // Use this for initialization
    void Start()
    {
        _webcam = new VideoCapture(webcamId);

    }

    // Update is called once per frame
    void Update()
    {
        biggestContour = new VectorOfPoint();
        contours = new VectorOfVectorOfPoint();
        biggestContourArea = 0;
        
        Mat image, imgGray, imgHSV, imgBlur, imgMedianBlur, imgGaussianBlur;
        //image = CvInvoke.Imread("Assets/jordi.jpg");
        image = _webcam.QueryFrame();
        CvInvoke.Flip(image, image, FlipType.Horizontal);
        
        imgBlur = image.Clone();
        imgMedianBlur = image.Clone();
        imgGaussianBlur = image.Clone();

        CvInvoke.Blur(image, imgBlur, new Size(sizeBlur, sizeBlur), new Point(-1, 1));
        CvInvoke.GaussianBlur(image, imgGaussianBlur, new Size(sizeBlur, sizeBlur), sizeBlur / 2.0);
        CvInvoke.MedianBlur(image, imgMedianBlur, sizeBlur);

        imgGray = image.Clone();
        CvInvoke.CvtColor(image, imgGray, ColorConversion.Bgr2Gray);

        imgHSV = image.Clone();
        CvInvoke.CvtColor(image, imgHSV, ColorConversion.Rgb2Hsv);
        Image<Hsv, byte> ImgHSV = imgHSV.ToImage<Hsv, byte>();
        Image<Gray, byte> hsv = ImgHSV.InRange(new Hsv(60 - intensity, sMin, vMin), new Hsv(60 + intensity, sMax, vMax));
        CvInvoke.Imshow("HSV", hsv);

        Image<Gray, byte> dilate = hsv.Clone();
        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(2 * sizeStruct + 1, 2 * sizeStruct + 1), new Point(sizeStruct, sizeStruct));

        CvInvoke.Dilate(hsv, dilate, structuringElement, new Point(-1, -1), nbOfIter, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Erode(dilate, dilate, structuringElement, new Point(-1, -1), nbOfIter, BorderType.Constant, new MCvScalar(0));


        Mat hierarchy = new Mat();
        CvInvoke.FindContours(dilate, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);
        for (int i = 0; i < contours.Size; i++)
        {
            if(CvInvoke.ContourArea(contours[i]) > biggestContourArea)
            {
                biggestContour = contours[i];
                biggestContourIndex = i;
                biggestContourArea = CvInvoke.ContourArea(contours[i]);
            }
           
        }
        var moments = CvInvoke.Moments(biggestContour);
        int cx = (int)(moments.M10 / moments.M00);
        int cy = (int)(moments.M01 / moments.M00);
        Point centroid = new Point(cx, cy);

        CvInvoke.Circle(image, centroid, 2, new MCvScalar(0, 0, 255), 2);
        CvInvoke.DrawContours(image, contours, biggestContourIndex, new MCvScalar(0,0,255), 5);
        CvInvoke.Imshow("DILATATION", dilate);
        CvInvoke.Imshow("Webcam View", image);

        CvInvoke.WaitKey(24);
    }

    private void OnDestroy()
    {
        CvInvoke.DestroyAllWindows();

    }
}
