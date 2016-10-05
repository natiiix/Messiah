using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messiah
{
    class Character
    {
        public string CharString;
        public string[] Alternatives;

        public Character(string strChar, params string[] strAlternatives)
        {
            CharString = strChar;

            if (strAlternatives.Length > 0)
                Alternatives = strAlternatives;
            else
                Alternatives = new string[1] { strChar };
        }
    }
}
