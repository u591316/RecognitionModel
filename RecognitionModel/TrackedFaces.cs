using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognitionModel
{
    internal class TrackedFaces
    {
        public Rectangle FaceRectangle { get; set; }
        public int Label { get; set; }
        /// <summary>
        /// Number of frames a detection is away from frame
        /// </summary>
        public int FrameCounter { get; set; }   
        public int LastDrawnLabel { get; set; }

        private static List<UnknownFace> _trackedFaces = new List<UnknownFace>();
        public int UnknownCounter { get; set; }


    }
}
