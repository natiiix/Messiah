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
        public Grammar ModeGrammar;

        public RecognitionMode(int id, double confidence, params string[] phrases)
        {
            ModeID = id;
            RequiredConfidence = confidence;
            ModeGrammar = new Grammar(new GrammarBuilder(new Choices(phrases)));
        }
    }
}
