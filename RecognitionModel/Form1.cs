using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Tensorflow;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using Emgu.CV.Face;
using System.Linq;

namespace RecognitionModel
{
    public partial class Form1 : Form
    {
        private CascadeClassifier faceDetector;
        public Form1()
        {
            InitializeComponent();
            //Load pre-trained face detection model --> For bad results on predictions, switch to frontalface_default for test purposes
            faceDetector = new CascadeClassifier(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\haarcascade_frontalface_alt.xml");
        }

        private void prepPic_Click(object sender, EventArgs e)
        {
            cropPictures();
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
                    cropFaces(imagePath, person1Cropped);
            }
            foreach(string imagePath in Directory.GetFiles(person2Folder))
            {
                if (IsSupportedImageFormat(imagePath))
                    cropFaces(imagePath, person2Cropped);
            }
            foreach(string imagePath in Directory.GetFiles(person3Folder))
            {
                if (IsSupportedImageFormat(imagePath))
                    cropFaces(imagePath, person3Cropped);
            }
            foreach (string imagePath in Directory.GetFiles(person4Folder))
            {
                if (IsSupportedImageFormat(imagePath))
                    cropFaces(imagePath, person4Cropped);
            }
        }

        private bool IsSupportedImageFormat(string imagePath)
        {
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
            string fileExtension = Path.GetExtension(imagePath).ToLower();

            return supportedExtensions.Contains(fileExtension);
        }

        private void cropFaces(string imagePath, string outputPath)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(imagePath);
            //Adjust resolution to reduce memory usage..
            image = image.Resize(640, 480, Inter.Linear);
            Image<Gray, byte> grayImage = image.Convert<Gray, byte>();
            Rectangle[] faces = faceDetector.DetectMultiScale(grayImage, 1.1, 5);
            foreach (Rectangle face in faces)
            {
                Image<Gray, byte> faceImage = grayImage.Copy(face);
                faceImage.Save(Path.Combine(outputPath, Path.GetFileName(imagePath)));
            }
        }

        private void btn_train_Click(object sender, EventArgs e)
        {
            trainModel();
        }

        private void trainModel()
        {
            LBPHFaceRecognizer _recognizer = new LBPHFaceRecognizer();


            string[] person1 = Directory.GetFiles("C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos\\MarkusPedersen");
            string[] person2 = Directory.GetFiles("C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos\\MatiasRaknes");
            string[] person3  = Directory.GetFiles("C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos\\ElonMusk");
            string[] person4  = Directory.GetFiles("C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\croppedPhotos\\StianTrohaug");

            List<Mat> images = new List<Mat>();
            var labels = new List<int>();

            foreach(string imagePath in person1)
            {
                Mat image = new Mat(imagePath);
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                images.Add(image);
                labels.Add(1);
            }
            foreach(string imagePath in person2)
            {
                Mat image = new Mat(imagePath);
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                images.Add(image);
                labels.Add(2);
            }
            foreach(string imagePath in person3)
            {
                Mat image = new Mat(imagePath);
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                images.Add(image);
                labels.Add(3);
            }
            foreach (string imagePath in person4)
            {
                Mat image = new Mat(imagePath);
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
                images.Add(image);
                labels.Add(4);
            }

            _recognizer.Train(images.ToArray(), labels.ToArray());
            _recognizer.Write("C:\\Users\\peder\\source\\repos\\RecognitionModel\\RecognitionModel\\bin\\Debug\\TrainedModel\\model.yml");
            
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
    }
}
