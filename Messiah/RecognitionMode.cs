using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;

namespace Messiah
{
    class RecognitionMode
    {
        public int ModeID;
        public double RequiredConfidence;
        public string[] Phrases;

        public RecognitionMode(int id, double confidence, params string[] phrases)
        {
            ModeID = id;
            RequiredConfidence = confidence;
            Phrases = phrases;
        }

        /// <summary>
        /// Adds more phrases into the string array Phrases.
        /// New phrases are added after already existing phrases.
        /// </summary>
        /// <param name="phrases">phrases to be added</param>
        public void AddPhrases(params string[] phrases)
        {
            int oldLen = Phrases.Length;
            Array.Resize(ref Phrases, oldLen + phrases.Length);

            for(int i = 0; i < phrases.Length; i++)
                Phrases[oldLen + i] = phrases[i];
        }
    }
}
