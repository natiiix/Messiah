using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messiah
{
    class MessiahAction
    {
        public int Index;
        public string[] Alternatives;

        /// <summary>
        /// Initializes a new object of the MessiahAction class.
        /// </summary>
        /// <param name="id">index assigned to this action</param>
        /// <param name="strAlternatives">strings that represent this action</param>
        public MessiahAction(int id, params string[] strAlternatives)
        {
            Index = id;
            Alternatives = strAlternatives;
        }
    }
}
