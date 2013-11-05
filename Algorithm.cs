using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxy
{
    class Algorithm
    {
        public static int index = 0;
        public static string algorithmName = "roundRobin";

        public static int RoundRobin(int NumberOfAvailableServers)
        {
            if (index == NumberOfAvailableServers) index = 0;
            index += 1;
            return index;
        }

        public static int Random(int NumberOfAvailableServers)
        {
            Random rnd = new Random();
            index = rnd.Next(1, NumberOfAvailableServers + 1);
            return index;
        }

        public static int AlgorithmChooser(int NumberOfAvailableServers)
        {
            var AlgorithmValue = 0;

            switch (algorithmName)
            {
                case "roundRobin":
                    AlgorithmValue = RoundRobin(NumberOfAvailableServers);
                    break;
                case "random":
                    AlgorithmValue = Random(NumberOfAvailableServers);
                    break;
            }
            return AlgorithmValue;
        }
    }
}
