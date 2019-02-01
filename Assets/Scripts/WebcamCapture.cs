using UnityEngine;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using Emgu.CV.Util;

public class WebcamCapture : MonoBehaviour
{

    #region Variables

    public int webcamId = 0;
    public UnityEngine.UI.Image image;
    public UnityEngine.UI.Image imageGray;
    public int sizeStruct = 1;
    public int nbOfIter = 10;
    [Range(0, 1)] public double _squareShapeTolerance = 0.8;
    [Range(0, 1)] public double _areaRectTolerance = 0.8;
    [Range(0, 120)] public int intensity = 10;
    [Range(0, 255)] public int sMin = 100;
    [Range(0, 255)] public int sMax = 255;
    [Range(0, 255)] public int vMin = 100;
    [Range(0, 255)] public int vMax = 255;


    private Mat frame, frameGray, frameHSV;
    private Texture2D tex, texGray;
    private VideoCapture webcam;
    private VectorOfVectorOfPoint contours;
    private double _minContourAreaProportion = 0.001;
    private double _maxContourAreaProportion = 0.03;
    private const int GREEN = 40;
    private double biggestContourArea = 0;
    private VectorOfPoint biggestContour;
    private int biggestContourIndex;

    #endregion


    void Start()
    {
        webcam = new VideoCapture(webcamId);
        webcam.ImageGrabbed += _webcam_ImageGrabbed;
        frame = new Mat();
        frameGray = new Mat();
    }

    void Update()
    {
        if (!webcam.IsOpened) return;
        webcam.Grab();

    }

    private void _webcam_ImageGrabbed(object sender, EventArgs e)
    {
        webcam.Retrieve(frame);
        if (frame.IsEmpty) return;        
        
        CvInvoke.Flip(frame, frame, FlipType.Horizontal);

        frameHSV = frame.Clone();
        CvInvoke.CvtColor(frame, frameHSV, ColorConversion.Rgb2Hsv);
        Image<Hsv, byte> HSV = frameHSV.ToImage<Hsv, byte>();
        Image<Gray, byte> hsvGray = HSV.InRange(new Hsv(GREEN - intensity, sMin, vMin), new Hsv(GREEN + intensity, sMax, vMax));
        CvInvoke.Imshow("TEST", hsvGray);
       
        Image<Gray, byte> dilate = hsvGray.Clone();
        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(2 * sizeStruct + 1, 2 * sizeStruct + 1), new Point(sizeStruct, sizeStruct));

        CvInvoke.Dilate(hsvGray, dilate, structuringElement, new Point(-1, -1), nbOfIter, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Erode(dilate, dilate, structuringElement, new Point(-1, -1), nbOfIter, BorderType.Constant, new MCvScalar(0));


        Mat hierarchy = new Mat();
        contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(dilate, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);
        biggestContourArea = 0;
        for (int i = 0; i < contours.Size; i++)
        {
            if (CvInvoke.ContourArea(contours[i]) > biggestContourArea)
            {
                biggestContour = contours[i];
                biggestContourIndex = i;
                biggestContourArea = CvInvoke.ContourArea(contours[i]);
            }

        }

        //CvInvoke.CvtColor(frame, frameGray, ColorConversion.Bgr2Gray);
        //CvInvoke.AdaptiveThreshold(frameGray, frameGray, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 7, 6);

        //Mat hierarchy = new Mat();
        //contours = new VectorOfVectorOfPoint();
        //CvInvoke.FindContours(frameGray, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);
        //contours = FilterContours(frameGray);

        if (contours != null &&  contours.Size > 0)
        {
            CvInvoke.DrawContours(frame, contours, biggestContourIndex, new MCvScalar(0, 0, 255), 3);
        }


        displayFrame(frame, image, tex);
        //displayFrame(frameGray, imageGray, texGray);


    }

    private void displayFrame(Mat _frame, UnityEngine.UI.Image _image, Texture2D _tex)
    {
        if (!_frame.IsEmpty)
        {
            _tex = convertMatToTexture2D(_frame.Clone(), (int)_image.rectTransform.rect.width, (int)_image.rectTransform.rect.width);
            _image.sprite = Sprite.Create(_tex, new Rect(0.0f, 0.0f, _tex.width, _tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }

    Texture2D convertMatToTexture2D(Mat _matImage, int _width, int _height)
    {
        if (_matImage.IsEmpty) return new Texture2D(_width, _height);

        CvInvoke.Resize(_matImage, _matImage, new Size(_width, _height));

        if (_matImage.IsEmpty) return new Texture2D(_width, _height);

        CvInvoke.Flip(_matImage, _matImage, FlipType.Vertical);

        if (_matImage.IsEmpty) return new Texture2D(_width, _height);

        Texture2D texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(_matImage.ToImage<Rgba, Byte>().Bytes);
        texture.Apply();

        return texture;
    }

    private VectorOfVectorOfPoint FilterContours(Mat _webcamFrame)
    {
        VectorOfVectorOfPoint vectListMarkers = new VectorOfVectorOfPoint();

        for (int i = 0; i < contours.Size; i++)
        {
            double contourArea = CvInvoke.ContourArea(contours[i], false);
            double contourAreaProportion = contourArea / (_webcamFrame.Height * _webcamFrame.Width);

            if (contourAreaProportion < _minContourAreaProportion || contourAreaProportion > _maxContourAreaProportion)
                continue;

            RotatedRect rect = CvInvoke.MinAreaRect(contours[i]);
            double squareShapeComparison = rect.Size.Width / rect.Size.Height;
            if (squareShapeComparison < (1.0 - _squareShapeTolerance) || squareShapeComparison > (1.0 + _squareShapeTolerance))
                continue;

            double areaComparison = contourArea / (rect.Size.Width * rect.Size.Height);
            if (areaComparison < (1.0 - _areaRectTolerance) || areaComparison > (1.0 + _areaRectTolerance))
                continue;


            vectListMarkers.Push(contours[i]);
        }

        return vectListMarkers;
    }

    private void OnDestroy()
    {
        CvInvoke.DestroyAllWindows();

    }

}
