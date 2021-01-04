using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Optimization
{
    class SimulationInitializer
    {

        private static readonly string gridFilePath = System.IO.Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Grid.json");
        private static readonly string pStarFilePath = System.IO.Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "PStar2.json");
        private static string qFilePath = System.IO.Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Q.json");

        internal static void InitSimulation()
        {
            bool shouldGetConfigData = bool.Parse(ConfigurationManager.AppSettings["getDataFromConfig"]);

            if (shouldGetConfigData)
                setDataFromConfig();
            else
                setDataFromUser();
        }

        internal static void WriteQ(List<(double, double)> q, int qIndex = 0)
        {
            var filePath = qIndex == 0 ? qFilePath : string.Concat(qFilePath.Split('.')[0], qIndex, '.', qFilePath.Split('.')[1]);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(q));
        }
        public static void Temp(int qIndex = 0)
        {
            Console.WriteLine(string.Concat(qFilePath.Split('.')[0], qIndex, '.', qFilePath.Split('.')[1]));
        }

        internal static void WritePStar(List<(double, double)> pStar)
        {
            File.WriteAllText(pStarFilePath, JsonConvert.SerializeObject(pStar));
        }

        private static void setDataFromUser()
        {
            SimulationData simulationData = SimulationData.Instance;

            Console.WriteLine("Enter the size of the simulation board");
            simulationData.SimulationBoardSize = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the amount of obstacles to create");
            simulationData.SimulationGrid = new Grid(simulationData.SimulationBoardSize, int.Parse(Console.ReadLine()));

            Console.WriteLine("Should export grid to file? Y/N");
            if (Console.ReadLine() == "Y")
                File.WriteAllText(gridFilePath, JsonConvert.SerializeObject(simulationData.SimulationGrid));

            Console.WriteLine("Enter the size of the population to work with");
            simulationData.PopulationSize = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter source cell (x,y)");
            simulationData.SourceCell = GetCellFromString(Console.ReadLine());

            Console.WriteLine("Enter destination cell (x,y)");
            simulationData.DestinationCell = GetCellFromString(Console.ReadLine());


            Console.WriteLine("Enter amount of generation to run the simulation");
            simulationData.GenerationAmount = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter Mutation Probability for the simulation (0-1)");
            simulationData.MutationProbability = double.Parse(Console.ReadLine());
        }

        private static (int, int) GetCellFromString(string cellString)
        {
            var cellArray = cellString.Split(',');
            int x = int.Parse(cellArray[0].Substring(1));
            int y = int.Parse(cellArray[1].Substring(0, cellArray[1].Length - 1));

            return (x, y);
        }
        private static void setDataFromConfig()
        {
            SimulationData simulationData = SimulationData.Instance;
            simulationData.PopulationSize = int.Parse(ConfigurationManager.AppSettings["initialPopulationSize"]);
            simulationData.GenerationAmount = int.Parse(ConfigurationManager.AppSettings["generationAmount"]);
            simulationData.MutationProbability = double.Parse(ConfigurationManager.AppSettings["mutationProbability"]);
            simulationData.SourceCell = GetCellFromString(ConfigurationManager.AppSettings["sourceCell"]);
            simulationData.DestinationCell = GetCellFromString(ConfigurationManager.AppSettings["destinationCell"]);
            simulationData.SimulationGrid = JsonConvert.DeserializeObject<Grid>(File.ReadAllText(gridFilePath));
            simulationData.SimulationBoardSize = simulationData.SimulationGrid.size;
            simulationData.GeneratePStar = bool.Parse(ConfigurationManager.AppSettings["createPStar"]);

        }
    }
}
