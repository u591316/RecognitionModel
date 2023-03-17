using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecognitionModel;
using System.Threading;

namespace RecognitionModel
{
    
    public class FaceUtils
    {
        private static CascadeClassifier faceDetector = new CascadeClassifier(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\haarcascade_frontalface_alt.xml");


        public static void CropFaces(string imagePath, string outputFolderPath)
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
                            using (Mat croppedFace = new Mat(gray, face))
                            {
                                string outputFilePath = Path.Combine(outputFolderPath, $"{Guid.NewGuid()}.jpg");
                                CvInvoke.Imwrite(outputFilePath, croppedFace);
                            }
                        }
                    }
                }
            }
        }
    }
}
