using Emgu.CV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognitionModel
{
    internal class FaceDatasetManager
    {
        private string _rawPhotosPath;
        private string _croppedPhotosPath;
        private string _labelsFilePath; 

        /// <summary>
        /// Initializes a new instance of the FaceDatasetManager class with specified paths.
        /// </summary>
        /// <param name="rawPhotosPath">The path of the raw photos directory. Pictures to be prepared for the recognizer for training</param>
        /// <param name="croppedPhotosPath">The path of the cropped/prepared photos directory</param>
        /// <param name="labelsFileName">The file name for the label-to-name mapping file.</param>
        public FaceDatasetManager(string rawPhotosPath, string croppedPhotosPath, string labelsFileName)
        {
            _rawPhotosPath = rawPhotosPath;
            _croppedPhotosPath = croppedPhotosPath;
            _labelsFilePath = Path.Combine(croppedPhotosPath, labelsFileName);
        }

        /// <summary>
        /// Processes raw photos by detecting and cropping faces and saving the cropped faces in seperate folders for each person
        /// </summary>
        public void ProcessRawPhotos()
        {
            foreach(string personFolder in Directory.GetDirectories(_rawPhotosPath))
            {
                string personName = Path.GetFileName(personFolder);
                string personCroppedFolderPath = Path.Combine(_croppedPhotosPath, personName);

                Directory.CreateDirectory(personCroppedFolderPath);

                foreach(string imagePath in Directory.GetFiles(personFolder))
                {
                    Mat image = new Mat(imagePath);
                    FaceUtils.CropFaces(imagePath, personCroppedFolderPath);
                }
            }
        }

        /// <summary>
        /// Saves the label-to-name mapping to a file 'label.txt'
        /// </summary>
        /// <param name="labelToName"></param>
        public void SaveLabelToNameMap(Dictionary<int, string> labelToName)
        {
            using (StreamWriter file = new StreamWriter(_labelsFilePath))
            {
                foreach (var entry in labelToName)
                {
                    file.WriteLine($"{entry.Key},{entry.Value}");
                }
            }
        }

        /// <summary>
        /// Loads the label-to-name mapping from a file and returns a dictionary containing the mappings
        /// </summary>
        /// <returns>Dictionary<int, string> labelToName</returns>
        public Dictionary<int, string> LoadLabelToNameMap()
        {
            Dictionary<int, string> labelToName = new Dictionary<int, string>();

            if (File.Exists(_labelsFilePath))
            {
                using (StreamReader file = new StreamReader(_labelsFilePath))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] parts = line.Split(',');
                        int label = int.Parse(parts[0]);
                        string name = parts[1];
                        labelToName[label] = name;
                    }
                }
            }

            return labelToName;
        }
    }
}
