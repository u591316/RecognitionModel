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
    /// <summary>
    /// 'FaceUtils' is used to crop faces from raw images provided in a folder.
    /// Uses CascadeClassifier to detect faces in a picture.
    /// </summary>
    public class FaceUtils
    {
        private static CascadeClassifier faceDetector = new CascadeClassifier(@"C:\Users\peder\source\repos\RecognitionModel\RecognitionModel\haarcascade_frontalface_alt.xml");

        /// <summary>
        /// Crops faces from the input image and saves them in the specified output folder.
        /// </summary>
        /// <param name="imagePath">The path of the input image containing faces.</param>
        /// <param name="outputFolderPath">the path ouf the output fodlder where cropped faces will be saved(Used for training the recognizer)</param>
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
                        Rectangle[] faces = faceDetector.DetectMultiScale(image, 1.1, 10, new Size(50, 50)); 

                        // Crop and save each face
                        foreach (Rectangle face in faces)
                        {
                            using (Mat croppedFace = new Mat(gray, face)) //Test image and gray for comparing result
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
