using System;
using System.Collections.Generic;

namespace Optimization
{
    class PathGenerator
    {
        private Random random;
        private int monotonePathLength;

        private static readonly PathGenerator instance = new PathGenerator();

        private PathGenerator()
        {
            random = new Random();
            monotonePathLength = 0;
        }

        public static PathGenerator Instance { get { return instance; } }

        public void GetInitialPaths()
        {
            Path xMonotone = generateXMonotonePath(SimulationData.Instance.SourceCell, SimulationData.Instance.DestinationCell);
            Path yMonotone = generateYMonotonePath(SimulationData.Instance.SourceCell, SimulationData.Instance.DestinationCell);
            monotonePathLength = xMonotone.pathCells.Count;

            SimulationData.Instance.PathsPopulation.Add(xMonotone);
            SimulationData.Instance.PathsPopulation.Add(yMonotone);

            int randomPathsAmount = SimulationData.Instance.InitialPopulationSize - 2;
            while (randomPathsAmount-- > 0)
                SimulationData.Instance.PathsPopulation.Add(generateRandomPath(SimulationData.Instance.SourceCell, SimulationData.Instance.DestinationCell));
        }

        public List<(int, int)> connectCellsMonotony((int, int) from, (int, int) to, bool? isXmonotone = null)
        {
            if (!isXmonotone.HasValue)
                isXmonotone = random.Next(2) == 1;
            List<(int, int)> pathCells = new List<(int, int)>();

            pathCells.AddRange((bool)isXmonotone ? getMonotonXCells(from, to) : getMonotonYCells(from, to));
            pathCells.AddRange((bool)isXmonotone ? getMonotonYCells(from, to) : getMonotonXCells(from, to));


            return pathCells;
        }

        public List<(int, int)> BypassObstacle((int x, int y) from, (int x, int y) to, (int x, int y) obstacle)
        {
            List<(int, int)> bypassedCells = new List<(int, int)>();
            (int x, int y) currentIndex = from;
            while (!(currentIndex.x == to.x && currentIndex.y == to.y))
            {
                (int x, int y) tempCell = getNextRandomCell(currentIndex, to);
                if (tempCell.x == obstacle.x && tempCell.y == obstacle.y)
                    continue;
                currentIndex = tempCell;
                bypassedCells.Add(currentIndex);
            }
            return bypassedCells;
        }

        private Path generateRandomPath((int x, int y) sIndex, (int x, int y) dIndex)
        {
            var currentIndex = sIndex;
            Path newPath = initializePath(sIndex);

            int maxCellAmount = random.Next(monotonePathLength, monotonePathLength * 2);

            for (int i = 0; i < maxCellAmount; i++)
            {
                currentIndex = getNextRandomCell(currentIndex, dIndex);
                newPath.pathCells.Add(currentIndex);
                if (currentIndex.x == dIndex.x && currentIndex.y == dIndex.y)
                    return newPath;
            }

            return newPath;
        }
        private Path generateXMonotonePath((int x, int y) sIndex, (int x, int y) dIndex)
        {
            Path newPath = initializePath(sIndex);
            newPath.pathCells.AddRange(connectCellsMonotony(sIndex, dIndex, true));
            return newPath;
        }

        private Path generateYMonotonePath((int x, int y) sIndex, (int x, int y) dIndex)
        {
            Path newPath = initializePath(sIndex);
            newPath.pathCells.AddRange(connectCellsMonotony(sIndex, dIndex, false));
            return newPath;
        }

        private List<(int, int)> getMonotonXCells((int x, int y) from, (int x, int y) to)
        {
            var currentIndex = from;
            List<(int, int)> pathCells = new List<(int, int)>();

            while (currentIndex.x < to.x)
            {
                currentIndex = (currentIndex.x + 1, currentIndex.y);
                pathCells.Add(currentIndex);
            }

            while (currentIndex.x > to.x)
            {
                currentIndex = (currentIndex.x - 1, currentIndex.y);
                pathCells.Add(currentIndex);
            }

            return pathCells;
        }

        private List<(int, int)> getMonotonYCells((int x, int y) from, (int x, int y) to)
        {
            var currentIndex = from;
            List<(int, int)> pathCells = new List<(int, int)>();

            while (currentIndex.y < to.y)
            {
                currentIndex = (currentIndex.x, currentIndex.y + 1);
                pathCells.Add(currentIndex);
            }

            while (currentIndex.y > to.y)
            {
                currentIndex = (currentIndex.x, currentIndex.y - 1);
                pathCells.Add(currentIndex);
            }

            return pathCells;
        }

        private Path initializePath((int, int) sIndex)
        {
            Path newPath = new Path();
            newPath.pathCells.Add(sIndex);
            return newPath;
        }

        /// <summary>
        ///    3  4  6
        ///    __ __ __              ___
        ///  3|__|__|__|6           |{d}|   
        ///  2|__|{}|__|4           |___|  
        ///  1|__|__|__|3
        ///     1  2  3
        /// 
        /// total of (1+2+2+3+3+4+4+6=25)
        /// 0-5 ->  both x & y are towards the d
        /// 6-9 -> x toward y natural
        /// 10-13 -> y toward x natural
        /// 14-16 -> x toward y against
        /// 17-19 -> y toward x against
        /// 20-21 -> x natural y against
        /// 22-23 -> y natural x against
        /// 24 -> x against y against.
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="dIndex"></param>
        /// <returns></returns>
        private (int, int) getNextRandomCell((int x, int y) currentIndex, (int x, int y) dIndex)
        {
            int xDirection = dIndex.x == currentIndex.x ? 0 : dIndex.x > currentIndex.x ? 1 : -1;
            int yDirection = dIndex.y == currentIndex.y ? 0 : dIndex.y > currentIndex.y ? 1 : -1;
            int oppositeX = xDirection == 0 ? (random.Next(2) == 0 ? -1 : 1) : -1 * xDirection;
            int oppositeY = yDirection == 0 ? (random.Next(2) == 0 ? -1 : 1) : -1 * yDirection;

            int num = random.Next(25);

            if (num <= 5)
                return (currentIndex.x + xDirection, currentIndex.y + yDirection);
            else if (num <= 9)
                return (currentIndex.x + xDirection, currentIndex.y);
            else if (num <= 13)
                return (currentIndex.x, currentIndex.y + yDirection);
            else if (num <= 16)
                return (currentIndex.x + xDirection, currentIndex.y + oppositeY);
            else if (num <= 19)
                return (currentIndex.x + oppositeX, currentIndex.y + yDirection);
            else if (num <= 21)
                return (currentIndex.x, currentIndex.y + oppositeY);
            else if (num <= 23)
                return (currentIndex.x + oppositeX, currentIndex.y);
            else
                return (currentIndex.x + oppositeX, currentIndex.y + oppositeY);
        }
    }
}
