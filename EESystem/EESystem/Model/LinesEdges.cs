using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EESystem.Model
{
    public class LinesEdges
    {
        public NodeEntity FirstNode { get; set; }
        public NodeEntity LastNode { get; set; }

        public LinesEdges()
        {

        }

        public LinesEdges(NodeEntity first, NodeEntity last)
        {
            FirstNode = first;
            LastNode = last;
        }
    }
}
