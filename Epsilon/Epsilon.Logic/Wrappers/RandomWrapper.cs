using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers
{
    public class RandomWrapper : IRandomWrapper
    {
        private Random _random = new Random();

        public int Next(int minValue, int exclusiveMaxValue)
        {
            return _random.Next(minValue, exclusiveMaxValue);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public T Pick<T>(T[] items)
        {
            return items[_random.Next(0, items.Length)];
        }
    }
}
