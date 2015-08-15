namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IRandomWrapper
    {
        int Next(int minValue, int exclusiveMaxValue);

        double NextDouble();

        T Pick<T>(T[] items);
    }
}
