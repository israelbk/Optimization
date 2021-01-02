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
                //if (generationCount % 10 == 0)
                //    Console.WriteLine($"Iteration: {SimulationData.Instance.GenerationAmount - generationCount}");
                PathRepair();
                EvaluatePaths();
                CalcRank();
                GeneticOperators(SelectionMechanism());
                MutatePaths();
                if (generationCount % 10 == 0)
                {
                    EvaluatePaths();
                    Printer.PrintAverageFitness();
                }
                if (generationCount % 100 == 0)
                {
                    CalcRank();
                    Printer.PrintPaths();
                }
            }

        }

        private bool isIncorrectPath(Path path)
        {

            for (int i = 0; i < path.pathCells.Count - 1; i++)
            {
                if (Math.Abs(path.pathCells[i].Item1 - path.pathCells[i + 1].Item1) > 1 || Math.Abs(path.pathCells[i].Item2 - path.pathCells[i + 1].Item2) > 1)
                    return true;
            }

            return false;
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
            List<Path> potentialNewPaths = new List<Path>();
            for (int i = 0; i < selectedPath.Count; i += 2)
                potentialNewPaths.AddRange(GetNewPathsGenetically(selectedPath[i], selectedPath[i + 1]));

            var shuffeldPotentialNewPaths = potentialNewPaths.ToArray().OrderBy(c => random.Next()).ToList();

            foreach (var potentialPath in shuffeldPotentialNewPaths)
            {
                // The child is new path.
                if (!IsAnyPathAheadEqual(potentialPath, 0))
                {
                    SimulationData.Instance.PopulationPaths.Add(potentialPath);
                    // we filled the population path.
                    if (SimulationData.Instance.PopulationPaths.Count() == SimulationData.Instance.PopulationSize)
                        return;
                }
            }

            // Fill up the rest of the path and it will be mutated since it's equal to other paths.
            SimulationData.Instance.PopulationPaths.AddRange(shuffeldPotentialNewPaths.Take(SimulationData.Instance.PopulationSize - SimulationData.Instance.PopulationPaths.Count));
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
        private List<Path> GetNewPathsGenetically(Path path1, Path path2)
        {
            List<(int, int)> pathsMutualCells = GetMutualPathsCells(path1, path2);
            List<(int, int)> filteredMutualCells = filterMutalCellByDistinctChilden(path1, path2, pathsMutualCells);
            // No common cells
            if (filteredMutualCells.Count == 0)
                return GetMonotoneGeneticPaths(path1, path2);

            List<Path> childPaths = new List<Path>();

            foreach (var mutualCell in filteredMutualCells)
            {
                Path newPath1 = new Path();
                Path newPath2 = new Path();

                var path1MutualIndex = path1.pathCells.IndexOf(mutualCell);
                var path2MutualIndex = path2.pathCells.IndexOf(mutualCell);

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
            }

            return childPaths;
        }

        private List<(int, int)> filterMutalCellByDistinctChilden(Path path1, Path path2, List<(int, int)> pathsMutualCells)
        {
            List<(int, int)> distinctMutualCells = new List<(int, int)>();
            bool firstHalfDistincted, secondHalfDistincted;

            foreach (var mutualCell in pathsMutualCells)
            {
                firstHalfDistincted = false;
                secondHalfDistincted = false;

                int index1 = path1.pathCells.IndexOf(mutualCell);
                int index2 = path2.pathCells.IndexOf(mutualCell);

                if (index1 != index2)
                    firstHalfDistincted = true;

                else
                {
                    for (int i = 0; i < index1; i++)
                    {
                        if (path1.pathCells[i] != path2.pathCells[i])
                            firstHalfDistincted = true;
                    }
                }

                if (path1.pathCells.Count - index1 != path2.pathCells.Count - index2)
                    secondHalfDistincted = true;
                else
                {
                    for (int i = 0; i < path1.pathCells.Count - index1; i++)
                    {
                        if (path1.pathCells[index1 + i] != path2.pathCells[index2 + i])
                            secondHalfDistincted = true;
                    }
                }

                if (firstHalfDistincted && secondHalfDistincted)
                {
                    distinctMutualCells.Add(mutualCell);
                    continue;
                }
            }

            return distinctMutualCells;
        }

        private bool ArePathsEqual(Path path1, Path path2)
        {
            if (path1.pathCells.Count != path2.pathCells.Count)
                return false;
            for (int i = 0; i < path1.pathCells.Count; i++)
            {
                if (!AreCellsEqual(path1.pathCells[i], path2.pathCells[i]))
                    return false;
            }
            return true;
        }

        private bool AreCellsEqual((int x, int y) path1, (int x, int y) path2)
        {
            return (path1.x == path2.x && path1.y == path2.y);
        }


        /// <summary>
        /// Selectd a the midlle of the two path, and randomly connects the halfs. 
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        private List<Path> GetMonotoneGeneticPaths(Path path1, Path path2)
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
                {
                    if (cell == otherPathCell)
                    {
                        mutualPathsCells.Add(cell);
                        continue;
                    }
                }
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
        private void MutatePaths()
        {
            List<Path> mutatedPaths = new List<Path>();
            int mutationCount = 0;
            int pathIndex = 0;
            foreach (var path in SimulationData.Instance.PopulationPaths)
            {
                pathIndex++;
                // Should mutate.
                if (random.NextDouble() < SimulationData.Instance.MutationProbability)
                {
                    mutatedPaths.Add(MutuateSinglePath(path, randomaly: true));
                }
                else if (IsAnyPathAheadEqual(path, pathIndex))
                {
                    mutatedPaths.Add(MutuateSinglePath(path, randomaly: false));
                }
                else
                    mutatedPaths.Add(path);
            }
            SimulationData.Instance.PopulationPaths = mutatedPaths;
        }

        private Path MutuateSinglePath(Path path, bool randomaly)
        {
            // Selects randomally cell to mutate.
            int mutatedCellFromIndex = random.Next(1, path.pathCells.Count - 1);
            int mutatedCellToIndex = random.Next(mutatedCellFromIndex, path.pathCells.Count);

            Path mutatedPath = new Path();
            // Copies all the cell untill the mutation.
            mutatedPath.pathCells.AddRange(path.pathCells.GetRange(0, mutatedCellFromIndex));
            // Randomlly connects the mutation points.
            if (randomaly)
                mutatedPath.pathCells.AddRange(PathGenerator.Instance.ConnectCellsRandomally(path.pathCells[mutatedCellFromIndex - 1], path.pathCells[mutatedCellToIndex]));
            else
                mutatedPath.pathCells.AddRange(PathGenerator.Instance.ConnectCellsMonotony(path.pathCells[mutatedCellFromIndex - 1], path.pathCells[mutatedCellToIndex]));

            mutatedPath.pathCells.AddRange(path.pathCells.GetRange(mutatedCellToIndex, path.pathCells.Count - mutatedCellToIndex));

            return mutatedPath;
        }

        private bool IsAnyPathAheadEqual(Path currentPath, int pathIndex)
        {
            for (int i = pathIndex; i < SimulationData.Instance.PopulationPaths.Count(); i++)
                if (ArePathsEqual(SimulationData.Instance.PopulationPaths[i], currentPath))
                    return true;
            return false;
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
            Path outboundRepairedPath = new Path();

            // OutboundCell repair
            for (int i = 0; i < path.pathCells.Count; i++)
            {
                if (IsCellOutOfBounds(path.pathCells[i]))
                {
                    int lastCellInBound = i - 1;
                    (int, int) connectToCell;
                    // Run while we still out of bounds or items left.
                    while (i + 1 < path.pathCells.Count && IsCellOutOfBounds(path.pathCells[++i])) ;

                    connectToCell = path.pathCells[i];
                    // Last item is not destination.
                    if (i == path.pathCells.Count - 1 && IsCellOutOfBounds(path.pathCells[i]))
                        connectToCell = SimulationData.Instance.DestinationCell;
                    // Repair connects last cell in bound with the new cell
                    outboundRepairedPath.pathCells.AddRange(PathGenerator.Instance.ConnectCellsMonotony(path.pathCells[lastCellInBound], connectToCell));
                }
                else
                    outboundRepairedPath.pathCells.Add(path.pathCells[i]);
            }

            // Connects items to source (if it's already connected nothig will happen).
            if (SimulationData.Instance.SourceCell != outboundRepairedPath.pathCells.First())
            {
                var oldFirst = outboundRepairedPath.pathCells.First();
                outboundRepairedPath.pathCells.Insert(0, SimulationData.Instance.SourceCell);
                outboundRepairedPath.pathCells.InsertRange(0, PathGenerator.Instance.ConnectCellsMonotony(SimulationData.Instance.SourceCell, oldFirst));
                outboundRepairedPath.pathCells.Remove(oldFirst);
            }
            // Connects last item to destination (if it's already connected nothig will happen).
            outboundRepairedPath.pathCells.AddRange(PathGenerator.Instance.ConnectCellsMonotony(outboundRepairedPath.pathCells.Last(), SimulationData.Instance.DestinationCell));

            Path obstacleRepairedPath = new Path();

            // ObstacleRepair
            for (int i = 0; i < outboundRepairedPath.pathCells.Count; i++)
            {
                if (IsCellOutOfBounds(outboundRepairedPath.pathCells[i]))
                {
                    Console.WriteLine("Should not get here since outbound is already repaired");
                }
                // Cell is obstacle.
                if (!IsCellOutOfBounds(outboundRepairedPath.pathCells[i]) && SimulationData.Instance.SimulationGrid.GetCellWD(outboundRepairedPath.pathCells[i]) == 1)
                {
                    if (i + 1 == outboundRepairedPath.pathCells.Count)
                        throw new Exception("Cannot access out of bounds cells");
                    obstacleRepairedPath.pathCells.AddRange(PathGenerator.Instance.BypassObstacle(outboundRepairedPath.pathCells[i - 1], outboundRepairedPath.pathCells[i + 1], outboundRepairedPath.pathCells[i]));
                    i++;
                    continue;
                }

                // Else all is normal.
                obstacleRepairedPath.pathCells.Add(outboundRepairedPath.pathCells[i]);
            }

            Path loopRepairedPath = new Path();

            // Remove path redundant loops
            for (int i = 0; i < obstacleRepairedPath.pathCells.Count; i++)
            {
                for (int j = i + 1; j < obstacleRepairedPath.pathCells.Count; j++)
                {
                    // We have inner loop that should be removed
                    if (obstacleRepairedPath.pathCells[i] == obstacleRepairedPath.pathCells[j])
                    {
                        // We will skip all the cells between since they are redundant.
                        i = j;
                    }
                }
                loopRepairedPath.pathCells.Add(obstacleRepairedPath.pathCells[i]);
            }

            return loopRepairedPath;
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

            foreach (var cell in pathToEvaluate.pathCells)
            {
                // Cell is out of boundries.
                if (IsCellOutOfBounds(cell))
                {
                    outOfBoundryCellsCount++;
                    pathSumWD += 1;
                }

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
                f1 = (grid.size * grid.size) - pathToEvaluate.pathCells.Count; // (n^2 -c)

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
