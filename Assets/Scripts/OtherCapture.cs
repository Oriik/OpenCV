using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.XImgproc;
using System.Drawing;
using Emgu.CV.UI;

public class OtherCapture : MonoBehaviour {

    public VideoCapture _webcam;
    public int webcamId = 0;   
    public UnityEngine.UI.Image image;

    private Mat frame, frameGray;
    private CascadeClassifier frontFacesCascadeClassifier;
    private string pathToFrontFacesCascadeClassifier = "Assets/data/haarcascades/haarcascade_frontalface_default.xml";
    private Rectangle[] frontFaces;
    private int MIN_FACE_SIZE = 50;
    private int MAX_FACE_SIZE = 200;
    private Texture2D tex;

    void Start()
    {
        _webcam = new VideoCapture(webcamId);
        _webcam.ImageGrabbed += _webcam_ImageGrabbed;
        frame = new Mat();
        frameGray = new Mat();
        frontFaces = new Rectangle[0];
        frontFacesCascadeClassifier = new CascadeClassifier(pathToFrontFacesCascadeClassifier);

    }

    private void _webcam_ImageGrabbed(object sender, EventArgs e)
    {
        _webcam.Retrieve(frame);
        if (frame.IsEmpty) return;

        CvInvoke.CvtColor(frame, frameGray, ColorConversion.Bgr2Gray);
        frontFaces = frontFacesCascadeClassifier.DetectMultiScale(frameGray);
        if(frontFaces.Length > 0)
        {
            Mat frameHead = new Mat(frame, frontFaces[0]);

            displayFrame(frameHead);
        }
       
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
    

    void Update () {
        if (!_webcam.IsOpened) return;
        _webcam.Grab();
	}

    private void OnDestroy()
    {
        CvInvoke.DestroyAllWindows();
    }


}
