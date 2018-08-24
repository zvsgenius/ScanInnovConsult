using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanInnov
{
    class Comment
    {
        public int begin { get; private set; }
        public int length { get; private set; }

        public Comment(int begin, int length)
        {
            this.begin = begin;
            this.length = length;
        }
    }
}
