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
            SimulationInitializer.InitSimulation();
            Printer.PrintGrid();
            MooAlgorithm alg = new MooAlgorithm();
        }
    }
}
