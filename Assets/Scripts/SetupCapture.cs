using UnityEngine;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using Emgu.CV.Util;

public class SetupCapture : MonoBehaviour
{

    #region Variables

    public int webcamId = 0;
    public UnityEngine.UI.Image image;

    private Texture2D texFrame;
    private VideoCapture webcam;
    private Mat frame, test;

    private Vector2 orgBoxPos = Vector2.zero;
    private Vector2 endBoxPos = Vector2.zero;
    private RaycastHit hit;

    private Texture2D tex;
    private UnityEngine.Color texColor;
    private double hTarget;
    private double sTarget;
    private double vTarget;
    private int intensity = 20;
    private double sMin = 0;
    private double vMin = 0;
    private double sMax = 255;
    private double vMax = 255;
    private bool colorDetect = false;
    #endregion

    #region UnityFunction

    private void Start()
    {
        SetupWebcam();
        SetupDraw();
    }

    void Update()
    {
        if (!webcam.IsOpened) return;
        webcam.Grab();

        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                orgBoxPos = Input.mousePosition;
            }
            else
            {
                endBoxPos = Input.mousePosition;
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (endBoxPos != Vector2.zero && orgBoxPos != Vector2.zero)
            {
                HandleUnitSelection();
            }

            endBoxPos = orgBoxPos = Vector2.zero;
        }
        if (colorDetect)
        {
          
        }

       


    }
    #endregion


    private void HandleUnitSelection()
    {
        Vector2 temp = endBoxPos - orgBoxPos;
        Rectangle rect = new Rectangle((int)orgBoxPos.x, 480 - (int)(orgBoxPos.y),
                                (int)(Mathf.Abs(temp.x)),
                                (int)(Mathf.Abs(temp.y)));


         test = new Mat(frame, rect);

        Image<Bgr, Byte> imgBgr = test.ToImage<Bgr, byte>();

        var moments = CvInvoke.Moments(imgBgr[0]);
        int cx = (int)(moments.M10 / moments.M00);
        int cy = (int)(moments.M01 / moments.M00);

        int b = 0, g = 0, r = 0;
        for (int i = 0; i < 5; i++)
        {
            Point pts1 = new Point(cx + i, cy + i);
            Point pts2 = new Point(cx - i, cy - i);
            Point pts3 = new Point(cx - i, cy + i);
            Point pts4 = new Point(cx + i, cy - i);

            r += (int)imgBgr[pts1].Red;
            r += (int)imgBgr[pts2].Red;
            r += (int)imgBgr[pts3].Red;
            r += (int)imgBgr[pts4].Red;

            g += (int)imgBgr[pts1].Green;
            g += (int)imgBgr[pts2].Green;
            g += (int)imgBgr[pts3].Green;
            g += (int)imgBgr[pts4].Green;

            b += (int)imgBgr[pts1].Blue;
            b += (int)imgBgr[pts2].Blue;
            b += (int)imgBgr[pts3].Blue;
            b += (int)imgBgr[pts4].Blue;
        }

        r = r / 20;
        g = g / 20;
        b = b / 20;

        System.Drawing.Color c = System.Drawing.Color.FromArgb(r, g, b);
        Hsv hsv = new Hsv(c.GetHue(), c.GetSaturation(), c.GetBrightness());
     
        hTarget = hsv.Hue / 2;
        sTarget = hsv.Satuation;
        vTarget = hsv.Value;

        Mat tata = test.Clone();
        CvInvoke.CvtColor(test, tata, ColorConversion.Bgr2Hsv);
        Image<Hsv, byte> ImgHSV = tata.ToImage<Hsv, byte>();

        Image<Gray, byte> tarace = ImgHSV.InRange(new Hsv(hTarget - intensity, 0, 0), new Hsv(hTarget + intensity, 255, 255));

        Mat hierarchy = new Mat();
        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(tarace, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

        double biggestContourArea = 0;
        VectorOfPoint biggestContour = new VectorOfPoint();
        int biggestContourIndex = 0;
        for (int i = 0; i < contours.Size; i++)
        {
            
            if (CvInvoke.ContourArea(contours[i]) > biggestContourArea)
            {
                biggestContour = contours[i];
                biggestContourIndex = i;
                biggestContourArea = CvInvoke.ContourArea(contours[i]);
            }

        }


        Mat biggestContourMat = new Mat(test, CvInvoke.BoundingRectangle(contours[biggestContourIndex]));

       CvInvoke.Mean()
        




       // colorDetect = true;


    }

    private void _webcam_ImageGrabbed(object sender, EventArgs e)
    {
        webcam.Retrieve(frame);
        if (frame.IsEmpty) return;
        //Debug.Log("Frame width ="+frame.Width);
        //Debug.Log("Frame height ="+frame.Height);

        displayFrame(frame, image, texFrame);
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

    void OnGUI()
    {
        if (orgBoxPos != Vector2.zero && endBoxPos != Vector2.zero)
        {
            var rect = new Rect(orgBoxPos.x, Screen.height - orgBoxPos.y,
                                endBoxPos.x - orgBoxPos.x,
                                -1 * (endBoxPos.y - orgBoxPos.y));

            GUI.DrawTexture(rect, tex);

        }
    }

    #region SetupFunction
    private void SetupDraw()
    {
        texColor = new UnityEngine.Color(255, 0, 0);
        tex = new Texture2D(100, 100);
        var texPixels = tex.GetPixels();
        for (int i = 0; i < texPixels.Length; i++)
        {
            texPixels[i] = texColor;
        }
        tex.SetPixels(texPixels);
        tex.Apply();

        hit = new RaycastHit();
    }

    private void SetupWebcam()
    {
        webcam = new VideoCapture(webcamId);
        webcam.ImageGrabbed += _webcam_ImageGrabbed;
        frame = new Mat();
        Debug.Log("cam width =" + webcam.Width);
        Debug.Log("cam height =" + webcam.Height);
    }
    #endregion

    private void OnDestroy()
    {
        CvInvoke.DestroyAllWindows();

    }
}
