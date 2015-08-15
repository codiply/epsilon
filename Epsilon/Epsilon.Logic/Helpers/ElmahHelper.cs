using Elmah;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class ElmahHelper : IElmahHelper
    {
        public void Raise(Exception ex)
        {
            try
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            catch (Exception)
            {
                RaiseWithoutContext(ex);
            }
        }

        private void RaiseWithoutContext(Exception ex)
        {
            try
            {
                ErrorLog.GetDefault(null).Log(new Error(ex));
            }
            catch (Exception) { }
        }
    }
}
