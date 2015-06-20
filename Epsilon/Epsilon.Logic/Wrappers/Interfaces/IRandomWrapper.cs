using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IRandomWrapper
    {
        int Next(int minValue, int exclusiveMaxValue);

        double NextDouble();
    }
}
