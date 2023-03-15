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
        private bool training = false; 


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

            Dictionary<int, string> labelToName = new Dictionary<int, string>
            {
                {1, "Markus Pedersen" },
                {2, "Matias Raknes" },
                {3, "Elon Musk" },
                {4, "Stian Trohaug" }
            };

            if (faces.Length > 0)
            {
                faceNotDetectedCounter = 0;
                if(previousPredictedLabel == -1)
                {
                    previousPredictedLabel = PerformFaceRecognition(grayFrame, faces[0]);
                }
                using (Graphics g = Graphics.FromImage(image))
                {
                    // Justere tekststil og posisjon etter behov
                    string name = labelToName[previousPredictedLabel];
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

        private int PerformFaceRecognition(Image<Gray, byte> grayFrame, Rectangle face)
        {
            Image<Gray, byte> faceImage = grayFrame.Copy(face);
            var result = _recognizer.Predict(faceImage);

            return result.Label; 
        }

        private void prepPic_Click(object sender, EventArgs e)
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

        private void cropPictures()
        {
            //Add folders for pictures to be cropped and grayed 
            string person1Folder = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\rawPhotos\\MarkusPedersen";
            string person2Folder = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\rawPhotos\\MatiasRaknes";
            string person3Folder = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\rawPhotos\\ElonMusk";
            string person4Folder = @"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\bin\Debug\rawPhotos\StianTrohaug";

            //Add folder for cropped faces
            string person1Cropped = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos\\MarkusPedersen";
            string person2Cropped = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos\\MatiasRaknes";
            string person3Cropped = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos\\ElonMusk";
            string person4Cropped = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos\\StianTrohaug";


            foreach (string imagePath in Directory.GetFiles(person1Folder))
            {
                if(IsSupportedImageFormat(imagePath))
                    FaceUtils.CropFaces(imagePath, person1Cropped);
            }
            foreach(string imagePath in Directory.GetFiles(person2Folder))
            {
                if (IsSupportedImageFormat(imagePath))
                    FaceUtils.CropFaces(imagePath, person2Cropped);
            }
            foreach(string imagePath in Directory.GetFiles(person3Folder))
            {
                if (IsSupportedImageFormat(imagePath))
                    FaceUtils.CropFaces(imagePath, person3Cropped);
            }
            foreach (string imagePath in Directory.GetFiles(person4Folder))
            {
                if (IsSupportedImageFormat(imagePath))
                    FaceUtils.CropFaces(imagePath, person4Cropped);
            }
        }

        private bool IsSupportedImageFormat(string imagePath)
        {
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
            string fileExtension = Path.GetExtension(imagePath).ToLower();

            return supportedExtensions.Contains(fileExtension);
        }

        private void cropFaces(string imagePath, string outputFolderPath)
        {
            using (Mat image = CvInvoke.Imread(imagePath, ImreadModes.Color))
            {
                if (image != null && !image.IsEmpty)
                {
                    using (Mat gray = new Mat())
                    {
                        CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                        CvInvoke.EqualizeHist(gray, gray);

                        // Detect faces
                        Rectangle[] faces = faceDetector.DetectMultiScale(gray, 1.1, 10, new Size(50, 50));

                        // Crop and save each face
                        foreach (Rectangle face in faces)
                        {
                            using (Mat croppedFace = new Mat(image, face))
                            {
                                string outputFilePath = Path.Combine(outputFolderPath, $"{Guid.NewGuid()}.jpg");
                                CvInvoke.Imwrite(outputFilePath, croppedFace);
                            }
                        }
                    }
                }
            }
        }


        private void btn_train_Click(object sender, EventArgs e)
        {
            TrainModel();
        }

        private void TrainModel()
        {
            LBPHFaceRecognizer _recognizer = new LBPHFaceRecognizer();

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
            training = true; 
        }


        private void PredictLabels_btn(object sender, EventArgs e)
        {
            predictionTest();
        }

        private void predictionTest()
        {
            string testImageRaw = @"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\bin\Debug\rawPhotos\StianTrohaugTest\Jesus-fødselsdag.jpg";
            string testImageCroppedOut = "C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedTestPhotos";
            cropFaces(testImageRaw, testImageCroppedOut);
            //Get the filename of the test image
            string testImageFilename = Path.GetFileName(testImageRaw);
            string testImageCroppedPath = Path.Combine(testImageCroppedOut, testImageFilename);

            //Dictionary to attach each label to correct person
            Dictionary<int, string> labelToName = new Dictionary<int, string>
            {
                {1, "Markus Pedersen" },
                {2, "Matias Raknes" },
                {3, "Elon Musk" },
                {4, "Stian Trohaug" }
            };
            //Load pretrained model from file
            var loadedRecognizer =  new LBPHFaceRecognizer();
            loadedRecognizer.Read(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\bin\Debug\TrainedModel\model.yml");

            //Read testPicture and convert to grayScale

            Image<Bgr, byte> testImage = new Image<Bgr, byte>(testImageCroppedPath);
            Image<Gray, byte> grayTestImage = testImage.Convert<Gray, byte>();

            //Get prediction from model 
            var result = loadedRecognizer.Predict(grayTestImage);

            Console.WriteLine($"Predicted Label: {result.Label}");
            Console.WriteLine($"Predicted Name: {labelToName[result.Label]}");
            Console.WriteLine($"Confidence: {result.Distance}");
        }

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
