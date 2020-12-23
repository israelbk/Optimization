using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    class MooAlgorithm
    {
        List<Path> pathsPopulation { get; set; }
        private Stopwatch watch { get; set; }
        private long initalPopulationTime { get; set; }

        public MooAlgorithm()
        {
            watch = new Stopwatch();
            watch.Start();
            pathsPopulation = PathGenerator.Instance.GetInitialPaths();
            watch.Stop();
            initalPopulationTime = watch.ElapsedMilliseconds;
            watch.Reset();

        }

        private (double, double) GetFitnessEvaluation(Path pathToEvaluate, Grid grid)
        {
            double outOfBoundryCellsCount = 0;
            double obstaclesCellsCount = 0;
            double pathSumWD = 0;
            double f1, f2;

            foreach ((int x, int y) cell in pathToEvaluate.pathCells)
            {
                // Cell is out of boundries.
                if (cell.x >= grid.size || cell.y >= grid.size || cell.x < 0 || cell.y < 0)
                    outOfBoundryCellsCount++;
                else
                {
                    // Cell containing obstacle
                    if (grid.GetCellWD(cell) == 1)
                        obstaclesCellsCount++;

                    // Sum WD of cell.
                    pathSumWD += grid.GetCellWD(cell);
                }
             }

            if (outOfBoundryCellsCount != 0)
                f1 = 1;
            else
            {
                f1 = (grid.size * grid.size) - pathsPopulation.Count; // (n^2 -c)

                if (obstaclesCellsCount != 0)
                    f1 /= (20.0 * obstaclesCellsCount); // (n^2 -c) / (20I)
            }

            f2 = (grid.size * grid.size) - pathSumWD;

            return (f1, f2);
        }


    }
}
