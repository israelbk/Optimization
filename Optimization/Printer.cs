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

        public static void PrintMetaData()
        {
            // Prints the source and destination of the path.
            Console.WriteLine("\n");
            Console.WriteLine($"Calculating path from " +
               $"({SimulationData.Instance.SourceCell.Item1},{SimulationData.Instance.SourceCell.Item2})" +
               $" to ({SimulationData.Instance.DestinationCell.Item1},{SimulationData.Instance.DestinationCell.Item2})");
            Console.WriteLine("\n");

        }

        public static void PrintPaths(List<Path> paths)
        {
            int i = 0;
            foreach (var path in paths)
            {
                Console.WriteLine();
                Console.Write(i++ + ": ");
                foreach ((int x, int y) cell in path.pathCells)
                {
                    Console.Write($"({cell.x},{cell.y})>");
                }
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
