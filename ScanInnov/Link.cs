using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanInnov
{
    class Link
    {
        public bool NewLink { get; set; }

        public string link { get; private set; }

        public Link(string link)
        {
            this.link = link;
            NewLink = true;
        }
    }
}
