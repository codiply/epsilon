using Epsilon.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Web.Models.ViewModels.PropertyInfo
{
    public class ViewPropertyViewModel
    {
        public ViewPropertyInfoModel PropertyInfo { get; set; }
        public bool ReturnToSummary { get; set; }
    }
}
