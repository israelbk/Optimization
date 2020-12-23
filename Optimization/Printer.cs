﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    static class Printer
    {
        public static void PrintGrid()
        {
            Grid grid = SimulationData.Instance.simulationGrid;
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

        public static void PrintPaths(List<Path> paths)
        {
            int i = 0;
            foreach (var path in paths)
            {
                Console.WriteLine();
                Console.Write(i++ + ": ");
                foreach ((int x,int y) cell in path.pathCells)
                {
                    Console.Write($"({cell.x},{cell.y})>");
                }
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
