using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AprioriAllLib
{
    class Transaction2
    {
        public List<string> items=new List<string>();
        public int tid = 0;

        public Transaction2()
        {
            this.items = new List<string>();
            this.tid = 0;
        }

        public Transaction2(int tid, string[] elements)
        {
            foreach(string e in elements)
            {
                this.items.Add(e);
                this.tid = tid;
            }
        }
    }
}
