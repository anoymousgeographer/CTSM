using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSM.cls
{
    public class RandomModelClass
    {
        public static DateTime totalTime;
        public static Random random = new Random();
        public static int nextDiscrete(double[] probs)
        {
            double sum = 0.0;
            for (int i = 0; i < probs.Length; i++)
                sum += probs[i];

            double r = random.NextDouble() * sum;
            sum = 0.0;


            for (int i = 0; i < probs.Length; i++)
            {
                sum += probs[i];
                if (sum > r)
                    return i;
            }
            return probs.Length - 1;
        }
    }
}
