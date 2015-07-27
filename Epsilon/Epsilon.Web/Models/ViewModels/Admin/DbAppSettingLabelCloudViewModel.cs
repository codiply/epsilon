using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class DbAppSettingLabelCloudViewModel
    {
        public IList<string> Labels { get; set; }
        public string SelectedLabel { get; set; }
    }
}
