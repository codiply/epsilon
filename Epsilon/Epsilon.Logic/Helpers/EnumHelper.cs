using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public static class EnumHelper
    {
        public static EnumMemoizer<DbAppSettingValueType> DbAppSettingValueType =
            new EnumMemoizer<DbAppSettingValueType>(); 
    }
}
