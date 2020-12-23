using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    class MooAlgorithm
    {
        private int populationSize { get; set; }
        List<Path> pathsPopulation { get; set; }

        public MooAlgorithm()
        {
            populationSize = SimulationData.Instance.initialPopulationSize;
            SetInitialPopulation(SimulationData.Instance.sourceCell, SimulationData.Instance.destinationCell);
            //Printer.PrintPaths(pathsPopulation);

            var watch = new System.Diagnostics.Stopwatch();

            Console.WriteLine("Execution time messurtment started");

            watch.Start();

            foreach (Path path in pathsPopulation)
            {
                GetFitnessEvaluation(path, SimulationData.Instance.simulationGrid);
            }

            watch.Stop();

            Console.WriteLine($"Execution time is {watch.ElapsedMilliseconds}");
        }

        private void SetInitialPopulation((int, int) sIndex, (int, int) dIndex)
        {
            pathsPopulation = PathGenerator.Instance.GetInitialPaths(sIndex, dIndex, populationSize);
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
