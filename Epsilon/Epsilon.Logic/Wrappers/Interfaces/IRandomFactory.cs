namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IRandomFactory
    {
        IRandomWrapper Create();
        IRandomWrapper Create(int seed);
    }
}
