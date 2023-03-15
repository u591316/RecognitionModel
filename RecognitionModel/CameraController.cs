﻿using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RecognitionModel
{
    internal class CameraController
    {

        private VideoCapture _capture;

        public CameraController()
        {
            _capture = new VideoCapture(0);
            _capture.ImageGrabbed += Capture_ImageGrabbed;
            _capture.Start();
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame);
            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
            OnFrameCaptured(image.ToBitmap());

        }
      


        public delegate void FrameCapturedHandler(Bitmap image);
        public event FrameCapturedHandler FrameCaptured;

        protected virtual void OnFrameCaptured(Bitmap image)
        {
            FrameCaptured?.Invoke(image);
        }
    }
}