

using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;

namespace Optimization
{
    class SimulationInitializer
    {

        private static string gridFilePath = System.IO.Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Grid.json");

        internal static void InitSimulation()
        {
            bool shouldGetConfigData = bool.Parse(ConfigurationManager.AppSettings["getDataFromConfig"]);

            if (shouldGetConfigData)
                setDataFromConfig();
            else
                setDataFromUser();
        }

        private static void setDataFromUser()
        {
            SimulationData simulationData = SimulationData.Instance;

            Console.WriteLine("Enter the size of the simulation board");
            simulationData.simulationBoardSize = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the amount of obstacles to create");
            simulationData.obstaclePrecentageCreation = int.Parse(Console.ReadLine());
            simulationData.simulationGrid = new Grid(simulationData.simulationBoardSize, simulationData.obstaclePrecentageCreation);

            Console.WriteLine("Should export grid to file? Y/N");
            if(Console.ReadLine() == "Y")
                File.WriteAllText(gridFilePath, JsonConvert.SerializeObject(simulationData.simulationGrid));

            Console.WriteLine("Enter the size of the population to work with");
            simulationData.initialPopulationSize = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter source cell (x,y)");
            simulationData.sourceCell = GetCellFromString(Console.ReadLine());

            Console.WriteLine("Enter destination cell (x,y)");
            simulationData.destinationCell = GetCellFromString(Console.ReadLine());


            Console.WriteLine("Enter amount of generation to run the simulation");
            simulationData.generationAmount = int.Parse(Console.ReadLine());
        }

        private static (int,int) GetCellFromString(string cellString)
        {
            var cellArray = cellString.Split(',');
            int x = int.Parse(cellArray[0].Substring(1));
            int y = int.Parse(cellArray[1].Substring(0, cellArray.Length - 1));

            return (x, y);
        }

        private static void setDataFromConfig()
        {
            SimulationData simulationData = SimulationData.Instance;
            simulationData.obstaclePrecentageCreation = int.Parse(ConfigurationManager.AppSettings["obstaclePrecentageCreation"]);
            simulationData.simulationGrid = new Grid(simulationData.simulationBoardSize, simulationData.obstaclePrecentageCreation);
            simulationData.initialPopulationSize = int.Parse(ConfigurationManager.AppSettings["initialPopulationSize"]);
            simulationData.generationAmount = int.Parse(ConfigurationManager.AppSettings["generationAmount"]);
            simulationData.sourceCell = GetCellFromString(ConfigurationManager.AppSettings["sourceCell"]); 
            simulationData.destinationCell = GetCellFromString(ConfigurationManager.AppSettings["destinationCell"]);
            simulationData.simulationGrid = JsonConvert.DeserializeObject<Grid>(File.ReadAllText(gridFilePath));
            simulationData.simulationBoardSize = simulationData.simulationGrid.size;
        }
    }
}
