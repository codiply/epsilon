using Epsilon.Logic.Entities;
using System.Collections.Generic;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class DbAppSettingListViewModel
    {
        public IList<AppSetting> Settings { get; set; }

        public IList<string> AllLabels { get; set; }

        public string SelectedLabel { get; set; }
    }
}