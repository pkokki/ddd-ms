using System;
using System.Threading.Tasks;

namespace Mocks
{
    public static class Utils
    {
        private static readonly Random random = new Random();

        public static Task SimulateAsync(int minValue = 100, int maxValue = 300)
        {
            return Task.Delay(random.Next(minValue, maxValue));
        }
    }
}
