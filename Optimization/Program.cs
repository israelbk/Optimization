using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the size of the simulation board");
            int boardSize = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the amount of obstacles to create");
            int obstaclesAmount = int.Parse(Console.ReadLine());
            Grid simulationGrid = new Grid(boardSize, obstaclesAmount);
            Printer.PrintGrid(simulationGrid);

            Console.WriteLine("Enter the size of the population to work with");
            int initialPopulationSize = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter source cell (x,y)");
            string sourceString = Console.ReadLine();
            var sourceArray = sourceString.Split(',');
            int xSource = int.Parse(sourceArray[0].Substring(1));
            int ySource = int.Parse(sourceArray[1].Substring(0, sourceArray[1].Length-1));

            Console.WriteLine("Enter destination cell (x,y)");
            string destString = Console.ReadLine();
            var destArray = destString.Split(',');
            int xDest = int.Parse(destArray[0].Substring(1));
            int yDest = int.Parse(destArray[1].Substring(0, destArray.Length - 1));

            (int, int) source = (xSource, ySource);
            (int, int) destination = (xDest, yDest);

            MooAlgorithm alg = new MooAlgorithm(initialPopulationSize, source, destination, simulationGrid);
        }
    }
}
