using Epsilon.Logic.Constants.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public class DbAppSettingDefaultValue : IDbAppSettingDefaultValue
    {
        public double AdminAlertSnoozePeriodInHours { get { return 24.0; } }
    }
}
