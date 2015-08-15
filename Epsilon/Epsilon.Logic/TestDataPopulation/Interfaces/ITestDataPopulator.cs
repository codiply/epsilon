using System.Threading.Tasks;

namespace Epsilon.Logic.TestDataPopulation.Interfaces
{
    public interface ITestDataPopulator
    {
        Task Populate(string userId);
    }
}
