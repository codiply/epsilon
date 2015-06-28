﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Interfaces
{
    public interface IAppSettingsDefaultValue
    {
        double CoinAccountSnapshotSnoozePeriodInHours { get; }
        int CoinAccountSnapshotNumberOfTransactionsThreshold { get; }
    }
}