using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    public class Path
    {
        public List<(int, int)> pathCells;
        public readonly int pathId;
        private static int runningId=0;

        public Path()
        {
            pathId = runningId++;
            pathCells = new List<(int, int)>();
        }   
    }
}
