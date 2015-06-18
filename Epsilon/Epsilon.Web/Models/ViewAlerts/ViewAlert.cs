using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Web.Models.ViewAlerts
{
    public class ViewAlert
    {
        public const string TempDataKey = "TempDataViewAlerts";

        public string AlertStyle { get; set; }
        public string Message { get; set; }
        public bool Dismissable { get; set; }
    }
}
