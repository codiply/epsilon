using Epsilon.Logic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    // NOTE: If you add a DbAppSettingValueType search for string 'EnumSwitch:DbAppSettingValueType' 
    //       for all places where you need to add a case in a switch statement.
    public enum DbAppSettingValueType
    {
        Boolean,
        Double,
        Frequency,
        Integer,
        String
    }
}
