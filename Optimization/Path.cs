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

        public Path()
        {
            pathCells = new List<(int, int)>();
        }

        public Path(List<(int, int)> pathCells)
        {
            this.pathCells = pathCells;
        }

        public int GetPathSize()
        {
            return pathCells.Count;
        }
    }
}
