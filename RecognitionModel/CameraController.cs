using Emgu.CV;
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

        /// <summary>
        /// Initializes a new instance of the CameraController class and starts capturing video frames.
        /// </summary>
        public CameraController()
        {
            _capture = new VideoCapture();
            _capture.ImageGrabbed += Capture_ImageGrabbed;
            _capture.Start();
        }

        /// <summary>
        /// Handles the ImageGrabbed event of the VideoCapture object, processes the captured frame, and raises the FrameCaptured event.
        /// </summary>
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame);
            if (frame.IsEmpty)
            {
                Console.WriteLine("Frame is empty");
                return;
            }
            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
            OnFrameCaptured(image.ToBitmap());

        }


        /// <summary>
        /// Represents the method that will handle the FrameCaptured event of a CameraController object.
        /// </summary>
        /// <param name="image">The captured image as a Bitmap object.</param>
        public delegate void FrameCapturedHandler(Bitmap image);

        /// <summary>
        /// Occurs when a new frame is captured by the CameraController.
        /// </summary>
        public event FrameCapturedHandler FrameCaptured;


        /// <summary>
        /// Raises the FrameCaptured event with the specified captured image.
        /// </summary>
        /// <param name="image">The captured image as a Bitmap object.</param>
        protected virtual void OnFrameCaptured(Bitmap image)
        {
            FrameCaptured?.Invoke(image);
        }
    }
}
