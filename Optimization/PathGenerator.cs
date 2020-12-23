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

        public List<Path> GetInitialPaths()
        {
            List<Path> initialPaths = new List<Path>(SimulationData.Instance.initialPopulationSize);

            Path xMonotone = generateXMonotonePath(SimulationData.Instance.sourceCell, SimulationData.Instance.destinationCell);
            Path yMonotone = generateYMonotonePath(SimulationData.Instance.sourceCell, SimulationData.Instance.destinationCell);
            monotonePathLength = xMonotone.pathCells.Count;

            initialPaths.Add(xMonotone);
            initialPaths.Add(yMonotone);

            int randomPathsAmount = SimulationData.Instance.initialPopulationSize - 2;
            while (randomPathsAmount-- > 0)
                initialPaths.Add(generateRandomPath(SimulationData.Instance.sourceCell, SimulationData.Instance.destinationCell));

            return initialPaths;
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
            var currentIndex = sIndex;
            Path newPath = initializePath(sIndex);

            while (currentIndex.x < dIndex.x)
            {
                currentIndex = (currentIndex.x + 1, currentIndex.y);
                newPath.pathCells.Add(currentIndex);
            }

            while (currentIndex.x > dIndex.x)
            {
                currentIndex = (currentIndex.x - 1, currentIndex.y);
                newPath.pathCells.Add(currentIndex);
            }

            while (currentIndex.y < dIndex.y)
            {
                currentIndex = (currentIndex.x, currentIndex.y + 1);
                newPath.pathCells.Add(currentIndex);
            }

            while (currentIndex.x > dIndex.x)
            {
                currentIndex = (currentIndex.x, currentIndex.y - 1);
                newPath.pathCells.Add(currentIndex);
            }

            return newPath;
        }

        private Path generateYMonotonePath((int x, int y) sIndex, (int x, int y) dIndex)
        {
            var currentIndex = sIndex;
            Path newPath = initializePath(sIndex);

            while (currentIndex.y < dIndex.y)
            {
                currentIndex = (currentIndex.x, currentIndex.y + 1);
                newPath.pathCells.Add(currentIndex);
            }

            while (currentIndex.x > dIndex.x)
            {
                currentIndex = (currentIndex.x, currentIndex.y - 1);
                newPath.pathCells.Add(currentIndex);
            }

            while (currentIndex.x < dIndex.x)
            {
                currentIndex = (currentIndex.x + 1, currentIndex.y);
                newPath.pathCells.Add(currentIndex);
            }

            while (currentIndex.x > dIndex.x)
            {
                currentIndex = (currentIndex.x - 1, currentIndex.y);
                newPath.pathCells.Add(currentIndex);
            }

            return newPath;
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
