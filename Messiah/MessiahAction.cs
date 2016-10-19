using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messiah
{
    class MessiahAction
    {
        public string[] Alternatives;

        /// <summary>
        /// Initializes a new object of the MessiahAction class.
        /// </summary>
        /// <param name="strAlternatives">strings that represent this action</param>
        public MessiahAction(params string[] strAlternatives)
        {
            Alternatives = strAlternatives;
        }
    }
}
