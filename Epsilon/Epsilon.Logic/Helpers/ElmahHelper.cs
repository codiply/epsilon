using Elmah;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public static class ElmahHelper
    {
        public static void Raise(Exception ex)
        {
            try
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            catch (Exception ignored)
            {
                ErrorLog.GetDefault(null).Log(new Error(ex));
            }
        }
    }
}
