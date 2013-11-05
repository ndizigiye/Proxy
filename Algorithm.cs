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

        public static int RoundRobin()
        {
            if(index == 3) index = 0;
            index += 1;
            return index;
        }

        public static int Random()
        {
            Random rnd = new Random();
            index = rnd.Next(1,4);
            return index;
        }

        public static int AlgorithmChooser()
        {
            var AlgorithmValue = 0;

            switch (algorithmName)
            {
                case "roundRobin":
                    AlgorithmValue = RoundRobin();
                    break;
                case "random":
                    AlgorithmValue = Random();
                    break;
            }
            return AlgorithmValue;
        }
    }
}
