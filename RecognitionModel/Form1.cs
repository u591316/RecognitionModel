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


        private bool processingCompleted = false;
        private bool training = File.Exists(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\bin\Debug\TrainedModel\model.yml"); 


        public Form1()
        {
            InitializeComponent();
            rawPhotosPath = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\rawPhotos";
            croppedPhotosPath = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos";
            labelsFileName = "labels.txt";
            _faceDatasetManager = new FaceDatasetManager(rawPhotosPath, croppedPhotosPath, labelsFileName);
            processingCompleted = CheckForProcessedImages();
            Dictionary<int, string> labelToName = _faceDatasetManager.LoadLabelToNameMap();
            //Load pre-trained face detection model --> For bad results on predictions, switch to frontalface_default for test purposes
            faceDetector = new CascadeClassifier(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\haarcascade_frontalface_alt.xml");
            _recognizer = new LBPHFaceRecognizer();
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
            Rectangle[] faces = faceDetector.DetectMultiScale(grayFrame, 1.1, 5);
            if (faces.Length > 0)
            {
                faceNotDetectedCounter = 0;
                if(previousPredictedLabel == -1)
                {
                    previousPredictedLabel = PerformFaceRecognition(grayFrame, faces[0]);
                }
                using (Graphics g = Graphics.FromImage(image))
                {
                    //Labelling for detection in frame
                    string name = GetLabelToNameMap().ContainsKey(previousPredictedLabel) ? GetLabelToNameMap()[previousPredictedLabel] : "Ukjent";
                    Font font = new Font("Arial", 16);
                    SolidBrush brush = new SolidBrush(Color.Red);
                    PointF point = new PointF(faces[0].Left, faces[0].Top - 20);

                    g.DrawString(name, font, brush, point);
                }
            }
            else
            {
                faceNotDetectedCounter++;
                //Number of frames without a detection threshold for running new recognition
                if(faceNotDetectedCounter > 30)
                {
                    previousPredictedLabel = -1;
                    faceNotDetectedCounter = 0;
                }
            }
            pictureBox1.Image = image; 
        }

        private Dictionary<int, string> GetLabelToNameMap()
        {
            Dictionary<int, string> labelToname = _faceDatasetManager.LoadLabelToNameMap();
            if(labelToname == null || labelToname.Count == 0)
            {
                labelToname = new Dictionary<int, string>
                {
                    {1, "Markus Pedersen" },
                    {2, "Matias Raknes" },
                    {3, "Elon Musk" },
                    {4, "Stian Trohaug" }
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
            var result = _recognizer.Predict(faceImage);

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
                 {3, "ElonMusk" },
                 {4, "StianTrohaug" }

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
