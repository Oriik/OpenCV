using UnityEngine;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using Emgu.CV.Util;

public class Seuillage : MonoBehaviour {

    public VideoCapture _webcam;
    public int webcamId = 0;
    public UnityEngine.UI.Image image;

    private Mat frame, frameGray;
    private Texture2D tex;
    private VectorOfVectorOfPoint contours;
    private double _minContourAreaProportion=0.001;
    private double _maxContourAreaProportion=0.03;
    private double _squareShapeTolerance=0.1;
    private double _areaRectTolerance=0.1;

    void Start()
    {
        _webcam = new VideoCapture(webcamId);
        _webcam.ImageGrabbed += _webcam_ImageGrabbed;
        frame = new Mat();
        frameGray = new Mat();
        
       
    }

    private void _webcam_ImageGrabbed(object sender, EventArgs e)
    {
        _webcam.Retrieve(frame);
        if (frame.IsEmpty) return;

        CvInvoke.CvtColor(frame, frameGray, ColorConversion.Bgr2Gray);
        CvInvoke.AdaptiveThreshold(frameGray, frameGray, 255, AdaptiveThresholdType.MeanC,ThresholdType.BinaryInv, 7, 6);

        Mat hierarchy = new Mat();
        contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(frameGray, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);
        contours = FilterContours(frameGray);
      
        for (int i = 0; i < contours.Size; i++)
        {
            CvInvoke.DrawContours(frame, contours, i, new MCvScalar(0, 0, 255), 3);  
        }
        displayFrame(frame);
      

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

    private void displayFrame(Mat _frame)
    {
        if (!_frame.IsEmpty)
        {
            Destroy(tex);
            tex = convertMatToTexture2D(_frame.Clone(), (int)image.rectTransform.rect.width, (int)image.rectTransform.rect.width);
            image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        }
    }

    Texture2D convertMatToTexture2D(Mat matImage, int width, int height)
    {
        if (matImage.IsEmpty) return new Texture2D(width, height);
        CvInvoke.Resize(matImage, matImage, new Size(width, height));

        if (matImage.IsEmpty) return new Texture2D(width, height);
        CvInvoke.Flip(matImage, matImage, FlipType.Vertical);

        if (matImage.IsEmpty) return new Texture2D(width, height);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(matImage.ToImage<Rgba, Byte>().Bytes);
        texture.Apply();

        return texture;
    }

    void Update()
    {
        if (!_webcam.IsOpened) return;
        _webcam.Grab();
    }

    private void OnDestroy()
    {
        CvInvoke.DestroyAllWindows();
    }

}
