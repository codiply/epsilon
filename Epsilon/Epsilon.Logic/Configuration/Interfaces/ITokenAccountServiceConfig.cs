using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface ITokenAccountServiceConfig
    {
        TimeSpan SnapshotSnoozePeriod { get; }
        int SnapshotNumberOfTransactionsThreshold { get; }
    }
}
