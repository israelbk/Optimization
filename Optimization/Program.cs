using System.Collections.Generic;
using System.Diagnostics;

namespace Optimization
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            SimulationInitializer.InitSimulation();
            Printer.PrintGrid();
            MooAlgorithm alg = new MooAlgorithm();
            if (!SimulationData.Instance.GeneratePStar)
            {
                watch.Start();
                List<(double, double)> unionResults = new List<(double, double)>();
                for (int i = 0; i < 30; i++)
                {
                    System.Console.WriteLine($"simulation #{i + 1} is started");
                    SimulationInitializer.WriteQ(alg.RunSimulation(), (i + 1));
                }
                watch.Stop();
                Printer.PrintSimulationTime(watch.ElapsedMilliseconds);
            }

            else
            {
                watch.Start();
                List<(double, double)> unionResults = new List<(double, double)>();
                for (int i = 0; i < 30; i++)
                {
                    System.Console.WriteLine($"simulation #{i + 1} is started");
                    unionResults.AddRange(alg.RunSimulation());
                }
                List<(double, double)> pStar = alg.GetNondominatedSubgroup(unionResults);
                SimulationInitializer.WritePStar(pStar);
                watch.Stop();
                Printer.PrintPStarTime(watch.ElapsedMilliseconds);
            }

            System.Console.ReadKey();
        }
    }
}
