using System.Collections.Generic;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class DbAppSettingLabelCloudViewModel
    {
        public IList<string> Labels { get; set; }
        public string SelectedLabel { get; set; }
    }
}
