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


        public FaceDatasetManager(string rawPhotosPath, string croppedPhotosPath, string labelsFileName)
        {
            _rawPhotosPath = rawPhotosPath;
            _croppedPhotosPath = croppedPhotosPath;
            _labelsFilePath = Path.Combine(croppedPhotosPath, labelsFileName);
        }

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
