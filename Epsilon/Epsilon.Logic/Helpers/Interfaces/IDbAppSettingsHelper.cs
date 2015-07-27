using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms;
using Epsilon.Logic.Forms.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IDbAppSettingsHelper : IAppSettingsHelper
    {
        Task<IList<AppSetting>> GetAllAppSettingEntities();

        Task<IList<AppSetting>> GetAllAppSettingEntitiesForLabel(string label);

        Task<IList<string>> GetAllLabels();

        Task<AppSetting> GetAppSettingEntity(string id);

        Task Update(DbAppSettingForm form, string userId);
    }
}
