using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class DbAppSettingListViewModel
    {
        public IList<AppSetting> Settings { get; set; }

        public IList<string> AllLabels { get; set; }

        public string SelectedLabel { get; set; }
    }
}