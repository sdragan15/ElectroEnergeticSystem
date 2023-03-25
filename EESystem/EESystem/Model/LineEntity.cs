using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EESystem.Model
{
    public class LineEntity
    {
        private long firstEnd;
        private long secondEnd;

        public long FirstEnd { get => firstEnd; set => firstEnd = value; }
        public long SecondEnd { get => secondEnd; set => secondEnd = value; }
    }
}
