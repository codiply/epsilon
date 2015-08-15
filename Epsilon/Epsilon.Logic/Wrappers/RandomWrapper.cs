using Epsilon.Logic.Wrappers.Interfaces;
using System;

namespace Epsilon.Logic.Wrappers
{
    public class RandomWrapper : IRandomWrapper
    {
        private readonly Random _random;

        public RandomWrapper()
        {
            _random = new Random();
        }

        public RandomWrapper(int seed)
        {
            _random = new Random(seed);
        }

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
