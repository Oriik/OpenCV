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
    private Mat frame;

    private Vector2 orgBoxPos = Vector2.zero;
    private Vector2 endBoxPos = Vector2.zero;
    private RaycastHit hit;

    private Texture2D tex;
    private UnityEngine.Color texColor;
    private double hTarget;
    private int intensity = 20;
    private double sMin;
    private double vMin;
    private double sMax;
    private double vMax;
    private bool oui = false;
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
        if (oui)
        {
            Mat temp = frame.Clone();
            CvInvoke.CvtColor(frame, temp, ColorConversion.Rgb2Hsv);
            Image<Hsv, byte> ImgHSV = temp.ToImage<Hsv, byte>();
            Image<Gray, byte> hsv = ImgHSV.InRange(new Hsv(hTarget - intensity, sMin, vMin), new Hsv(hTarget + intensity, sMax, vMax));
            CvInvoke.Imshow("HSVOUI", hsv);
        }


    }
    #endregion


    private void HandleUnitSelection()
    {
        double[] minV, maxV;
        Point[] minL, maxL;
        int BlueHist;
        int GreenHist;
        int RedHist;

        Vector2 temp = endBoxPos - orgBoxPos;
        Rectangle rect = new Rectangle((int)orgBoxPos.x, 480 - (int)(orgBoxPos.y),
                                (int)(Mathf.Abs(temp.x)),
                                (int)(Mathf.Abs(temp.y)));



        Mat test = new Mat(frame, rect);
        Mat hsv = test.Clone();

        CvInvoke.CvtColor(test, hsv, ColorConversion.Bgr2Hsv);

        Mat[] channels = hsv.Split();

        RangeF H = channels[0].GetValueRange();
        RangeF S = channels[1].GetValueRange();
        RangeF V = channels[2].GetValueRange();

        Debug.Log(string.Format("Max H {0} Min H {1}", H.Max, H.Min));
        Debug.Log(string.Format("Max S {0} Min S {1}", S.Max, S.Min));
        sMin = S.Min;
        sMax = S.Max;
        Debug.Log(string.Format("Max V {0} Min V {1}", V.Max, V.Min));
        vMin = V.Min;
        vMax = V.Max;

        MCvScalar mean = CvInvoke.Mean(hsv);
        Debug.Log(string.Format("Mean H {0} Mean S {1} Mean V {2} ", mean.V0, mean.V1, mean.V2));
        hTarget = mean.V0;

        oui = true;



        //Mat median = test.Clone();
        ////CvInvoke.Imshow("test2", test);

        //CvInvoke.Rectangle(frame, rect, new MCvScalar(0, 0, 255), 3);

        //CvInvoke.Imshow("test", frame);

        //CvInvoke.MedianBlur(test, median, 101);
        //CvInvoke.Imshow("georges", median);

        //Image<Hsv, Byte> hsv = median.ToImage<Hsv, Byte>();
        ////CvInvoke.Imshow("georges2", hsv);
        //byte georges = hsv.Data[hsv.Width / 2, hsv.Height / 2,0];
        //Debug.Log(georges);
        //Image<Hsv, Byte> imgHSV = test.ToImage<Hsv, Byte>();


        //Image<Gray, Byte> img = imgHSV[0];


        //DenseHistogram hist = new DenseHistogram(256, new RangeF(0.0f, 255.0f));
        //hist.Calculate<Byte>(new Image<Gray, byte>[] { img }, true, null);

        //double[] minValue, maxValue;
        //Point[] minLocation;
        //Point[] maxLocation;
        //hist.MinMax(out minValue, out maxValue, out minLocation, out maxLocation);


        //Debug.Log(maxLocation.Length);
        //Debug.Log(maxLocation[0].X);
        //Debug.Log(maxLocation[0].Y);
        //Debug.Log(maxValue[0]);


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
