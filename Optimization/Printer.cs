using System;
using System.Collections.Generic;

namespace Optimization
{
    static class Printer
    {
        public static void PrintGrid()
        {
            Grid grid = SimulationData.Instance.SimulationGrid;
            Console.WriteLine();
            Console.Write('\t');
            for (int i = 0; i < grid.size; i++)
            {
                Console.Write('\t');
                Console.Write("  " + i);
            }
            Console.WriteLine();

            Console.Write("\t\t");
            for (int i = 0; i < grid.size; i++)
            {
                Console.Write("________");
            }
            Console.WriteLine();

            for (int i = 0; i < grid.size; i++)
            {
                Console.Write('\t');
                Console.Write(i);
                Console.Write('\t');
                Console.Write('|');
                for (int j = 0; j < grid.size; j++)
                {
                    if (grid.GetCellWD((i, j)) != 0 && grid.GetCellWD((i, j)) != 1)
                        Console.Write(grid.GetCellWD((i, j)).ToString().Substring(1) + "\t");
                    else
                        Console.Write(grid.GetCellWD((i, j)) + "\t");
                }
                Console.Write('|');
                Console.WriteLine();
            }

            Console.Write("\t\t");
            for (int i = 0; i < grid.size; i++)
            {
                Console.Write("________");
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

        }

        public static void PrintMetaData(long initalPopulationTime)
        {
            // Prints the source and destination of the path.
            Console.WriteLine("\n");
            Console.WriteLine($"Calculating path from " +
               $"({SimulationData.Instance.SourceCell.Item1},{SimulationData.Instance.SourceCell.Item2})" +
               $" to ({SimulationData.Instance.DestinationCell.Item1},{SimulationData.Instance.DestinationCell.Item2})");
            Console.WriteLine("\n");
            Console.WriteLine($"initialization time for {SimulationData.Instance.PopulationSize} paths took {initalPopulationTime} ms");
            Console.WriteLine("\n");

        }

        internal static void PrintAverageFitness()
        {
            int i = 0;
            double sumF1 = 0;
            double sumF2 = 0;
            foreach (var fitness in SimulationData.Instance.Fitnesses)
            {
                //Console.WriteLine($"path: {i} f1-{fitness.Value.f1}, f2-{fitness.Value.f2}");
                sumF1 += fitness.Value.f1;
                sumF2 += fitness.Value.f2;
            }
            Console.WriteLine($"Average fitness: f1-{sumF1 / SimulationData.Instance.PopulationSize}, f2-{sumF2 / SimulationData.Instance.PopulationSize}");
        }

        public static void PrintPaths()
        {
            int i = 0;
            foreach (var path in SimulationData.Instance.PopulationPaths)
            {
                Console.WriteLine();
                Console.Write(i++ + ": ");
                foreach ((int x, int y) cell in path.pathCells)
                {
                    Console.Write($"({cell.x},{cell.y})>");
                }
                Console.Write($" Rank: {SimulationData.Instance.Ranks[path.pathId]}");
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
