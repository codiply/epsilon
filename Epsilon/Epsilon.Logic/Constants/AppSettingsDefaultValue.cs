using Epsilon.Logic.Constants.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public class AppSettingsDefaultValue : IAppSettingsDefaultValue
    {
        public double CoinAccountSnapshotSnoozePeriodInHours { get { return 24; } }
        public int CoinAccountSnapshotNumberOfTransactionsThreshold { get { return 16; } }
    }
}
