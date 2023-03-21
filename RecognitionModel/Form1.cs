using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using Emgu.CV.Face;
using System.Linq;
using System.Drawing.Text;
using Tensorflow.Train;
using System.Diagnostics;
using System.Threading;

namespace RecognitionModel
{
    public partial class Form1 : Form
    {
        private CascadeClassifier faceDetector;
        private LBPHFaceRecognizer _recognizer;
        private CameraController _cameraController;
        private int previousPredictedLabel = -1;
        private int faceNotDetectedCounter = 0;

        private string rawPhotosPath;
        private string croppedPhotosPath;
        private string labelsFileName;
        private FaceDatasetManager _faceDatasetManager;

        private List<TrackedFaces> _trackedFaces;


        private bool processingCompleted = false;
        private bool training = File.Exists(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\bin\Debug\TrainedModel\model.yml");
        /// <summary>
        /// threshold for movement in frame to trigger new recognition
        /// </summary>
        private int threshold = 100;
        /// <summary>
        /// threshold for a prediction to be considered recognized
        /// </summary>
        private double confidenceThreshold = 75.0;
        private static int N = 5;

        public Form1()
        {
            InitializeComponent();
            rawPhotosPath = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\rawPhotos";
            croppedPhotosPath = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos";
            labelsFileName = "labels.txt";
            _faceDatasetManager = new FaceDatasetManager(rawPhotosPath, croppedPhotosPath, labelsFileName);
            _trackedFaces = new List<TrackedFaces>();
            processingCompleted = CheckForProcessedImages();
            Dictionary<int, string> labelToName = _faceDatasetManager.LoadLabelToNameMap();
            //Load pre-trained face detection model --> For bad results on predictions, switch to frontalface_default for test purposes
            faceDetector = new CascadeClassifier(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\haarcascade_frontalface_alt.xml");
            _recognizer = new LBPHFaceRecognizer(1, 32);
            if(training)
                _recognizer.Read(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\bin\Debug\TrainedModel\model.yml");
            _cameraController = new CameraController();
            _cameraController.FrameCaptured += CameraController_FrameCaptured;

        }
        /// <summary>
        /// Event handler for the FrameCaptured event in the CameraController class.
        /// Processes the captured image and updates pictureBox1.
        /// </summary>
        /// <param name="image">The captured Bitmap image.</param>
        private void CameraController_FrameCaptured(Bitmap image)
        {
            if (!processingCompleted)
            {
                pictureBox1.Image = image;
                return;
            }
            Bitmap frameBitmap = new Bitmap(image);
            Image<Bgr, byte> frame = frameBitmap.ToImage<Bgr, byte>();
            Image<Gray, byte> grayFrame = frame.Convert<Gray, byte>();
            Rectangle[] faces = faceDetector.DetectMultiScale(grayFrame, 1.1, 5); //Testing both frame and grayFrame to see which works best

            //Update existing tracking data for faces in frame
            foreach(var trackedFace in _trackedFaces)
            {
                trackedFace.FrameCounter++;
            }

            foreach(var face in faces)
            {
                TrackedFaces matchingTrackedFace = null;
                int minDistance = int.MaxValue; 

                foreach(var trackedFace in _trackedFaces)
                {
                    /*if(!IsFacePositionChanged(trackedFace.FaceRectangle, face, threshold))
                    {
                        matchingTrackedFace = trackedFace;
                        break;
                    }*/
                    int deltaX = Math.Abs(trackedFace.FaceRectangle.X - face.X);
                    int deltaY = Math.Abs(trackedFace.FaceRectangle.Y - face.Y);
                    int distance = deltaX + deltaY;

                    if(distance < threshold && distance < minDistance)
                    {
                        matchingTrackedFace = trackedFace;
                        minDistance = distance; 
                    }


                }
                if(matchingTrackedFace != null && matchingTrackedFace.Label != -1)
                //if(matchingTrackedFace != null)
                {
                    matchingTrackedFace.FaceRectangle = face;
                    matchingTrackedFace.FrameCounter = 0;
                }
                else
                {
                     int label = PerformFaceRecognition(grayFrame, face);
                    if(label == -1)//If the label is "unknown"
                    {
                        var unknownFace = _trackedFaces.FirstOrDefault(f => f.FaceRectangle == face);
                        
                        if(unknownFace == null)
                        {
                            _trackedFaces.Add(new TrackedFaces { FaceRectangle = face, Label = label, FrameCounter = 0, LastDrawnLabel = -1, UnknownCounter = 1});
                        }
                        else
                        {
                            unknownFace.UnknownCounter++;
                            if(unknownFace.UnknownCounter >= N)
                            {
                                unknownFace.UnknownCounter = 0;
                                label = PerformFaceRecognition(grayFrame, face);
                                Debug.WriteLine(label);
                                if(label != -1)
                                {
                                    unknownFace.Label = label;
                                }
                            }
                        }
                    }
                    else
                    {
                    _trackedFaces.Add(new TrackedFaces { FaceRectangle = face, Label = label, FrameCounter = 0, LastDrawnLabel = -1 });

                    }
                }
            }
                using (Graphics g = Graphics.FromImage(image))
                {
                     foreach(var trackedFace in _trackedFaces)
                     {
                        
                        string name = GetLabelToNameMap().ContainsKey(trackedFace.Label) ? GetLabelToNameMap()[trackedFace.Label] : "Ukjent";
                        Font font = new Font("Arial", 16);
                        SolidBrush brush = new SolidBrush(Color.Red);
                        PointF point = new PointF(trackedFace.FaceRectangle.Left, trackedFace.FaceRectangle.Top - 20);
                        if(name != "Ukjent")
                        {
                            g.DrawString(name, font, brush, point);
                            g.DrawRectangle(new Pen(brush, 2), trackedFace.FaceRectangle);

                        }


                     }
                    
                }
            //Remove old tracked faces based on number of frames not detected
            _trackedFaces.RemoveAll(t => t.FrameCounter > 30);
            /*
            foreach(var trackedFace in _trackedFaces)
            {
                bool faceDetected = faces.Any(face => IsFacePositionChanged(trackedFace.FaceRectangle, face, threshold));
                if (faceDetected)
                {
                    trackedFace.FrameCounter++;
                }
            }
            */
            pictureBox1.Image = image;
        }

        private bool IsFacePositionChanged(Rectangle face1, Rectangle face2, int threshold)
        {
            int deltaX = Math.Abs(face1.X - face2.X);
            int deltaY = Math.Abs(face1.Y - face2.Y);
            
            //Debug.WriteLine(deltaX + ", " +  deltaY);
            return deltaX > threshold || deltaY > threshold;
        }

        private Dictionary<int, string> GetLabelToNameMap()
        {
            Dictionary<int, string> labelToname = _faceDatasetManager.LoadLabelToNameMap();
            if(labelToname == null || labelToname.Count == 0)
            {
                labelToname = new Dictionary<int, string>
                {
                    {1, "MarkusPedersen" },
                    {2, "MatiasRaknes" },
                    {3, "StianTrohaug" }
                };
                _faceDatasetManager.SaveLabelToNameMap(labelToname);
            }
            return labelToname;
        }


        /// <summary>
        /// Performs face recognition on the given grayscale image and returns the label of the recognized face.
        /// </summary>
        /// <param name="grayFrame">The grayscale image containing the face.</param>
        /// <param name="face">The detected face in the image.</param>
        /// <returns>The label of the recognized face.</returns>
        private int PerformFaceRecognition(Image<Gray, byte> grayFrame, Rectangle face)
        {
            //If training is uncompleted, return -1
            if (!training)
            {
                Console.WriteLine("The model is not yet trained");
                return -1;
            }
            Image<Gray, byte> faceImage = grayFrame.Copy(face);
            //faceImage.Resize(50, 50, Inter.Cubic);
            var result = _recognizer.Predict(faceImage);
            Debug.WriteLine(result.Distance);
            
            if(result.Distance <= confidenceThreshold)
            {
                return result.Label;
            }
            else
            {
                return -1; //Return -1 for label "ukjent"
            }
            return result.Label; 
        }


        /// <summary>
        /// Event handler for the click event of the prepPic button.
        /// Processes raw photos and saves the label to name mapping.
        /// </summary>
        private void prepPic_Click(object sender, EventArgs e)
        {
            if (!CheckForProcessedImages())
            {
                _faceDatasetManager.ProcessRawPhotos();
                Dictionary<int, string> newLabelToName = new Dictionary<int, string>
                {
                    {1, "MarkusPedersen" },
                    {2, "MatiasRaknes" },
                    {3, "StianTrohaug" }

                };

                _faceDatasetManager.SaveLabelToNameMap(newLabelToName);
                processingCompleted = true;
            }
            
        }
      

        /// <summary>
        /// Checks if the given image file is in a supported format.
        /// </summary>
        /// <param name="imagePath">The file path of the image.</param>
        /// <returns>True if the image is in a supported format, false otherwise.</returns>
        private bool IsSupportedImageFormat(string imagePath)
        {
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
            string fileExtension = Path.GetExtension(imagePath).ToLower();

            return supportedExtensions.Contains(fileExtension);
        }


        private void btn_train_Click(object sender, EventArgs e)
        {
            if(!training)
                TrainModel();
        }


        /// <summary>
        /// Trains the face recognition model with the available dataset.
        /// </summary>
        private void TrainModel()
        {

            // Load label to name map
            Dictionary<int, string> labelToName = _faceDatasetManager.LoadLabelToNameMap();

            // Get cropped photos from the croppedPhotosPath
            List<Mat> images = new List<Mat>();
            List<int> labels = new List<int>();

            foreach (KeyValuePair<int, string> entry in labelToName)
            {
                int label = entry.Key;
                string personName = entry.Value;
                string personCroppedPath = Path.Combine(croppedPhotosPath, personName);

                foreach (string imagePath in Directory.GetFiles(personCroppedPath))
                {
                    Mat image = new Mat(imagePath);
                    CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                    images.Add(image);
                    labels.Add(label);
                }
            }

            _recognizer.Train(images.ToArray(), labels.ToArray());
            _recognizer.Write("C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\TrainedModel\\model.yml");
            _recognizer.Read("C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\TrainedModel\\model.yml");
            training = true; 
        }


        /// <summary>
        /// Checks if there are any processed images in the dataset.
        /// </summary>
        /// <returns>True if there are any processed images, false otherwise.</returns>
        private bool CheckForProcessedImages()
        {
            var directories = Directory.GetDirectories(croppedPhotosPath);
            foreach (var dir in directories)
            {
                if (Directory.GetFiles(dir).Any(file => IsSupportedImageFormat(file)))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
