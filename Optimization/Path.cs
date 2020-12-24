using System.Collections.Generic;

namespace Optimization
{
    public class Path
    {
        public List<(int, int)> pathCells;
        public readonly int pathId;
        private static int runningId = 0;

        public Path()
        {
            pathId = runningId++;
            pathCells = new List<(int, int)>();
        }
    }
}
