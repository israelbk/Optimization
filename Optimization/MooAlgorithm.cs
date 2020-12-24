using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Optimization
{
    class MooAlgorithm
    {
        private Stopwatch watch { get; set; }
        private long initalPopulationTime { get; set; }
        private Random random;

        public MooAlgorithm()
        {
            random = new Random();
            watch = new Stopwatch();
            watch.Start();
            PathGenerator.Instance.GetInitialPaths();
            watch.Stop();
            initalPopulationTime = watch.ElapsedMilliseconds;
            watch.Reset();

        }

        internal void RunSimulation()
        {
            // Validate simulation data.
            if (isCellOutOfBounds(SimulationData.Instance.SourceCell) ||
                         isCellOutOfBounds(SimulationData.Instance.SourceCell) ||
                         SimulationData.Instance.SimulationGrid.GetCellWD(SimulationData.Instance.SourceCell) == 1 ||
                         SimulationData.Instance.SimulationGrid.GetCellWD(SimulationData.Instance.DestinationCell) == 1)
            {
                Console.WriteLine("Source Or destination cells are not valid, exiting...");
                return;
            }

            int generationCount = SimulationData.Instance.GenerationAmount;
            while (generationCount-- > 0)
            {
                PathRepair();
                EvaluatePaths();
                CalcRank();
                GeneticOperators(SelectionMechanism());
            }
        }

        private void CalcRank()
        {
            foreach (var fitness in SimulationData.Instance.Fitnesses)
            {
                // Reset old ranks.
                SimulationData.Instance.Ranks = new Dictionary<int, int>();
                int dominatedByCount = 0;
                foreach (var otherPathFitness in SimulationData.Instance.Fitnesses.Values)
                    if (otherPathFitness.f1 > fitness.Value.f1 && otherPathFitness.f2 >= fitness.Value.f2 || otherPathFitness.f2 > fitness.Value.f2 && otherPathFitness.f1 >= fitness.Value.f1)
                        dominatedByCount++;
                SimulationData.Instance.Ranks.Add(fitness.Key, SimulationData.Instance.PopulationSize - dominatedByCount);
            }
        }

        private void GeneticOperators(List<Path> selectedPath)
        {
            throw new NotImplementedException();
        }

        private List<Path> SelectionMechanism()
        {
            List<Path> selectedPaths = new List<Path>();
            // Shuffles the paths to achieve tournaament selection.
            var pathsKeys = SimulationData.Instance.Fitnesses.Keys.ToArray().OrderBy(c => random.Next()).ToArray();

            // Iterate over pairs.
            for (int i = 0; i < SimulationData.Instance.PopulationSize; i += 2)
            {
                if (SimulationData.Instance.Ranks[pathsKeys[i]] >= SimulationData.Instance.Ranks[pathsKeys[i + 1]])
                    selectedPaths.Add(GetPathById(pathsKeys[i]));
                else
                    selectedPaths.Add(GetPathById(pathsKeys[i+1]));
            }

            return selectedPaths;
        }

        private void EvaluatePaths()
        {
            // Reset fitnesses.
            SimulationData.Instance.Fitnesses = new Dictionary<int, (double f1, double f2)>();
            foreach (var path in SimulationData.Instance.PopulationPaths)
                SimulationData.Instance.Fitnesses.Add(path.pathId, GetPathFitnessEvaluation(path));
        }

        private void PathRepair()
        {
            List<Path> repairedPopulation = new List<Path>();
            foreach (var path in SimulationData.Instance.PopulationPaths)
            {
                repairedPopulation.Add(RepairSinglePath(path));
            }
            SimulationData.Instance.PopulationPaths = repairedPopulation;
        }

        private Path RepairSinglePath(Path path)
        {
            Path tempRepairedPath = new Path();
            Path finalRepairedPath = new Path();

            // OutboundCell repair
            for (int i = 0; i < path.pathCells.Count; i++)
            {
                if (isCellOutOfBounds(path.pathCells[i]))
                {
                    int lastCellInBound = i - 1;
                    // Run while we still out of bounds or items left.
                    while (i < path.pathCells.Count - 1 && isCellOutOfBounds(path.pathCells[++i])) ;
                    tempRepairedPath.pathCells.AddRange(PathGenerator.Instance.connectCellsMonotony(path.pathCells[lastCellInBound], path.pathCells[i]));
                }
                else
                    tempRepairedPath.pathCells.Add(path.pathCells[i]);
            }

            // Connects last item to destination (if it's already connected nothig will happen).
            tempRepairedPath.pathCells.AddRange(PathGenerator.Instance.connectCellsMonotony(tempRepairedPath.pathCells.Last(), SimulationData.Instance.DestinationCell));


            // ObstacleRepair
            for (int i = 0; i < tempRepairedPath.pathCells.Count; i++)
            {
                // Cell is obstacle.
                if (SimulationData.Instance.SimulationGrid.GetCellWD(tempRepairedPath.pathCells[i]) == 1)
                {
                    finalRepairedPath.pathCells.AddRange(PathGenerator.Instance.BypassObstacle(tempRepairedPath.pathCells[i - 1], tempRepairedPath.pathCells[i + 1], tempRepairedPath.pathCells[i]));
                    i++;
                    continue;
                }

                // Else all is normal.
                finalRepairedPath.pathCells.Add(tempRepairedPath.pathCells[i]);
            }

            return finalRepairedPath;
        }

        private bool isCellOutOfBounds((int x, int y) cell)
        {
            return cell.x >= SimulationData.Instance.SimulationGrid.size ||
                cell.y >= SimulationData.Instance.SimulationGrid.size ||
                cell.x < 0 || cell.y < 0;
        }

        private (double, double) GetPathFitnessEvaluation(Path pathToEvaluate)
        {
            Grid grid = SimulationData.Instance.SimulationGrid;
            double outOfBoundryCellsCount = 0;
            double obstaclesCellsCount = 0;
            double pathSumWD = 0;
            double f1, f2;

            foreach ((int x, int y) cell in pathToEvaluate.pathCells)
            {
                // Cell is out of boundries.
                if (isCellOutOfBounds(cell))
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
                f1 = (grid.size * grid.size) - SimulationData.Instance.PopulationSize; // (n^2 -c)

                if (obstaclesCellsCount != 0)
                    f1 /= (20.0 * obstaclesCellsCount); // (n^2 -c) / (20I)
            }

            f2 = (grid.size * grid.size) - pathSumWD;

            return (f1, f2);
        }

        private Path GetPathById(int id)
        {
            foreach (var path in SimulationData.Instance.PopulationPaths)
                if (path.pathId == id)
                    return path;

            Console.WriteLine($"No corresponding path with id {id} was found");
            return null;
        }
    }
}
