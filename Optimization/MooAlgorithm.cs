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

            Printer.PrintMetaData(initalPopulationTime);
        }

        internal void RunSimulation()
        {
            // Validate simulation data.
            if (IsCellOutOfBounds(SimulationData.Instance.SourceCell) ||
                         IsCellOutOfBounds(SimulationData.Instance.SourceCell) ||
                         SimulationData.Instance.SimulationGrid.GetCellWD(SimulationData.Instance.SourceCell) == 1 ||
                         SimulationData.Instance.SimulationGrid.GetCellWD(SimulationData.Instance.DestinationCell) == 1)
            {
                Console.WriteLine("Source Or destination cells are not valid, exiting...");
                return;
            }

            int generationCount = SimulationData.Instance.GenerationAmount;
            while (generationCount-- > 0)
            {
                if (generationCount % 10 == 0)
                    Console.WriteLine($"Iteration: {SimulationData.Instance.GenerationAmount - generationCount}");
                PathRepair();
                EvaluatePaths();
                CalcRank();
                if (generationCount == 0)
                    Printer.PrintPaths();
                GeneticOperators(SelectionMechanism());
            }

            //CalcRank();
            //Printer.PrintPaths();

        }

        private void CalcRank()
        {
            SimulationData.Instance.Ranks = new Dictionary<int, int>();

            foreach (var fitness in SimulationData.Instance.Fitnesses)
            {
                // Reset old ranks.
                int dominatedByCount = 0;
                foreach (var otherPathFitness in SimulationData.Instance.Fitnesses.Values)
                    if (otherPathFitness.f1 > fitness.Value.f1 && otherPathFitness.f2 >= fitness.Value.f2 || otherPathFitness.f2 > fitness.Value.f2 && otherPathFitness.f1 >= fitness.Value.f1)
                        dominatedByCount++;
                SimulationData.Instance.Ranks.Add(fitness.Key, SimulationData.Instance.PopulationSize - dominatedByCount);
            }
        }

        private void GeneticOperators(List<Path> selectedPath)
        {
            // Resets the population to contain only the winners of the tournament selection.
            SimulationData.Instance.PopulationPaths = new List<Path>(selectedPath);

            for (int i = 0; i < selectedPath.Count; i += 2)
            {
                SimulationData.Instance.PopulationPaths.AddRange(MutatePaths(GetNewPathsGenetically(selectedPath[i], selectedPath[i + 1])));
            }
        }

        /// <summary>
        /// Finds all mutual cells of the paths, selects one randomally, and generate two new path by
        /// replacing the half paths.
        /// for example:
        /// for the paths.
        /// (1,1)->(1,2)->(1,3)->(2,3)->(3,4)-(4,4)
        /// (1,1)->(2,2)->(2,3)->(3,3)->(3,4)-(4,4)
        /// we will randomise (2,3) & (3,4), assuming that (2,3) is selected as turning point the new child path will be:
        /// (1,1)->(1,2)->(1,3)->(2,3)->(3,3)->(3,4)-(4,4) 
        /// (1,1)->(2,2)->(2,3)->(3,4)-(4,4)
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        private IEnumerable<Path> GetNewPathsGenetically(Path path1, Path path2)
        {
            List<(int, int)> mutualIndexes = GetMutualPathsCells(path1, path2);
            // No common cells
            if (mutualIndexes.Count == 0)
                return GetMonotoneGeneticPaths(path1, path2);

            // Else, there is connection spots between the cells.
            var randomMutualCell = mutualIndexes.ElementAt(random.Next(mutualIndexes.Count));
            var path1MutualIndex = path1.pathCells.IndexOf(randomMutualCell);
            var path2MutualIndex = path2.pathCells.IndexOf(randomMutualCell);

            List<Path> childPaths = new List<Path>();
            Path newPath1 = new Path();
            Path newPath2 = new Path();

            var path1FirstHalf = path1.pathCells.Take(path1MutualIndex);
            var path1SecondHalf = path1.pathCells.Skip(path1MutualIndex);
            var path2FirstHalf = path2.pathCells.Take(path2MutualIndex);
            var path2SecondHalf = path2.pathCells.Skip(path2MutualIndex);

            newPath1.pathCells.AddRange(path1FirstHalf);
            newPath1.pathCells.AddRange(path2SecondHalf);

            newPath2.pathCells.AddRange(path2FirstHalf);
            newPath2.pathCells.AddRange(path1SecondHalf);

            childPaths.Add(newPath1);
            childPaths.Add(newPath2);

            return childPaths;
        }


        /// <summary>
        /// Selectd a the midlle of the two path, and randomly connects the halfs. 
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        private IEnumerable<Path> GetMonotoneGeneticPaths(Path path1, Path path2)
        {
            List<Path> childPaths = new List<Path>();
            Path newPath1 = new Path();
            Path newPath2 = new Path();

            var path1FirstHalf = path1.pathCells.Take(path1.pathCells.Count / 2);
            var path1SecondHalf = path1.pathCells.Skip(path1.pathCells.Count / 2);
            var path2FirstHalf = path2.pathCells.Take(path2.pathCells.Count / 2);
            var path2SecondHalf = path2.pathCells.Skip(path2.pathCells.Count / 2);

            newPath1.pathCells.AddRange(path1FirstHalf);
            newPath1.pathCells.AddRange(PathGenerator.Instance.ConnectCellsRandomally(path1FirstHalf.Last(), path2SecondHalf.First()));
            path2SecondHalf.ToList().RemoveAt(0);
            newPath1.pathCells.AddRange(path2SecondHalf);

            newPath2.pathCells.AddRange(path2FirstHalf);
            newPath2.pathCells.AddRange(PathGenerator.Instance.ConnectCellsRandomally(path2FirstHalf.Last(), path1SecondHalf.First()));
            path1SecondHalf.ToList().RemoveAt(0);
            newPath2.pathCells.AddRange(path1SecondHalf);

            childPaths.Add(newPath1);
            childPaths.Add(newPath2);

            return childPaths;
        }

        private List<(int, int)> GetMutualPathsCells(Path path1, Path path2)
        {
            List<(int, int)> mutualPathsCells = new List<(int, int)>();
            foreach (var cell in path1.pathCells)
            {
                // Not adding source and destination cells.
                if (cell == SimulationData.Instance.SourceCell || cell == SimulationData.Instance.DestinationCell)
                    continue;
                foreach (var otherPathCell in path2.pathCells)
                    if (cell == otherPathCell)
                        mutualPathsCells.Add(cell);
            }
            return mutualPathsCells;
        }

        /// <summary>
        /// Mutation works as following:
        /// with a probability of (MutationProbability) if a path is selected to be mutated, we choose two random points in the path
        /// and connects them randomly so a new path is generated and we avoid finding the local optimum. 
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private IEnumerable<Path> MutatePaths(IEnumerable<Path> paths)
        {
            List<Path> mutatedPaths = new List<Path>();
            foreach (var path in paths)
            {
                // Should mutate.
                if (random.NextDouble() < SimulationData.Instance.MutationProbability)
                {
                    // Selects randomally cell to mutate.
                    int mutatedCellFromIndex = random.Next(path.pathCells.Count - 1);
                    int mutatedCellToIndex = random.Next(mutatedCellFromIndex, path.pathCells.Count);

                    Path mutatedPath = new Path();
                    // Copies all the cell untill the mutation.
                    mutatedPath.pathCells.AddRange(path.pathCells.GetRange(0, mutatedCellFromIndex));
                    // Randomlly connects the mutation points.
                    mutatedPath.pathCells.AddRange(PathGenerator.Instance.ConnectCellsRandomally(path.pathCells[mutatedCellFromIndex], path.pathCells[mutatedCellToIndex]));
                    mutatedPath.pathCells.AddRange(path.pathCells.GetRange(mutatedCellToIndex, path.pathCells.Count - mutatedCellToIndex));
                    mutatedPaths.Add(mutatedPath);
                }
                else
                    mutatedPaths.Add(path);
            }
            return mutatedPaths;
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
                    selectedPaths.Add(GetPathById(pathsKeys[i + 1]));
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

            // OutboundCell repair
            for (int i = 0; i < path.pathCells.Count; i++)
            {
                if (IsCellOutOfBounds(path.pathCells[i]))
                {
                    int lastCellInBound = i - 1;
                    // Run while we still out of bounds or items left.
                    while (i < path.pathCells.Count - 1 && IsCellOutOfBounds(path.pathCells[++i])) ;
                    tempRepairedPath.pathCells.AddRange(PathGenerator.Instance.ConnectCellsMonotony(path.pathCells[lastCellInBound], path.pathCells[i]));
                }
                else
                    tempRepairedPath.pathCells.Add(path.pathCells[i]);
            }

            // Connects items to source (if it's already connected nothig will happen).
            if (SimulationData.Instance.SourceCell != tempRepairedPath.pathCells.First())
            {
                var oldFirst = tempRepairedPath.pathCells.First();
                tempRepairedPath.pathCells.Insert(0, SimulationData.Instance.SourceCell);
                tempRepairedPath.pathCells.InsertRange(0, PathGenerator.Instance.ConnectCellsMonotony(SimulationData.Instance.SourceCell, tempRepairedPath.pathCells.First()));
                tempRepairedPath.pathCells.Remove(oldFirst);
            }
            // Connects last item to destination (if it's already connected nothig will happen).
            tempRepairedPath.pathCells.AddRange(PathGenerator.Instance.ConnectCellsMonotony(tempRepairedPath.pathCells.Last(), SimulationData.Instance.DestinationCell));

            Path finalRepairedPath = new Path();

            // ObstacleRepair
            for (int i = 0; i < tempRepairedPath.pathCells.Count; i++)
            {
                // Cell is obstacle.
                if (!IsCellOutOfBounds(tempRepairedPath.pathCells[i]) && SimulationData.Instance.SimulationGrid.GetCellWD(tempRepairedPath.pathCells[i]) == 1)
                {
                    if (i + 1 == tempRepairedPath.pathCells.Count)
                        throw new Exception("Cannot access out of bounds cells");
                    finalRepairedPath.pathCells.AddRange(PathGenerator.Instance.BypassObstacle(tempRepairedPath.pathCells[i - 1], tempRepairedPath.pathCells[i + 1], tempRepairedPath.pathCells[i]));
                    i++;
                    continue;
                }

                // Else all is normal.
                finalRepairedPath.pathCells.Add(tempRepairedPath.pathCells[i]);
            }

            return finalRepairedPath;
        }

        private bool IsCellOutOfBounds((int x, int y) cell)
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
                if (IsCellOutOfBounds(cell))
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
